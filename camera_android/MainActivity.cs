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

namespace camera_android
{
	public class NonConfiguration {
	}

	[Activity (Label = "camera_android", MainLauncher = true)]
	public class MainActivity : Activity
	{
		NonConfiguration _nonConfiguration;
		ImageView _imageView;
		Button _newMomentButton;
		Java.IO.File _file;
		Java.IO.File _dir;
		camera_android.Core.Moment _moment;

		public void publishToGallery ()
		{
			// make it available in the gallery
			Intent mediaScanIntent = new Intent (Intent.ActionMediaScannerScanFile);
			Uri contentUri = Uri.FromFile (_file);
			mediaScanIntent.SetData (contentUri);
			SendBroadcast (mediaScanIntent);

		}

		public void persistToSqlite ()
		{
			MomentManager.Save (_moment);
		}

		public bool IsThereAnAppToTakePictures ()
		{
			Intent intent = new Intent (MediaStore.ActionImageCapture);
			IList<ResolveInfo> availableActivities = PackageManager.QueryIntentActivities (intent, PackageInfoFlags.MatchDefaultOnly);
			return availableActivities != null && availableActivities.Count > 0;
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Main);
			_newMomentButton = FindViewById<Button> (Resource.Id.myButton);
			_imageView = FindViewById<ImageView> (Resource.Id.imageView1);


			if (LastNonConfigurationInstance != null) {
				_nonConfiguration = (NonConfiguration)LastNonConfigurationInstance;
				if (_moment != null) {
					displayMoment ();
				}

			} 
				
			if (IsThereAnAppToTakePictures ()) {
				_newMomentButton.Click += NewMomentButtonClick;
			} else {
				_newMomentButton.Text = "No camera!";
			}


		}

		public override Java.Lang.Object OnRetainNonConfigurationInstance ()
		{
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

		void displayInImageView ()
		{
			_imageView.SetImageBitmap (_moment.Photo);
		}
			
		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			if (resultCode == Result.Ok) {
			
				_moment = new camera_android.Core.Moment ();

				try {
					Bitmap bitmap = BitmapFactory.DecodeFile (_file.Path);
					_moment.Photo = bitmap;
					_moment.Latitude = "1.2";
					_moment.Longitude = "1.3";
					_moment.Note = "test";
					_moment.Time = "343434";
				} catch (Exception e) {

				}
			} 
		}

		private void SavePhotoButtonClick (object sender, EventArgs eventArgs)
		{
			ThreadPool.QueueUserWorkItem (o => _image.persistToSqlite ());
		}
	}
}


