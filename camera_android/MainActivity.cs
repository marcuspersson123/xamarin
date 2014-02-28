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
	[Activity (Label = "camera_android", MainLauncher = true)]
	public class MainActivity : Activity
	{
		ImageView _imageView;
		ImageHelper _imageHelper;
		Button _button;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Main);
			_button = FindViewById<Button> (Resource.Id.myButton);
			_imageView = FindViewById<ImageView> (Resource.Id.imageView1);


			if (LastNonConfigurationInstance != null) {
				_imageHelper = (ImageHelper)LastNonConfigurationInstance;
				if (_imageHelper.ImageValid) {
					displayInImageView ();
				}

			} else {
				_imageHelper = new ImageHelper (this.ApplicationContext);
				_imageHelper.CreateDirectoryForPictures ("xamarin_images");
			}
				
			if (_imageHelper.IsThereAnAppToTakePictures ()) {
				_button.Click += TakeAPictureButtonClick;
			} else {
				_button.Text = "No camera!";
			}

			if (_imageHelper.Image == null) {
				_imageHelper.loadImage (1);
			} else if (_imageHelper.Image.Photo == null) {
				_imageHelper.loadImage (1);
				
			}
			if (_imageHelper.Image != null && _imageHelper.Image.Photo != null) {
				displayInImageView ();
			}
		}

		public override Java.Lang.Object OnRetainNonConfigurationInstance ()
		{
			base.OnRetainNonConfigurationInstance ();
			return (Java.Lang.Object)_imageHelper;
		}

		private void TakeAPictureButtonClick (object sender, EventArgs eventArgs)
		{
			_imageHelper.CreateRandomFilename ();
			_imageHelper.createFile ();
			Intent intent = new Intent (MediaStore.ActionImageCapture);
			intent.PutExtra (MediaStore.ExtraOutput, Uri.FromFile (_imageHelper.File));
			StartActivityForResult (intent, 0);
		}

		void displayInImageView ()
		{
			// display in ImageView. We will resize the bitmap to fit the display
			// Loading the full sized image will consume to much memory 
			// and cause the application to crash.
			int height = (int)(Resources.DisplayMetrics.HeightPixels * 0.6);
			int width = (int)(Resources.DisplayMetrics.WidthPixels * 0.8);
			File file = _imageHelper.File;
			if (height < width) {
				int temp = height;
				height = width;
				width = temp;
			}
			//using (Bitmap bitmap = file.Path.LoadAndResizeBitmap (width, height)) {
			_imageView.SetImageBitmap (_imageHelper.Image.Photo);
			//}
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			if (resultCode == Result.Ok) {
				_imageHelper.ImageValid = true;
				bool success = _imageHelper.GenerateItem ();
				if (success) {
					_imageHelper.publishToGallery ();
					ThreadPool.QueueUserWorkItem (o => _imageHelper.persistToSqlite ());

					this.displayInImageView ();
				}
			} else {
				_imageHelper.deleteFile ();
			}
		}
	}
}


