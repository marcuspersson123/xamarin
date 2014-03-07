using System;

namespace MomentsApp.Core
{
	public class Moment
	{
		private int _id;
		public Moment ()
		{
			_id = -1;
		}

		public int ID { get { return _id; } set { _id = value; } }

		public string Note { get; set; }

		public string Longitude { get; set; }

		public string Latitude { get; set; }

		public string Time { get; set; }

		//		public Android.Graphics.Bitmap Photo { get; set; }

	}
}