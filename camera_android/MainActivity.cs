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
using Facebook;

//using System.IO;
using Android.Locations;
using System.Globalization;


namespace MomentsApp
{

	public class NonConfiguration : Java.Lang.Object
	{
		public Moment _moment;
		public Bitmap _bitmap;
		public byte[] _photoBytes;
		public bool _isLoggedIn;
		public string _accessToken;
		public FacebookClient _fb;
		public string _profileName;
	}

	[Activity (Label = "Moments App", MainLauncher = true)]
	public class MainActivity : Activity
	{
		NonConfiguration _nonConfiguration;
		ImageView _photoImageView;
		EditText _noteEditText;
		ImageButton _shareImageButton;
		ImageButton _newImageButton;
		ImageButton _saveImageButton;
		ImageButton _deleteImageButton;
		ImageButton _historyImageButton;
		ImageButton _currentLocationImageButton;
		TextView _userTextView;
		Button _loginImageButton;

		TextView _timeTextView;
		Java.IO.File _file;
		Java.IO.File _dir;
	
		string AppId = "599862353421638";
		private const string ExtendedPermissions = "user_about_me,read_stream,publish_stream";

		LocationManager _locationManager;
		string _locationProvider;

		/*
		public void publishToGallery ()
		{
			// make it available in the gallery
			Intent mediaScanIntent = new Intent (Intent.ActionMediaScannerScanFile);
			Uri contentUri = Uri.FromFile (_file);
			mediaScanIntent.SetData (contentUri);
			SendBroadcast (mediaScanIntent);

		}
		*/

		public bool IsThereAnAppToTakePictures ()
		{
			Intent intent = new Intent (MediaStore.ActionImageCapture);
			IList<ResolveInfo> availableActivities = PackageManager.QueryIntentActivities (intent, PackageInfoFlags.MatchDefaultOnly);
			return availableActivities != null && availableActivities.Count > 0;
		}

		void loadAndDisplayMoment (int id)
		{
			_nonConfiguration._moment = MomentsManager.GetMoment (id);
			if (_nonConfiguration._moment != null) {
				//_moment = moments [moments.Count - 1];
				ThreadPool.QueueUserWorkItem (o => { 
					_nonConfiguration._photoBytes = MomentsManager.GetPhotoBytes (_nonConfiguration._moment);
					RunOnUiThread (() => {
						if (_nonConfiguration._bitmap != null) {
							_nonConfiguration._bitmap.Recycle ();
						}
						_nonConfiguration._bitmap = Android.Graphics.BitmapFactory.DecodeByteArray (_nonConfiguration._photoBytes, 0, _nonConfiguration._photoBytes.Length);
						UpdateUI ();
						_saveImageButton.Visibility = ViewStates.Visible;
					});
				}
				);
			} else {
				_saveImageButton.Visibility = ViewStates.Gone;
			}
		}

		protected override void OnDestroy ()
		{
			if (!_isChangingOrientation) {
				if (_nonConfiguration._bitmap != null) {
					_nonConfiguration._bitmap.Recycle ();
				}
			}
			base.OnDestroy ();
		}

		void LoginButtonClick (object sender, EventArgs e)
		{
			//  http://components.xamarin.com/view/facebook-sdk
			var webAuth = new Intent (this, typeof(FBWebViewAuthActivity));
			webAuth.PutExtra ("AppId", AppId);
			webAuth.PutExtra ("ExtendedPermissions", ExtendedPermissions);
			StartActivityForResult (webAuth, 2);
		}

