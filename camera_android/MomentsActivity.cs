using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MomentsApp
{
	[Activity (Label = "Moments App")]			
	public class MomentsActivity : ListActivity
	{


		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Could be replaced with List<String> filled with DB data
			//vals = new String[] { "stuff", "test", "others" };
			List<MomentsApp.Core.Moment> moments = (List<MomentsApp.Core.Moment>) MomentsManager.GetMoments ();

			ListAdapter = new MomentsArrayAdapter (this, moments);


		}
		// When item is clicked
		protected override void OnListItemClick (ListView l, View v, int position, long id)
		{
			Intent intent = new Intent();

			intent.PutExtra ("ID", id);
			SetResult (Result.Ok, intent);
			Finish ();
		}
	}
}

