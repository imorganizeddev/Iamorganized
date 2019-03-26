﻿using System;
using Android.Widget;
using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Runtime;
using Android.Graphics;
using Android.Content;
using Android.Preferences;
using Android.Database.Sqlite;
using Android.Database;
using Android.Media;
using System.IO;
namespace IMInventory
{
	public class LocationItemListAdapter:BaseAdapter<LocationListItemTableItem>
	{
		List<LocationListItemTableItem> items;
		Activity context;
		SQLiteDatabase db = null;
		int i=0;
		Dialog	dialog;
		ISharedPreferencesEditor editor;
		ISharedPreferences prefs;
		//private ItemFilter mFilter = new ItemFilter();

		public LocationItemListAdapter(Activity context, List<LocationListItemTableItem> items)
			: base()
		{
			
			this.context = context;
			this.items = items;

			prefs = PreferenceManager.GetDefaultSharedPreferences (context); 
		}
		public override long GetItemId(int position)
		{
			return position;
		}
		public override LocationListItemTableItem this[int position]
		{
			get { return items[position]; }
		}
		public override int Count
		{
			get { return items.Count; }
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			var item = items[position];
			View view = convertView;
			if (view == null) // no view to re-use, create new
				view = context.LayoutInflater.Inflate(Resource.Layout.LocationListItem, null);

			Typeface tf = Typeface.CreateFromAsset(context.Assets, "Fonts/ROBOTO-LIGHT.TTF");

            view.FindViewById<TextView>(Resource.Id.TextLocationItem).Text = GetFixedLenghString(item.ItemDescription.Trim().ToString(), 13);

			view.FindViewById<TextView>(Resource.Id.TextLocationItem).Typeface = tf;
			view.FindViewById<TextView>(Resource.Id.TextLocationItem).Invalidate();



			view.FindViewById<TextView>(Resource.Id.TextLocationItem).Click+=delegate {
				try{
					GetInventoryDetails(item.ID,item.InventoryType.Trim (),item.Locationname);
				}
				catch (Exception ex)
				{
					Toast.MakeText(context, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
				}
			};

            view.FindViewById<TextView>(Resource.Id.TextLocationItemInventoryDate).Text = item.Quantity.Trim();
			view.FindViewById<TextView>(Resource.Id.TextLocationItemInventoryDate).Typeface = tf;
			view.FindViewById<TextView>(Resource.Id.TextLocationItemInventoryDate).Invalidate();
			view.FindViewById<TextView>(Resource.Id.hdnInventoryID).Text = item.ID;
			view.FindViewById<TextView> (Resource.Id.TextLocationItemInventoryDate).Click += delegate {
                try{
				GetInventoryDetails(item.ID,item.InventoryType.Trim (),item.Locationname);
                }
                catch (Exception ex)
                {
                    Toast.MakeText(context, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                }
			};

            view.FindViewById<TextView>(Resource.Id.TextLocationItemInventoryUnitCost).Text = item.UnitCost.Trim().ToString();

			view.FindViewById<TextView>(Resource.Id.TextLocationItemInventoryUnitCost).Typeface = tf;
			view.FindViewById<TextView>(Resource.Id.TextLocationItemInventoryUnitCost).Invalidate();
			view.FindViewById<TextView> (Resource.Id.TextLocationItemInventoryUnitCost).Click += delegate {
				try{
					if(item.ID!=""){
						GetInventoryDetails(item.ID,item.InventoryType.Trim (),item.Locationname);
					}
				}
				catch (Exception ex)
				{
					Toast.MakeText(context, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
				}
			};
			return view;
		}


		public string GetFixedLenghString(string s, int length)
		{
			
			if (s.Trim().Length > length)
			{
				s = s.Substring(0, length);
				s += "...";
			}
			return s;
		}
		private void GetInventoryDetails(string ID,string InventoryType, string Locationname){
			
			editor = prefs.Edit ();


			if (InventoryType.ToLower() == "manual") {
				editor.PutLong ("InventoryType", 1);
				editor.PutString ("LocationName", Locationname);
				try{
				context.Finish ();
				}
				catch{
				}
				editor.PutString ("EditFromItemList", ID);
				editor.PutString ("EditFromItemListForBarcode", "");
				editor.Commit ();
				editor.Apply ();
				context.StartActivity (typeof(EntryTab));

			}

			if (InventoryType.ToLower() == "barcode") {
				editor.PutLong ("InventoryType", 3);
				editor.PutString ("LocationName", Locationname);
				try{
					context.Finish ();
				}
				catch{
				}
				editor.PutString ("EditFromItemList", "");
				editor.PutString ("EditFromItemListForBarcode", ID);
				editor.Commit ();
				editor.Apply ();
				context.StartActivity (typeof(EntryTab));
			}

		
		}
	}
}
