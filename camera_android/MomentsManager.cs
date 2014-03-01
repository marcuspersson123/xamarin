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
			return MomentsRepositoryADO.SaveMoment (moment,PhotoToByteArray(moment));
		}

		public static int DeleteMoment (int id)
		{
			return MomentsRepositoryADO.DeleteMoment (id);
		}

		public static void GetPhoto(Moment moment) {
			byte[] photoBytes = MomentsRepositoryADO.GetPhotoByteArray (moment.ID);
			moment.Photo = Android.Graphics.BitmapFactory.DecodeByteArray(photoBytes, 0, photoBytes.Length);
			// photoBytes = null; out of memory-problemet. hade ingen effekt att sätt  till null 
		}

		public static byte[] PhotoToByteArray(Moment moment) {
			System.IO.MemoryStream stream = new System.IO.MemoryStream();
			moment.Photo.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 0, stream);
			byte[] photoBytes = stream.ToArray();
			//MomentsRepositoryADO.SavePhotoBytesArray (moment.ID, photoBytes);
			return photoBytes;
		}
	}
}