		void ShareButtonClick (object sender, EventArgs eventArgs)
		{
			_nonConfiguration._fb.PostCompleted += (o, e) => {
				if (e.Cancelled || e.Error != null) {
					Toast.MakeText(this, "Sharing failed!", ToastLength.Long).Show();
					return;
				}
				RunOnUiThread (() => {
					Toast.MakeText(this, "Moment is shared!", ToastLength.Long).Show();
				});


				var result = e.GetResultData ();
			};

			var parameters = new Dictionary<string, object> ();
			parameters ["message"] = "Photo was taken: "+_nonConfiguration._moment.Time+", at location: " + _nonConfiguration._moment.Latitude+","+_nonConfiguration._moment.Longitude +  " , note: " + _nonConfiguration._moment.Note;
			parameters ["file"] = new FacebookMediaObject {
				ContentType = "image/jpeg",
				FileName = "image.jpeg"
			}.SetValue (_nonConfiguration._photoBytes);
			Toast.MakeText(this, "Sending moment...", ToastLength.Long).Show();
			_nonConfiguration._fb.PostTaskAsync ("me/photos", parameters);
		}
			
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Main);

			_photoImageView = FindViewById<ImageView> (Resource.Id.photoImageView);
			_noteEditText = FindViewById<EditText> (Resource.Id.noteEditText);
			_saveImageButton = FindViewById<ImageButton> (Resource.Id.saveButton);
			_deleteImageButton = FindViewById<ImageButton> (Resource.Id.deleteButton);
			_shareImageButton = FindViewById<ImageButton> (Resource.Id.shareButton);
			_newImageButton = FindViewById<ImageButton> (Resource.Id.newButton);
			_timeTextView = FindViewById<TextView> (Resource.Id.timeTextView);
			_historyImageButton = FindViewById<ImageButton> (Resource.Id.historyButton);
			_userTextView = FindViewById<TextView> (Resource.Id.userTextView);
			_loginImageButton = FindViewById<Button> (Resource.Id.loginButton);
			_currentLocationImageButton = FindViewById<ImageButton> (Resource.Id.currentLocationButton);

			if (LastNonConfigurationInstance != null) {
				_nonConfiguration = (NonConfiguration)LastNonConfigurationInstance;
			} else {
				_nonConfiguration = new NonConfiguration ();
			}

			//_deleteButton.Visibility = ViewStates.Gone;

			_deleteImageButton.Click += DeleteButtonClick;
			_shareImageButton.Click += ShareButtonClick;
			_historyImageButton.Click += HistoryButtonClick;
			_loginImageButton.Click += LoginButtonClick;
			_saveImageButton.Click += SaveButtonClick;
			_currentLocationImageButton.Click += CurrentLocationButtonClick;

			_dir = new File (Environment.GetExternalStoragePublicDirectory (Environment.DirectoryPictures), "xamarin_temporary");
			_file = new File (_dir, "temporary_photo");
			if (!_dir.Exists ()) {
				_dir.Mkdirs ();
			}


			if (IsThereAnAppToTakePictures ()) {
				_newImageButton.Click += NewButtonClick;
			} else {
				_newImageButton.Enabled = false;
				Toast.MakeText(this, "No camera!", ToastLength.Long).Show();
			}



