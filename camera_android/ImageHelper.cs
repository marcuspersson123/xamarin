using System;
using Android.App;
using Uri = Android.Net.Uri;
using System.Collections.Generic;
using Android.Content.PM;
using Java.IO;
using Environment = Android.OS.Environment;
using Android.Content;
using Android.Provider;
using Android.Graphics;
using camera_android.Core;


namespace camera_android
{
	public class ImageHelper : Java.Lang.Object
	{
		public camera_android.Core.Image Image {
			get {return _image;  }

		}

		Image _image;

		public bool GenerateItem ()
		{
			bool success = false;
			File file = File;
			camera_android.Core.Image image = new camera_android.Core.Image ();

			try {
				Bitmap bitmap = BitmapFactory.DecodeFile (file.Path);
				image.Photo = bitmap;
				image.Latitude = "1.2";
				image.Longitude = "1.3";
				image.Note = "test";
				image.Time = "343434";
				_image = image;
				success = true;
			} catch (Exception e) {

			}
			return success;
		}

		public void persistToSqlite ()
		{
			ImageManager.SaveImage (_image);
		}

		bool _imageValid = false;

		public bool ImageValid {
			get { return _imageValid; }
			set { _imageValid = value; }
		}

		private Context _context;
		public File File {
			get { return _file; }

		}



		Java.IO.File _file;
		Java.IO.File _dir;
		private string _fileName;
		string _folderName;

		// TODO: base64
		/*
		public static string Base64EncodePicture(string fileName) {
			ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();  
			bitmap.compress(Bitmap.CompressFormat.PNG, 100, byteArrayOutputStream);
			byte[] byteArray = byteArrayOutputStream .toByteArray();
			string encoded = Base64.encodeToString(byteArray, Base64.DEFAULT);
			return encoded;
		}
		*/

		public ImageHelper (Context context)
		{
			_context = context;
		}

		public bool IsThereAnAppToTakePictures ()
		{
			Intent intent = new Intent (MediaStore.ActionImageCapture);
			IList<ResolveInfo> availableActivities = _context.PackageManager.QueryIntentActivities (intent, PackageInfoFlags.MatchDefaultOnly);
			return availableActivities != null && availableActivities.Count > 0;
		}

		public void CreateDirectoryForPictures (string folderName)
		{
			_folderName = folderName;
			_dir = new File (Environment.GetExternalStoragePublicDirectory (Environment.DirectoryPictures), _folderName);
			if (!_dir.Exists ()) {
				_dir.Mkdirs ();
			}
		}

		public void CreateRandomFilename ()
		{
			_fileName = String.Format ("myPhoto_{0}.jpg", Guid.NewGuid ());
		}

		public void createFile ()
		{
			_imageValid = false;
			_file = new File (_dir, _fileName);
		}

		public void publishToGallery ()
		{
			// make it available in the gallery
			Intent mediaScanIntent = new Intent (Intent.ActionMediaScannerScanFile);
			Uri contentUri = Uri.FromFile (_file);
			mediaScanIntent.SetData (contentUri);
			_context.SendBroadcast (mediaScanIntent);

		}

		public static byte[] GetBytes(Bitmap bitmap) {
			System.IO.MemoryStream stream = new System.IO.MemoryStream();
			bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 0, stream);
			return stream.ToArray();
		}

		// convert from byte array to bitmap
		public static Bitmap GetPhoto(byte[] image) {
			return BitmapFactory.DecodeByteArray(image, 0, image.Length);
		}

		public void deleteFile ()
		{
			_file.Delete ();
		}
	}
}

