using System;
using System.Collections.Generic;
using MomentsApp.Core;

namespace MomentsApp
{
	public static class MomentsManager
	{
		static MomentsManager ()
		{

		}

		public static Moment GetMoment (int id)
		{
			return MomentsRepositoryADO.GetMoment (id);
		}

		public static IList<Moment> GetMoments ()
		{
			return new List<Moment> (MomentsRepositoryADO.GetMoments ());
		}

		public static int SaveMoment (Moment moment)
		{
			return MomentsRepositoryADO.SaveMoment (moment, PhotoToByteArray (moment));
		}

		public static int DeleteMoment (int id)
		{
			return MomentsRepositoryADO.DeleteMoment (id);
		}

		public static void GetPhoto (Moment moment)
		{
			byte[] photoBytes = MomentsRepositoryADO.GetPhotoByteArray (moment.ID);
			using (var bitmap = Android.Graphics.BitmapFactory.DecodeByteArray (photoBytes, 0, photoBytes.Length)) {
				moment.Photo = bitmap;
			}

		}

		public static byte[] PhotoToByteArray (Moment moment)
		{
			byte[] photoBytes = null;
			using (System.IO.MemoryStream stream = new System.IO.MemoryStream ()) {
				moment.Photo.Compress (Android.Graphics.Bitmap.CompressFormat.Png, 0, stream);
				photoBytes = stream.ToArray ();
			}
			return photoBytes;
		}
	}
}

