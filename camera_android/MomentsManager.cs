using System;
using System.Collections.Generic;

namespace camera_android.Core
{
	/// <summary>
	/// Manager classes are an abstraction on the data access layers
	/// </summary>
	public static class MomentsManager
	{
		static MomentsManager ()
		{

		}

		public static Moment GetMoment (int id, bool shallow)
		{
			return MomentRepositoryADO.GetMoment (id, shallow);
		}

		public static IList<Moment> GetMoments (bool shallow)
		{
			return new List<Moment> (MomentRepositoryADO.GetMoments (shallow));
		}

		public static int SaveMoment (Moment moment)
		{
			return MomentRepositoryADO.SaveMoment (moment);
		}

		public static int DeleteMoment (int id)
		{
			return MomentRepositoryADO.DeleteMoment (id);
		}
	}
}

