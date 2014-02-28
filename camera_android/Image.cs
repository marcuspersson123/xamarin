﻿using System;

namespace camera_android.Core
{
	/// <summary>
	/// Image business object
	/// </summary>
	public class Image
	{
		public Image ()
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