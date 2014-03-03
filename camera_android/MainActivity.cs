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
		//MomentsApp.Core.Moment _moment;
		string AppId = "599862353421638";
		private const string ExtendedPermissions = "user_about_me,read_stream,publish_stream";

		//string accessToken;

		string lastMessageId;

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

		void loadAndDisplayMoment (int id)
		{
			//List<Moment> moments = (List<Moment>)MomentsManager.GetMoments ();
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
					return;
				}
				var result = e.GetResultData ();
			};

			var parameters = new Dictionary<string, object> ();
			parameters ["message"] = "-- "+_nonConfiguration._moment.Time+" --"+ _nonConfiguration._moment.Note;
			parameters ["file"] = new FacebookMediaObject {
				ContentType = "image/jpeg",
				FileName = "image.jpeg"
			}.SetValue (_nonConfiguration._photoBytes);
			_nonConfiguration._fb.PostTaskAsync ("me/photos", parameters);
		}

		TextView _userTextView;
		Button _loginButton;

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
			_userTextView = FindViewById<TextView> (Resource.Id.userTextView);
			_loginButton = FindViewById<Button> (Resource.Id.loginButton);

			if (LastNonConfigurationInstance != null) {
				_nonConfiguration = (NonConfiguration)LastNonConfigurationInstance;
				//_moment = _nonConfiguration._moment;

			} else {
				_nonConfiguration = new NonConfiguration ();
			}

			_deleteButton.Visibility = ViewStates.Gone;
			_shareButton.Click += ShareButtonClick;
			_historyButton.Click += HistoryButtonClick;
			_loginButton.Click += LoginButtonClick;
			_saveButton.Click += SaveButtonClick;

			_dir = new File (Environment.GetExternalStoragePublicDirectory (Environment.DirectoryPictures), "xamarin_temporary");
			_file = new File (_dir, "temporary_photo");
			if (!_dir.Exists ()) {
				_dir.Mkdirs ();
			}


			if (IsThereAnAppToTakePictures ()) {
				_newButton.Click += NewButtonClick;
			} else {
				_newButton.Text = "No camera!";
			}

			UpdateUI ();

		}

		bool _isChangingOrientation = false;

		public override Java.Lang.Object OnRetainNonConfigurationInstance ()
		{
			_isChangingOrientation = true;
			base.OnRetainNonConfigurationInstance ();
			//_nonConfiguration = new NonConfiguration ();
			//_nonConfiguration.Moment = _moment;
			return (Java.Lang.Object)_nonConfiguration;
		}

		private void NewButtonClick (object sender, EventArgs eventArgs)
		{

			Intent intent = new Intent (MediaStore.ActionImageCapture);
			intent.PutExtra (MediaStore.ExtraOutput, Uri.FromFile (_file));
			StartActivityForResult (intent, 0);
		}

		private void HistoryButtonClick (object sender, EventArgs eventArgs)
		{
			StartActivityForResult (typeof(MomentsActivity), 1);
		}

		void UpdateUI ()
		{
			if (_nonConfiguration._moment != null) {
				_mapImageView.Visibility = ViewStates.Visible;
				_photoImageView.Visibility = ViewStates.Visible;
				_saveButton.Visibility = ViewStates.Visible;
				if (_nonConfiguration._isLoggedIn) {
					_shareButton.Visibility = ViewStates.Visible;
				} else {
					_shareButton.Visibility = ViewStates.Invisible;
				}
				//if (_nonConfiguration._bitmap != null) {
					_photoImageView.SetImageBitmap (_nonConfiguration._bitmap);
				//}
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
				_noteEditText.Visibility = ViewStates.Visible;
			} else {
				_userTextView.Text = "";
				_timeTextView.Text = "";
				_noteEditText.Visibility = ViewStates.Invisible;
				_mapImageView.Visibility = ViewStates.Invisible;
				_photoImageView.Visibility = ViewStates.Invisible;
				_saveButton.Visibility = ViewStates.Invisible;
				_shareButton.Visibility = ViewStates.Invisible;

			}
			if (MomentsManager.GetMoments ().Count > 0) {
				_historyButton.Visibility = ViewStates.Visible;
			} else {
				_historyButton.Visibility = ViewStates.Invisible;
			}
			if (_nonConfiguration._isLoggedIn) {
				_loginButton.Visibility = ViewStates.Invisible;
				_userTextView.Text = _nonConfiguration._profileName;
			} else {
				_loginButton.Visibility = ViewStates.Visible;
				_userTextView.Text = "";
			}
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			switch (requestCode) {
			case 0:  // from camera
				if (resultCode == Result.Ok) {

					_nonConfiguration._moment = new MomentsApp.Core.Moment ();
					DateTime now = DateTime.Now;
					//_moment.Time = now.ToString ("dddd d\\/M yyyy");
					_nonConfiguration._moment.Time = now.ToString ("yyyy-MM-dd HH:mm:ss");

					// från stackoverflow
					//Bitmap bmp = (Bitmap) data.getExtras().get("data");
					//img.setImageBitmap(bmp);

					if (_nonConfiguration._bitmap != null) {
						_nonConfiguration._bitmap.Recycle ();
					}

					//_photoBytes = _file.ToArray<byte[]> ();
					_nonConfiguration._bitmap = BitmapFromFile (_file, 400, 400);
					using (System.IO.MemoryStream stream = new System.IO.MemoryStream ()) {
						_nonConfiguration._bitmap.Compress (Bitmap.CompressFormat.Png, 0, stream);
						_nonConfiguration._photoBytes = stream.ToArray ();
					}

					//_photoBytes = _bitmap.ToArray<byte> ();

					_nonConfiguration._moment.Latitude = "1.2";
					_nonConfiguration._moment.Longitude = "1.3";
					//_moment.Note = "detta är ett test";
					//_moment.Time = "343434";

					//					_saveButton.Visibility = ViewStates.Visible;
					UpdateUI ();


				} else {
					if (_nonConfiguration._moment == null) {
						_saveButton.Visibility = ViewStates.Gone;
					} else {
						_saveButton.Visibility = ViewStates.Visible;
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

					//ImageView imgUser = FindViewById<ImageView> (Resource.Id.imgUser);
					//serName = FindViewById<TextView> (Resource.Id.txtvUserName);

					_nonConfiguration._fb.GetTaskAsync ("me").ContinueWith (t => {
						if (!t.IsFaulted) {

							var result = (IDictionary<string, object>)t.Result;

							// available picture types: square (50x50), small (50xvariable height), large (about 200x variable height) (all size in pixels)
							// for more info visit http://developers.facebook.com/docs/reference/api
							string profilePictureUrl = string.Format ("https://graph.facebook.com/{0}/picture?type={1}&access_token={2}", userId, "square", _nonConfiguration._accessToken);
							//var bm = BitmapFactory.DecodeStream (new Java.Net.URL (profilePictureUrl).OpenStream ());
							_nonConfiguration._profileName = "FB profile: " + (string)result ["name"];
							_nonConfiguration._isLoggedIn = true;
							RunOnUiThread (() => {
								UpdateUI ();
							});


						} else {
							_nonConfiguration._isLoggedIn = false;
							RunOnUiThread (() => {
								Alert ("Failed to Log In", "Reason: " + error, false, (res) => {
								});
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
				MomentsManager.SaveMoment (_nonConfiguration._moment, _nonConfiguration._photoBytes);
				RunOnUiThread (() => {
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


