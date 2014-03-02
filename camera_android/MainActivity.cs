using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Android.Provider;
using Uri = Android.Net.Uri;
using System.Collections.Generic;
using Android.Content.PM;
using Java.IO;
using Environment = Android.OS.Environment;
using System.Threading;
using MomentsApp.Core;
using Android.Content.Res;

namespace MomentsApp
{
	public class NonConfiguration : Java.Lang.Object
	{
		Moment _moment;
		public Moment Moment {

			get { return _moment; }
			set { _moment = value; }
		}
	}

	[Activity (Label = "camera_android", MainLauncher = true)]
	public class MainActivity : Activity
	{
		NonConfiguration _nonConfiguration;
		ImageView _photoImageView;
		ImageView _mapImageView;
		EditText _noteEditText;
		Button _shareButton;
		Button _newButton;
		Button _saveButton;
		Button _deleteButton;
		Button _historyButton;
		TextView _timeTextView;
		Java.IO.File _file;
		Java.IO.File _dir;
		MomentsApp.Core.Moment _moment;

		public void publishToGallery ()
		{
			// make it available in the gallery
			Intent mediaScanIntent = new Intent (Intent.ActionMediaScannerScanFile);
			Uri contentUri = Uri.FromFile (_file);
			mediaScanIntent.SetData (contentUri);
			SendBroadcast (mediaScanIntent);

		}

		public bool IsThereAnAppToTakePictures ()
		{
			Intent intent = new Intent (MediaStore.ActionImageCapture);
			IList<ResolveInfo> availableActivities = PackageManager.QueryIntentActivities (intent, PackageInfoFlags.MatchDefaultOnly);
			return availableActivities != null && availableActivities.Count > 0;
		}

		void loadAndDisplayLastMoment ()
		{
			List<Moment> moments = (List<Moment>)MomentsManager.GetMoments ();
			if (moments.Count > 0) {
				_moment = moments [moments.Count - 1];
				ThreadPool.QueueUserWorkItem (o => { 
					MomentsManager.GetPhoto (_moment);

					RunOnUiThread (() => {
						displayMoment ();
						_saveButton.Visibility = ViewStates.Visible;
					});
				}
				);
			} else {
				_saveButton.Visibility = ViewStates.Gone;
			}
		}

		protected override void OnDestroy ()
		{
			if (!_isChangingOrientation) {
				if (_moment != null && _moment.Photo != null) {
					_moment.Photo.Recycle ();
				}
			}
			base.OnDestroy ();
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Main);

			_photoImageView = FindViewById<ImageView> (Resource.Id.photoImageView);
			_mapImageView = FindViewById<ImageView> (Resource.Id.mapImageView);
			_noteEditText = FindViewById<EditText> (Resource.Id.noteEditText);
			_saveButton = FindViewById<Button> (Resource.Id.saveButton);
			_deleteButton = FindViewById<Button> (Resource.Id.deleteButton);
			_shareButton = FindViewById<Button> (Resource.Id.shareButton);
			_newButton = FindViewById<Button> (Resource.Id.newButton);
			_timeTextView = FindViewById<TextView> (Resource.Id.timeTextView);
			_historyButton = FindViewById<Button> (Resource.Id.historyButton);


			if (LastNonConfigurationInstance != null) {
				_nonConfiguration = (NonConfiguration)LastNonConfigurationInstance;
				_moment = _nonConfiguration.Moment;

			} 

			_deleteButton.Visibility = ViewStates.Gone;
				
			if (_moment != null) {
				displayMoment ();
			} else {
				loadAndDisplayLastMoment ();

			}

			if (IsThereAnAppToTakePictures ()) {
				_newButton.Click += NewButtonClick;
			} else {
				_newButton.Text = "No camera!";
			}

			_saveButton.Click += SaveButtonClick;


		}

		bool _isChangingOrientation = false;

