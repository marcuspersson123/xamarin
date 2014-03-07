using System;
using System.Collections.Generic;
using System.IO;

namespace MomentsApp.Core
{
	public class MomentsRepositoryADO {


		MomentDatabase db = null;
		protected static string dbLocation;		
		protected static MomentsRepositoryADO me;		

		static MomentsRepositoryADO ()
		{
			me = new MomentsRepositoryADO();
		}

		protected MomentsRepositoryADO ()
		{
			dbLocation = DatabaseFilePath;
			db = new MomentDatabase(dbLocation);
		}

		public static string DatabaseFilePath {
			get { 
				var sqliteFilename = "MomentsDatabase.db3";
				#if NETFX_CORE
				var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, sqliteFilename);
				#else

				#if SILVERLIGHT
				// Windows Phone expects a local path, not absolute
				var path = sqliteFilename;
				#else

				#if __ANDROID__
				// Just use whatever directory SpecialFolder.Personal returns
				string libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); ;
				#else
				// we need to put in /Library/ on iOS5.1 to meet Apple's iCloud terms
				// (they don't want non-user-generated data in Documents)
				string documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal); // Documents folder
				string libraryPath = Path.Combine (documentsPath, "..", "Library"); // Library folder
				#endif
				var path = Path.Combine (libraryPath, sqliteFilename);
				#endif

				#endif
				return path;	
			}
		}

		public static Moment GetMoment(int id)
		{
			return me.db.GetMoment(id);
		}

		public static IEnumerable<Moment> GetMoments ()
		{
			return me.db.GetMoments();
		}

		public static int SaveMoment (Moment moment, byte[] photoBytes)
		{
			return me.db.SaveMoment(moment, photoBytes);
		}

		public static int DeleteMoment(int id)
		{
			return me.db.DeleteMoment(id);
		}

		public static byte[] GetPhotoByteArray(int id) {
			byte[] photoBytes = me.db.LoadPhoto (id);
			return photoBytes;

		}


	}
}

