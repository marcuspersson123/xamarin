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

namespace camera_android
{
	[Activity (Label = "camera_android", MainLauncher = true)]
	public class MainActivity : Activity
	{
		Java.IO.File _file;
		Java.IO.File _dir;
		ImageView _imageView;


		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.Main);

			if (IsThereAnAppToTakePictures())
			{
				CreateDirectoryForPictures();

				Button button = FindViewById<Button>(Resource.Id.myButton);
				_imageView = FindViewById<ImageView>(Resource.Id.imageView1);

				button.Click += TakeAPicture;
			}
		}

		private bool IsThereAnAppToTakePictures()
		{
			Intent intent = new Intent(MediaStore.ActionImageCapture);
			IList<ResolveInfo> availableActivities = PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
			return availableActivities != null && availableActivities.Count > 0;
		}

		private void CreateDirectoryForPictures()
		{
			_dir = new File(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures), "CameraAppDemo");
			if (!_dir.Exists())
			{
				_dir.Mkdirs();
			}
		}

		private void TakeAPicture(object sender, EventArgs eventArgs)
		{
			Intent intent = new Intent(MediaStore.ActionImageCapture);

			_file = new File(_dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));

			intent.PutExtra(MediaStore.ExtraOutput, Uri.FromFile(_file));

			StartActivityForResult(intent, 0);
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			// make it available in the gallery
			Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
			Uri contentUri = Uri.FromFile(_file);
			mediaScanIntent.SetData(contentUri);
			SendBroadcast(mediaScanIntent);

			// display in ImageView. We will resize the bitmap to fit the display
			// Loading the full sized image will consume to much memory 
			// and cause the application to crash.
			int height = _imageView.Height;
			int width = Resources.DisplayMetrics.WidthPixels;
			using (Bitmap bitmap = _file.Path.LoadAndResizeBitmap(width, height))
			{
				_imageView.SetImageBitmap(bitmap);
			}
		}
	}
}


