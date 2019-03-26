
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
using Android.Preferences;
using Android.Database.Sqlite;
using Android.Database;
using System.Globalization;

namespace IMInventory
{
	[Activity (Label = "FindItLocationListItem")]			
	public class FindItLocationListItem : Activity
	{
		SQLiteDatabase db = null;
		string locationname="";
		string editinventorytype="";
		string LocationItemIDFromScan="";
		List<LocationListItemTableItem> tableItems;
		ListView listView;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			db = this.OpenOrCreateDatabase ("ImInventory", FileCreationMode.Private, null);
			ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
			locationname = prefs.GetString("LocationNameFromLocationList", "");
			editinventorytype = prefs.GetString("InventoryTypeFromLocationList", "");
			LocationItemIDFromScan = prefs.GetString ("LocationItemIDFromScan", "");
			this.RequestWindowFeature(WindowFeatures.NoTitle);
			SetContentView (Resource.Layout.MainLocationListItem);
            TextView lblLocationListHeading = FindViewById<TextView>(Resource.Id.lblLocationListHeading);
            TextView lblListItemQuantity = FindViewById<TextView>(Resource.Id.lblListItemQuantity);
            TextView lblListItemUnitCost = FindViewById<TextView>(Resource.Id.lblListItemUnitCost);
            TextView lblListItemDescription = FindViewById<TextView>(Resource.Id.lblListItemDescription);
            //lblListItemQuantity.Visibility = ViewStates.Gone;
            //lblListItemUnitCost.Visibility = ViewStates.Gone;
            //lblListItemDescription.Gravity = GravityFlags.Center;
            lblLocationListHeading.Text = locationname.Trim();
			LocationListItemList ();
			ImageView imgLocationBack = FindViewById<ImageView>(Resource.Id.imgLocationListItemBack);
			imgLocationBack.Click += delegate
			{
				try{
					StartActivity(typeof(LocationList));
				}
				catch (Exception ex)
				{
					Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
				}
			};
			if (IMApplication.player != null) {
				IMApplication.player.Stop ();
				IMApplication.player = null;
			}

			// Create your application here
		}

		private void LocationListItemList(){
			ICursor c1 = null;
				if (LocationItemIDFromScan == "") {

				c1 = db.RawQuery ("SELECT * FROM " + "tbl_Inventory WHERE  Location = '" + locationname + "' And InventoryType = '" + editinventorytype + "'", null);
			} else { 
				
				c1 = db.RawQuery ("SELECT * FROM " + "tbl_Inventory WHERE  ID = " + LocationItemIDFromScan , null);
				}
			int locationnamecolumn = 0;
			int addeddate = 0;
			int inventorytype = 0;
			int inventoryidcolumn = 0;
			int quantitycolumn = 0;
			int descriptioncolumn = 0;
			int barcodecolumn = 0;
			int unitcostcolumn = 0;
			int Inventoryid = 0;

			if (c1.Count > 0) {
				locationnamecolumn = c1.GetColumnIndex ("Location");
				addeddate = c1.GetColumnIndex ("Addeddate");
				inventorytype = c1.GetColumnIndex ("InventoryType");
				inventoryidcolumn = c1.GetColumnIndex ("ID");
				quantitycolumn = c1.GetColumnIndex ("Quantity");
				descriptioncolumn = c1.GetColumnIndex ("ItemDescription");
				barcodecolumn = c1.GetColumnIndex ("BarCodeNumber");
				unitcostcolumn = c1.GetColumnIndex ("UnitCost");
				c1.MoveToFirst ();

				if (c1 != null)
				{
					tableItems = new List<LocationListItemTableItem>();

					// Loop through all Results
					do
					{

						String Locationname = c1.GetString(locationnamecolumn);
						string Addeddate = c1.GetString(addeddate);
						int InventoryType = c1.GetInt(inventorytype);
						string Quantity = c1.GetString(quantitycolumn);
						string Description = c1.GetString(descriptioncolumn);
						string BarCode = c1.GetString(barcodecolumn);
						string UnitCost = c1.GetString(unitcostcolumn);
						Inventoryid = c1.GetInt(inventoryidcolumn);
						if (InventoryType == 1)
						{
							tableItems.Add (new LocationListItemTableItem () {
								ID = Inventoryid.ToString (),
								Locationname = Locationname,
								Quantity = Quantity,
								ItemDescription = Description,
								UnitCost = UnitCost,
								Addeddate = Addeddate,
								InventoryType = "Manual"
								});
						}
						if (InventoryType == 3)
						{
							tableItems.Add(new LocationListItemTableItem()
								{
									ID=Inventoryid.ToString(),
									Locationname = Locationname,
									Quantity = Quantity,
									ItemDescription = BarCode,
									UnitCost=UnitCost,
								    Addeddate =Addeddate,
									InventoryType = "BarCode"
								});
						}

					}
					while (c1.MoveToNext());

				}
				listView = FindViewById<ListView>(Resource.Id.ListLocationItems); // get reference to the ListView in the layout
				// populate the listview with data
				listView.Adapter = new LocationItemListAdapter(this, tableItems);
			}
			if (c1.Count == 0)
			{
				try{
					tableItems = new List<LocationListItemTableItem>();
					tableItems.Add(new LocationListItemTableItem()
						{
							ID = "",
							Locationname = "No location item found",
							Addeddate = "",
							InventoryType = ""
						});
					listView = FindViewById<ListView>(Resource.Id.ListLocation); // get reference to the ListView in the layout
					// populate the listview with data
					listView.Adapter = new FindItLocationListItemAdapter(this, tableItems);
				}
				catch{

					Toast.MakeText(this, "No location item found", ToastLength.Long).Show();
					StartActivity (typeof(Main));
				}

				//View view = this.LayoutInflater.Inflate(Resource.Layout.SearchLocation, null);
				//view.FindViewById<LinearLayout>(Resource.Id.imgEditLocation).Visibility = ViewStates.Gone;

			}
		}
	}
}

