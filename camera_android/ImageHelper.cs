using System;
using Android.App;
using Uri = Android.Net.Uri;
using System.Collections.Generic;
using Android.Content.PM;
using Java.IO;
using Environment = Android.OS.Environment;
using Android.Content;
using Android.Provider;


namespace camera_android
{
	public class ImageHelper : Java.Lang.Object
	{
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

		public void deleteFile ()
		{
			_file.Delete ();
		}
	}
}

