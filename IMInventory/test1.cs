
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


namespace IMInventory
{
	[Activity (Label = "test1",MainLauncher=false,Icon="@drawable/icon")]			
	public class test1 : ListActivity  
	{
		//private SupportToolbar mtoolbar;
		string[] items;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			items = new string[] { "Vegetables","Fruits","Flower Buds","Legumes","Bulbs","Tubers" };
			//ListAdapter = new ArrayAdapter<String>(this, Resource.Layout.test1,Resource.Id.txtlist, items);
			//SetContentView (Resource.Layout.test1);
			//mtoolbar = FindViewById<SupportToolbar> (Resource.Id.toolbar);
			//SetSupportActionBar (mtoolbar);
			// Create your application here
		}
	}
}