			UpdateUI ();

		}

		bool _isChangingOrientation = false;

		public override Java.Lang.Object OnRetainNonConfigurationInstance ()
		{
			_isChangingOrientation = true;
			base.OnRetainNonConfigurationInstance ();
			return (Java.Lang.Object)_nonConfiguration;
		}

		private void DeleteButtonClick (object sender, EventArgs eventArgs)
		{

			ThreadPool.QueueUserWorkItem (o => {
				MomentsManager.DeleteMoment (_nonConfiguration._moment.ID);
				RunOnUiThread (() => {
					_nonConfiguration._moment = null;
					Toast.MakeText(this, "Deleted!", ToastLength.Long).Show();
					UpdateUI ();
				});
			});
		}

		private void NewButtonClick (object sender, EventArgs eventArgs)
		{

			Intent intent = new Intent (MediaStore.ActionImageCapture);
			intent.PutExtra (MediaStore.ExtraOutput, Uri.FromFile (_file));
			StartActivityForResult (intent, 0);
		}

		private Location GetLastLocation ()
		{
			_locationManager = GetSystemService (Context.LocationService) as LocationManager;
			Criteria locationCriteria = new Criteria ();

			locationCriteria.Accuracy = Accuracy.Coarse;
			locationCriteria.PowerRequirement = Power.Medium;

			_locationProvider = _locationManager.GetBestProvider (locationCriteria, true);
			if (_locationProvider != null) {
				return _locationManager.GetLastKnownLocation (_locationProvider);
			}
			return null;
		}

		private void CurrentLocationButtonClick(object sender, EventArgs eventArgs) {
			Location lastLocation = GetLastLocation ();

			if (lastLocation != null) {
				double latitude = lastLocation.Latitude;
				double longitude = lastLocation.Longitude;
				string latitudeString = latitude.ToString ().Replace (",", ".");
				string longitudeString = longitude.ToString ().Replace (",", ".");
				string locationString = "geo:" + latitudeString + ", " + longitudeString + "?z=15";
				var geoUri = Android.Net.Uri.Parse (locationString);
				var mapIntent = new Intent (Intent.ActionView, geoUri);
				StartActivity (mapIntent);	
			} else {
				Toast.MakeText(this, "Location unavailable", ToastLength.Long).Show();
			}
		}

		private void HistoryButtonClick (object sender, EventArgs eventArgs)
		{
			StartActivityForResult (typeof(MomentsActivity), 1);
		}

		void UpdateUI ()
		{
			if (_nonConfiguration._moment != null) {
				//_photoImageView.Visibility = ViewStates.Visible;
				_saveImageButton.Visibility = ViewStates.Visible;

				if (_nonConfiguration._moment.ID >= 0) {
					_deleteImageButton.Visibility = ViewStates.Visible;
				} else {
					_deleteImageButton.Visibility = ViewStates.Gone;
				}
				if (_nonConfiguration._isLoggedIn) {
					_shareImageButton.Visibility = ViewStates.Visible;
				} else {
					_shareImageButton.Visibility = ViewStates.Gone;
				}

					_photoImageView.SetImageBitmap (_nonConfiguration._bitmap);

				_noteEditText.Text = _nonConfiguration._moment.Note;
				DateTime momentTime;
				bool isDateTime = DateTime.TryParse (_nonConfiguration._moment.Time, out momentTime);
				string formattedDateTime;
				if (isDateTime) {
					formattedDateTime = momentTime.ToString ("dddd d\\/M yyyy HH:mm");
				} else {
					formattedDateTime = "Not a DateTime";
				}
				_timeTextView.Text = formattedDateTime;
				_noteEditText.Enabled = true;
			} else {
				_userTextView.Text = "";
				_timeTextView.Text = "";
				_noteEditText.Text = "";
				_noteEditText.Enabled = false;
				//			_mapImageView.Visibility = ViewStates.Gone;
				_photoImageView.SetBackgroundResource(Android.Resource.Drawable.IcMenuGallery);
				_saveImageButton.Visibility = ViewStates.Gone;
				_shareImageButton.Visibility = ViewStates.Gone;
				_deleteImageButton.Visibility = ViewStates.Gone;

			}
			if (MomentsManager.GetMoments ().Count > 0) {
				_historyImageButton.Visibility = ViewStates.Visible;

			} else {
				_historyImageButton.Visibility = ViewStates.Gone;
			}
			if (_nonConfiguration._isLoggedIn) {
				_loginImageButton.Visibility = ViewStates.Gone;
				_userTextView.Text = _nonConfiguration._profileName;
			} else {
				_loginImageButton.Visibility = ViewStates.Visible;
				_userTextView.Text = "";
			}
		}

		void NewMoment ()
		{
			_nonConfiguration._moment = new MomentsApp.Core.Moment ();
			DateTime now = DateTime.Now;
			_nonConfiguration._moment.Time = now.ToString ("yyyy-MM-dd HH:mm:ss");

			if (_nonConfiguration._bitmap != null) {
				_nonConfiguration._bitmap.Recycle ();
			}

			_nonConfiguration._bitmap = BitmapFromFile (_file, 400, 400);
			using (System.IO.MemoryStream stream = new System.IO.MemoryStream ()) {
				_nonConfiguration._bitmap.Compress (Bitmap.CompressFormat.Png, 0, stream);
				_nonConfiguration._photoBytes = stream.ToArray ();
			}

			Location lastLocation = GetLastLocation ();

			string latitudeString = "0.0";
			string longitudeString = "0.0";
			if (lastLocation != null) {
				double latitude = lastLocation.Latitude;
				double longitude = lastLocation.Longitude;
				 latitudeString = latitude.ToString ().Replace (",", ".");
				 longitudeString = longitude.ToString ().Replace (",", ".");
					
			} else {
				Toast.MakeText(this, "Location unavailable", ToastLength.Long).Show();
			}

			_nonConfiguration._moment.Latitude = latitudeString;
			_nonConfiguration._moment.Longitude = longitudeString;
			UpdateUI ();
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			switch (requestCode) {
			case 0:  // from camera
				if (resultCode == Result.Ok) {
					NewMoment ();

				} else {
					if (_nonConfiguration._moment == null) {
						_saveImageButton.Visibility = ViewStates.Gone;
					} else {
						_saveImageButton.Visibility = ViewStates.Visible;
					}
				}
				break;
			case 1: // from moments listview
				if (resultCode == Result.Ok) {
					int id = (int)data.GetLongExtra ("ID", -1);
					loadAndDisplayMoment (id);
				}
				break;
			case 2:  // from facebook login
				if (data != null) {
					_nonConfiguration._accessToken = data.GetStringExtra ("AccessToken");
					string userId = data.GetStringExtra ("UserId");
					string error = data.GetStringExtra ("Exception");

					_nonConfiguration._fb = new FacebookClient (_nonConfiguration._accessToken);

					_nonConfiguration._fb.GetTaskAsync ("me").ContinueWith (t => {
						if (!t.IsFaulted) {
							var result = (IDictionary<string, object>)t.Result;

							string profilePictureUrl = string.Format ("https://graph.facebook.com/{0}/picture?type={1}&access_token={2}", userId, "square", _nonConfiguration._accessToken);
							_nonConfiguration._profileName = "FB profile: " + (string)result ["name"];
							_nonConfiguration._isLoggedIn = true;
							RunOnUiThread (() => {
								Toast.MakeText(this, "Logged in!", ToastLength.Long).Show();
								UpdateUI ();
							});


						} else {
							_nonConfiguration._isLoggedIn = false;
							RunOnUiThread (() => {
								Toast.MakeText(this, "Not logged in!", ToastLength.Long).Show();
								//Alert ("Failed to Log In", "Reason: " + error, false, (res) => {
								//});
							});
						}
					});
				}
				break;
			}
		}

		private void SaveButtonClick (object sender, EventArgs eventArgs)
		{
			_nonConfiguration._moment.Note = _noteEditText.Text;

			ThreadPool.QueueUserWorkItem (o => {
				int id = MomentsManager.SaveMoment (_nonConfiguration._moment, _nonConfiguration._photoBytes);
				RunOnUiThread (() => {
					Toast.MakeText(this, "Saved", ToastLength.Long).Show();
					_nonConfiguration._moment.ID = id;

					UpdateUI();
				});
			});
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

		public void Alert (string title, string message, bool CancelButton, Action<Result> callback)
		{
			AlertDialog.Builder builder = new AlertDialog.Builder (this);
			builder.SetTitle (title);
			builder.SetIcon (Android.Resource.Drawable.IcDialogAlert);
			builder.SetMessage (message);

			builder.SetPositiveButton ("Ok", (sender, e) => {
				callback (Result.Ok);
			});

			if (CancelButton) {
				builder.SetNegativeButton ("Cancel", (sender, e) => {
					callback (Result.Canceled);
				});
			}

			builder.Show ();
		}
	}
}


