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
using MomentsApp.Core;

namespace MomentsApp
{
	class MomentsArrayAdapter : BaseAdapter<Moment>
	{
		private Activity context;
		private List<Moment> values;

		public MomentsArrayAdapter (Activity context, List<Moment> moments) :
			base ()
		{
			this.context = context;
			this.values = moments;
		}

		public override long GetItemId (int position)
		{
			return values[position].ID;
		}

		public override Moment this [int position] {
			get { return values [position]; }
		}

		public override int Count {
			get { return values.Count; }
		}
			

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			LayoutInflater inflater = (LayoutInflater)context.GetSystemService (Context.LayoutInflaterService);
			View view = convertView;
			// If no view is used, create a new one
			if (view == null)
				view = inflater.Inflate (Android.Resource.Layout.SimpleListItem2, null);
			// Set Text to TextViews
			view.FindViewById<TextView> (Android.Resource.Id.Text1).Text = values [position].Time;
			view.FindViewById<TextView> (Android.Resource.Id.Text2).Text = values [position].Note;


			return view;
		}
	}
}

