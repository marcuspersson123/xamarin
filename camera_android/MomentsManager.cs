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

		public static int SaveMoment (Moment moment, byte[] photoBytes)
		{
			return MomentsRepositoryADO.SaveMoment (moment, photoBytes);
		}

		public static int DeleteMoment (int id)
		{
			return MomentsRepositoryADO.DeleteMoment (id);
		}

		public static byte[] GetPhotoBytes (Moment moment)
		{
			byte[] photoBytes = MomentsRepositoryADO.GetPhotoByteArray (moment.ID);
			return photoBytes;

		}


	}
}

