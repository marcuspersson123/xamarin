using System;

namespace MomentsApp.Core
{
	public class Moment
	{
		public Moment ()
		{
		}

		public int ID { get; set; }

		public string Note { get; set; }

		public string Longitude { get; set; }

		public string Latitude { get; set; }

		public string Time { get; set; }

		public Android.Graphics.Bitmap Photo { get; set; }

	}
}