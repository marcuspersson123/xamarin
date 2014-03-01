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
	}

	[Activity (Label = "camera_android", MainLauncher = true)]
	public class MainActivity : Activity
	{
		NonConfiguration _nonConfiguration;
		ImageView _imageView;
		Button _newMomentButton;
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

		void loadAndDisplayMoment ()
		{
			List<Moment> moments = (List<Moment>)MomentsManager.GetMoments ();
			if (moments.Count > 0) {
				_moment = moments [moments.Count - 1];
				ThreadPool.QueueUserWorkItem (o => { 
					MomentsManager.GetPhoto (_moment);

					RunOnUiThread (() => {
						displayMoment ();
					});
				}
				);
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
			_newMomentButton = FindViewById<Button> (Resource.Id.myButton);
			_imageView = FindViewById<ImageView> (Resource.Id.imageView1);


			if (LastNonConfigurationInstance != null) {
				_nonConfiguration = (NonConfiguration)LastNonConfigurationInstance;


			} 
				
			if (_moment != null) {
				displayMoment ();
			} else {
				loadAndDisplayMoment ();
			}

			if (IsThereAnAppToTakePictures ()) {
				_newMomentButton.Click += NewMomentButtonClick;
			} else {
				_newMomentButton.Text = "No camera!";
			}


		}

		bool _isChangingOrientation = false;

		public override Java.Lang.Object OnRetainNonConfigurationInstance ()
		{
			_isChangingOrientation = true;
			base.OnRetainNonConfigurationInstance ();
			return (Java.Lang.Object)_nonConfiguration;
		}

		private void NewMomentButtonClick (object sender, EventArgs eventArgs)
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
			_imageView.SetImageBitmap (_moment.Photo);
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			if (resultCode == Result.Ok) {
			
				_moment = new MomentsApp.Core.Moment ();

				try {
					//Bitmap bitmap = BitmapFactory.DecodeFile (_file.Path);
					Bitmap bitmap = BitmapFromFile (_file, 100, 100);

					_moment.Photo = bitmap;
					_moment.Latitude = "1.2";
					_moment.Longitude = "1.3";
					_moment.Note = "test";
					_moment.Time = "343434";
					MomentsManager.SaveMoment (_moment);
				} catch (Exception e) {

				}
			} 
		}

		private void SavePhotoButtonClick (object sender, EventArgs eventArgs)
		{
			ThreadPool.QueueUserWorkItem (o => MomentsManager.SaveMoment (_moment));
		}

		public static Bitmap BitmapFromFile (File file, int reqWidth, int reqHeight)
		{

			// First decode with inJustDecodeBounds=true to check dimensions
			BitmapFactory.Options options = new BitmapFactory.Options ();
			options.InJustDecodeBounds = true;
			BitmapFactory.DecodeFile (file.Path, options);

			// Calculate inSampleSize
			options.InSampleSize = CalculateInSampleSize (options, reqWidth, reqHeight);

			// Decode bitmap with inSampleSize set
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


