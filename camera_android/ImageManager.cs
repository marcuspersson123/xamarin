using System;
using System.Collections.Generic;

namespace camera_android.Core
{
	/// <summary>
	/// Manager classes are an abstraction on the data access layers
	/// </summary>
	public static class ImageManager
	{
		static ImageManager ()
		{

		}

		public static Image GetImage (int id)
		{
			return ImageRepositoryADO.GetImage (id);
		}

		public static IList<Image> GetImages ()
		{
			return new List<Image> (ImageRepositoryADO.GetImages ());
		}

		public static int SaveImage (Image item)
		{
			return ImageRepositoryADO.SaveImage (item);
		}

		public static int DeleteImage (int id)
		{
			return ImageRepositoryADO.DeleteImage (id);
		}
	}
}