		public override Java.Lang.Object OnRetainNonConfigurationInstance ()
		{
			_isChangingOrientation = true;
			base.OnRetainNonConfigurationInstance ();
			_nonConfiguration = new NonConfiguration ();
			_nonConfiguration.Moment = _moment;
			return (Java.Lang.Object)_nonConfiguration;
		}

		private void NewButtonClick (object sender, EventArgs eventArgs)
		{
			_dir = new File (Environment.GetExternalStoragePublicDirectory (Environment.DirectoryPictures), "xamarin_temporary");
			_file = new File (_dir, "temporary_photo");
			if (!_dir.Exists ()) {
				_dir.Mkdirs ();
			}
			Intent intent = new Intent (MediaStore.ActionImageCapture);
			intent.PutExtra (MediaStore.ExtraOutput, Uri.FromFile (_file));
			StartActivityForResult (intent, 0);
		}

		void displayMoment ()
		{
			_photoImageView.SetImageBitmap (_moment.Photo);
			_noteEditText.Text = _moment.Note;
			DateTime momentTime;
			bool isDateTime = DateTime.TryParse (_moment.Time, out momentTime);
			string formattedDateTime;
			if (isDateTime) {
				formattedDateTime = momentTime.ToString ("dddd d\\/M yyyy HH:mm");
			} else {
				formattedDateTime = "Not a DateTime";
			}
			_timeTextView.Text = formattedDateTime;
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			if (resultCode == Result.Ok) {
				if (_moment != null) {
					if (_moment.Photo != null) {
						_moment.Photo.Recycle ();
						Java.Lang.JavaSystem.Gc ();
					}
				}
				_moment = new MomentsApp.Core.Moment ();
				DateTime now = DateTime.Now;
				//_moment.Time = now.ToString ("dddd d\\/M yyyy");
				_moment.Time = now.ToString ("yyyy-MM-dd HH:mm:ss");


				// gör att _moment.Photo får intrptr fel
				//using (var bitmap = BitmapFromFile (_file, 400, 400)) {
				//		_moment.Photo = bitmap;
				//}

				var bitmap = BitmapFromFile (_file, 400, 400);
				_moment.Photo = bitmap;


				_moment.Latitude = "1.2";
				_moment.Longitude = "1.3";
				//_moment.Note = "detta är ett test";
				//_moment.Time = "343434";

				_saveButton.Visibility = ViewStates.Visible;
				displayMoment ();


			} else {
				if (_moment == null) {
					_saveButton.Visibility = ViewStates.Gone;
				} else {
					_saveButton.Visibility = ViewStates.Visible;
				}
			}
		}

		private void SaveButtonClick (object sender, EventArgs eventArgs)
		{
			_moment.Note = _noteEditText.Text;

			ThreadPool.QueueUserWorkItem (o => MomentsManager.SaveMoment (_moment));
		}

		public static Bitmap BitmapFromFile (File file, int reqWidth, int reqHeight)
		{
		
			BitmapFactory.Options options = new BitmapFactory.Options ();
			options.InJustDecodeBounds = true;
			using (var dispose = BitmapFactory.DecodeFile (file.Path, options)) {

			}
			options.InSampleSize = CalculateInSampleSize (options, reqWidth, reqHeight);
			options.InJustDecodeBounds = false;
			return BitmapFactory.DecodeFile (file.Path, options);
		}

		public static int CalculateInSampleSize (
			BitmapFactory.Options options, int reqWidth, int reqHeight)
		{
			// Raw height and width of image
			int height = options.OutHeight;
			int width = options.OutWidth;
			int inSampleSize = 1;

			if (height > reqHeight || width > reqWidth) {

				int halfHeight = height / 2;
				int halfWidth = width / 2;

				// Calculate the largest inSampleSize value that is a power of 2 and keeps both
				// height and width larger than the requested height and width.
				while ((halfHeight / inSampleSize) > reqHeight
				       && (halfWidth / inSampleSize) > reqWidth) {
					inSampleSize *= 2;
				}
			}

			return inSampleSize;
		}
	}
}


