/*
 * Copyright (C) 2007 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Android.App;
using Android.Content;
using Android.Util;
using Android.OS;
using Android.Graphics;
using Android.Views;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.GoogleMaps;
using Android.Widget;

namespace MonoDroid.Samples.MapsDemo
{
	// This demonstrates creating a Map based Activity.
	[Activity (Label = "MapView")]
	public class MapViewDemo : Activity
	{
        static readonly LatLng HAMBURG = new LatLng(53.558, 9.927);
        static readonly LatLng KIEL = new LatLng(53.551, 9.993);
        private GoogleMap map;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            MapFragment mapFrag = MapFragment.NewInstance();
            FragmentTransaction tx = FragmentManager.BeginTransaction();
            tx.Add(Resource.Id.map, mapFrag);
            tx.Commit();

            SetContentView(Resource.Layout.mapview);
            map = mapFrag.Map;

            // detta funkar inte (map == null(?))
            if (map != null)
            {
                Toast.MakeText(this, "shows", ToastLength.Short).Show();
                MarkerOptions markerOpt = new MarkerOptions();
                markerOpt.SetPosition(new LatLng(50.379444, 2.773611));
                markerOpt.SetTitle("Vimy Ridge");
                map.AddMarker(markerOpt);
            }
        }
	}
}
