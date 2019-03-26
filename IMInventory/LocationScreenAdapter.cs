using System;
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
    public class LocationScreenAdapter : BaseAdapter<LocationTableItem>
    {
        List<LocationTableItem> items;
        Activity context;
        SQLiteDatabase db = null;
        int i = 0;
        Dialog dialog;
        //private ItemFilter mFilter = new ItemFilter();

        public LocationScreenAdapter(Activity context, List<LocationTableItem> items)
            : base()
        {
            this.context = context;
            this.items = items;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override LocationTableItem this[int position]
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
                view = context.LayoutInflater.Inflate(Resource.Layout.SearchLocation, null);

            Typeface tf = Typeface.CreateFromAsset(context.Assets, "Fonts/ROBOTO-LIGHT.TTF");
            view.FindViewById<TextView>(Resource.Id.TextLocation).Text = item.Locationname;
            view.FindViewById<TextView>(Resource.Id.TextLocation).Typeface = tf;
            view.FindViewById<TextView>(Resource.Id.TextLocation).Invalidate();
            if (item.InventoryType.Trim() == "Manual")
            {
                view.FindViewById<ImageView>(Resource.Id.TextInventoryType).SetImageResource(Resource.Drawable.edit);
            }
            if (item.InventoryType.Trim() == "Barcode")
            {
                view.FindViewById<ImageView>(Resource.Id.TextInventoryType).SetImageResource(Resource.Drawable.barcode);
            }
            if (item.InventoryType.Trim() == "Voice")
            {
                view.FindViewById<ImageView>(Resource.Id.TextInventoryType).SetImageResource(Resource.Drawable.voice);
            }
            //view.FindViewById<TextView>(Resource.Id.TextInventoryType).Typeface = tf;
            //view.FindViewById<TextView>(Resource.Id.TextInventoryType).Invalidate();

            view.FindViewById<TextView>(Resource.Id.TextInventoryDate).Text = item.Addeddate;
            view.FindViewById<TextView>(Resource.Id.TextInventoryDate).Typeface = tf;
            view.FindViewById<TextView>(Resource.Id.TextInventoryDate).Invalidate();
            if (item.InventoryType.Trim() == "")
            {
                view.FindViewById<LinearLayout>(Resource.Id.imgEditLocation).Visibility = ViewStates.Gone;
            }
            else
            {
                view.FindViewById<LinearLayout>(Resource.Id.imgEditLocation).Visibility = ViewStates.Visible;

                view.FindViewById<LinearLayout>(Resource.Id.imgEditLocation).Click += delegate
                {

                    try
                    {
                        findposition(position, item.Locationname, item.InventoryType, item.ID);
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(context, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                    }

                };
            }



            return view;
        }

        private void findposition(int position, string locationname, string inventorytype, string id)
        {


            if (i > 0)
            {

                dialog.Dismiss();

                ISharedPreferences prefs;
                // context.StartActivity(typeof(Main));
                prefs = PreferenceManager.GetDefaultSharedPreferences(context);
                ISharedPreferencesEditor editor = prefs.Edit();

                Dialog locationeditdialog = new Dialog(context);
                dialog = new Dialog(context);

                dialog.RequestWindowFeature((int)WindowFeatures.NoTitle);
                dialog.SetContentView(Resource.Layout.LocationEditPopUp);
                ImageView imgCloseLocationEditPopUp = dialog.FindViewById<ImageView>(Resource.Id.imgCloseLocationEditPopUp);
                RelativeLayout rlAddLocation = dialog.FindViewById<RelativeLayout>(Resource.Id.rlAddLocation);
                RelativeLayout rlRenameLocation = dialog.FindViewById<RelativeLayout>(Resource.Id.rlRenameLocation);
                RelativeLayout rlDeleteLocation = dialog.FindViewById<RelativeLayout>(Resource.Id.rlDeleteLocation);
                RelativeLayout rlDetailsLocation = dialog.FindViewById<RelativeLayout>(Resource.Id.rlDetailsLocation);
                RelativeLayout rlListLocationItem = dialog.FindViewById<RelativeLayout>(Resource.Id.rlListLocationItem);

                if (inventorytype.Trim() == "Manual" || inventorytype.Trim() == "Barcode")
                {
                    rlListLocationItem.Visibility = ViewStates.Visible;
                }
                else
                {

                    rlListLocationItem.Visibility = ViewStates.Gone;
                }

                imgCloseLocationEditPopUp.Click += delegate
                {
                    dialog.Dismiss();
                };


                rlAddLocation.Click += delegate
                {

                    try
                    {
                        if (inventorytype.Trim() == "Manual")
                        {
                            editor.PutLong("InventoryType", 1);
                            editor.PutString("LocationName", locationname);

                            context.StartActivity(typeof(EntryTab));


                        }
                        if (inventorytype.Trim() == "Barcode")
                        {
                            editor.PutLong("InventoryType", 3);
                            editor.PutString("LocationName", locationname);
                            context.StartActivity(typeof(EntryTab));

                        }
                        if (inventorytype.Trim() == "Voice")
                        {
                            editor.PutLong("InventoryType", 2);
                            editor.PutString("LocationName", locationname);
                            editor.PutInt("FromVoiceEdit", 1);
                            context.StartActivity(typeof(EntryTab));

                        }
                        editor.Commit();
                        // applies changes synchronously on older APIs
                        editor.Apply();
                        dialog.Dismiss();
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(context, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                    }
                };

                rlListLocationItem.Click += delegate
                {
                    try
                    {

                        editor.PutString("LocationNameFromLocationList", locationname);
                        if (inventorytype.Trim() == "Manual")
                        {
                            editor.PutString("InventoryTypeFromLocationList", "1");

                        }
                        if (inventorytype.Trim() == "Barcode")
                        {
                            editor.PutString("InventoryTypeFromLocationList", "3");

                        }
                        editor.Commit();
                        // applies changes synchronously on older APIs
                        editor.Apply();
                        try
                        {
                            context.Finish();
                        }
                        catch
                        {
                        }
                        context.StartActivity(typeof(LocationListItem));

                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(context, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                    }

                };
                rlDeleteLocation.Click += delegate
                {
                    try
                    {
                        db = context.OpenOrCreateDatabase("ImInventory", FileCreationMode.Private, null);
                        AlertDialog.Builder builder = new AlertDialog.Builder(context);
                        builder.SetMessage("Are you sure want to delete this location?");
                        builder.SetCancelable(false);
                        //builder .SetIcon (Resource.Drawable.Icon);
                        builder.SetPositiveButton("Yes", (object sender, DialogClickEventArgs e) =>
                        {
								string locationnamefromsession=prefs.GetString("LocationName","");
								if(locationname.Trim()==locationnamefromsession.Trim()){
								editor.PutString ("LocationName", "");
								editor.Commit();
								editor.Apply ();  
								
								}
                            if (inventorytype.Trim() == "Manual")
                            {
                                try
                                {
                                    db.ExecSQL("delete from " + "tbl_Inventory where ID='" + id + "'");
                                    context.Finish();
                                    context.StartActivity(typeof(LocationList));
                                    Toast.MakeText(context, "Location deleted successfully", ToastLength.Long).Show();
                                    
                                }
                                catch
                                {
                                   // Toast.MakeText(context, "An error occured. Pls try again later", ToastLength.Long).Show();

                                }
                            }

                            if (inventorytype.Trim() == "Barcode")
                            {
                                try
                                {
                                    db.ExecSQL("delete from " + "tbl_Inventory where ID='" + id + "'");
                                    context.Finish();
                                    context.StartActivity(typeof(LocationList));
                                    Toast.MakeText(context, "Location deleted successfully", ToastLength.Long).Show();
                                }
                                catch
                                {

                                   // Toast.MakeText(context, "An error occured. Pls try again later", ToastLength.Long).Show();
                                }
                            }

                            if (inventorytype.Trim() == "Voice")
                            {
                                try
                                {
                                    db.ExecSQL("delete from " + "tbl_Inventory where ID='" + id + "'");
                                    context.Finish();
                                    context.StartActivity(typeof(LocationList));
                                    Toast.MakeText(context, "Location deleted successfully", ToastLength.Long).Show();
                                }
                                catch
                                {
                                    //Toast.MakeText(context, "An error occured. Pls try again later", ToastLength.Long).Show();

                                }

                            }
                        });

                        builder.SetNegativeButton("No", (object sender, DialogClickEventArgs e) =>
                        {

                        });
                        AlertDialog alertdialog = builder.Create();
                        alertdialog.Show();
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(context, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                    }
                };

                rlRenameLocation.Click += delegate
                {
                    try
                    {
                        locationeditdialog = new Dialog(context);
                        locationeditdialog.RequestWindowFeature((int)WindowFeatures.NoTitle);
                        locationeditdialog.SetContentView(Resource.Layout.EditLocationFromPopUp);

                        ImageView btnEditLocationFromPopUp = locationeditdialog.FindViewById<ImageView>(Resource.Id.btnEditLocationFromPopUp);
                        EditText txtEditPopUpLocationName = locationeditdialog.FindViewById<EditText>(Resource.Id.txtEditPopUpLocationName);
                        ImageView btnCancelEditLocationFromPopUp = locationeditdialog.FindViewById<ImageView>(Resource.Id.btnCancelEditLocationFromPopUp);
                        Typeface tf = Typeface.CreateFromAsset(context.Assets, "Fonts/ROBOTO-LIGHT.TTF");
                        txtEditPopUpLocationName.Typeface = tf;
                        txtEditPopUpLocationName.Invalidate();
                        txtEditPopUpLocationName.Text = locationname;
                        locationeditdialog.SetCanceledOnTouchOutside(false);
                        locationeditdialog.Show();
                        btnCancelEditLocationFromPopUp.Click += delegate
                        {
                            locationeditdialog.Dismiss();
                        };
                        btnEditLocationFromPopUp.Click += delegate
                        {
                            try
                            {
                                string updatequery = "";

                                ContentValues values = new ContentValues();
                                values.Put("Location", txtEditPopUpLocationName.Text);

                                db = context.OpenOrCreateDatabase("ImInventory", FileCreationMode.Private, null);
                                if (inventorytype.Trim() == "Manual")
                                {
                                    updatequery = "update" + " tbl_Inventory set Location='" + txtEditPopUpLocationName.Text + "'   where Location='" + locationname + "'";
                                    db.Update("tbl_Inventory", values, "ID = " + id, null);
                                    //db.ExecSQL ("update" + " tbl_Inventory set Location='" + txtEditPopUpLocationName.Text + "' where ID = " + id);

                                }
                                if (inventorytype.Trim() == "Barcode")
                                {
                                    //updatequery = "update" + " tbl_Inventory set Location='" + txtEditPopUpLocationName.Text + "' where Location='" + locationname + "'";
                                    db.Update("tbl_Inventory", values, "ID = " + id, null);
                                    //db.ExecSQL ("update" + " tbl_Inventory set Location='" + txtEditPopUpLocationName.Text + "' where ID = " + id);
                                }
                                if (inventorytype.Trim() == "Voice")
                                {
                                    //db.Update("tbl_Inventory",values,"ID = " + id,null);
                                    //db.ExecSQL ("update" + " tbl_Inventory set Location='" + txtEditPopUpLocationName.Text + "' where ID = " + id);
                                }
                                context.Finish();
                                context.StartActivity(typeof(LocationList));
                                Toast.MakeText(context, "Location updated successfully", ToastLength.Long).Show();
                            }
                            catch (Exception ex)
                            {
                                Toast.MakeText(context, "Location updation failed", ToastLength.Long).Show();

                            }

                        };
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(context, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                    }
                };

                rlDetailsLocation.Click += delegate
                {
                    try
                    {
                        db = context.OpenOrCreateDatabase("ImInventory", FileCreationMode.Private, null);
                        int manualcount = 0;
                        int voicecount = 0;
                        int barcodecount = 0;
                        ICursor c1 = db.RawQuery("SELECT * FROM " + "tbl_Inventory Where InventoryType=1 AND Location='" + locationname + "'", null);
                        manualcount = c1.Count;



                        c1 = db.RawQuery("SELECT * FROM " + "tbl_Inventory Where InventoryType=3 AND Location='" + locationname + "'", null);
                        barcodecount = c1.Count;

                        c1 = db.RawQuery("SELECT * FROM " + "tbl_Inventory Where InventoryType=2 AND Location='" + locationname + "'", null);
                        voicecount = c1.Count;

                        Dialog locationdetailsdialog = new Dialog(context);
                        locationdetailsdialog.RequestWindowFeature((int)WindowFeatures.NoTitle);
                        locationdetailsdialog.SetContentView(Resource.Layout.LocationDetails);

                        TextView txtManualEntryCount = locationdetailsdialog.FindViewById<TextView>(Resource.Id.txtManualEntryCount);
                        TextView txtVoiceLength = locationdetailsdialog.FindViewById<TextView>(Resource.Id.txtVoiceLength);
                        TextView txtBarcodeEntryCount = locationdetailsdialog.FindViewById<TextView>(Resource.Id.txtBarcodeEntryCount);

                        TextView txtManualEntryCountText = locationdetailsdialog.FindViewById<TextView>(Resource.Id.txtManualEntryCountText);
                        TextView txtVoiceLengthText = locationdetailsdialog.FindViewById<TextView>(Resource.Id.txtVoiceLengthText);
                        TextView txtBarcodeEntryCountText = locationdetailsdialog.FindViewById<TextView>(Resource.Id.txtBarcodeEntryCountText);
                        ImageView imgCloseLocationCountPopUp = locationdetailsdialog.FindViewById<ImageView>(Resource.Id.imgCloseLocationCountPopUp);
                        imgCloseLocationCountPopUp.Click += delegate
                        {
                            locationdetailsdialog.Dismiss();
                        };
                        string audiopath = CreateDirectoryForAudio();
                        String duration = "";
                        Int32 totalduration = 0;
                        string timeduration = "";
                        int hourcount = 0;
                        int mincount = 0;
                        int seccount = 0;
                        string hour = "";
                        string min = "";
                        string sec = "";

                        c1.MoveToFirst();
                        if (voicecount > 0)
                        {
                            //locationnamecolumn = c1.GetColumnIndex("Location");
                            //addeddate = c1.GetColumnIndex("Addeddate");
                            int voicecolumn = c1.GetColumnIndex("AudioFileName");

                            do
                            {
                                String AudioFileName = c1.GetString(voicecolumn);
                                if (AudioFileName != "")
                                {
                                    if (AudioFileName.Contains("|"))
                                    {
                                        string[] ArrAudioFileName = AudioFileName.Split('|');
                                        foreach (var item in ArrAudioFileName)
                                        {
                                            if (item.ToString() != "")
                                            {
                                                string AudioFilePath = "";
                                                AudioFilePath = audiopath + "/" + item.ToString();
                                                MediaMetadataRetriever mmr = new MediaMetadataRetriever();
                                                mmr.SetDataSource(AudioFilePath);
                                                duration = mmr.ExtractMetadata(MetadataKey.Duration);
                                                totalduration = totalduration + Convert.ToInt32(duration);
                                                TimeSpan ts = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(duration));
                                                ts = TimeSpan.FromMilliseconds(Convert.ToDouble(totalduration));
                                                hourcount = hourcount + ts.Hours;
                                                mincount = mincount + ts.Minutes;
                                                seccount = seccount + ts.Seconds;



                                            }
                                        }
                                    }
                                    else
                                    {
                                        MediaMetadataRetriever mmr = new MediaMetadataRetriever();
                                        mmr.SetDataSource(audiopath + "/" + AudioFileName);
                                        duration = mmr.ExtractMetadata(MetadataKey.Duration);
                                        totalduration = totalduration + Convert.ToInt32(duration);
                                        TimeSpan ts = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(duration));
                                        ts = TimeSpan.FromMilliseconds(Convert.ToDouble(totalduration));
                                        hourcount = hourcount + ts.Hours;
                                        mincount = mincount + ts.Minutes;
                                        seccount = seccount + ts.Seconds;

                                    }
                                }

                                //mmr.SetDataSource(
                            }
                            while (c1.MoveToNext());



                        }

                        if (hourcount < 10)
                        {
                            hour = "0" + hourcount.ToString();

                        }
                        else
                        {
                            hour = hourcount.ToString();

                        }

                        if (mincount < 10)
                        {
                            min = "0" + mincount.ToString();

                        }
                        else
                        {
                            min = mincount.ToString();

                        }

                        if (seccount < 10)
                        {
                            sec = "0" + seccount.ToString();

                        }
                        else
                        {
                            sec = seccount.ToString();

                        }
                        timeduration = string.Format("{0}:{1}:{2}", hour, min, sec);


                        Typeface tf = Typeface.CreateFromAsset(context.Assets, "Fonts/ROBOTO-LIGHT.TTF");
                        txtBarcodeEntryCount.Typeface = tf;
                        txtBarcodeEntryCount.Invalidate();
                        txtManualEntryCount.Text = manualcount.ToString(); ;

                        txtVoiceLength.Typeface = tf;
                        txtVoiceLength.Invalidate();
                        txtVoiceLength.Text = timeduration;

                        txtBarcodeEntryCount.Typeface = tf;
                        txtBarcodeEntryCount.Invalidate();
                        txtBarcodeEntryCount.Text = barcodecount.ToString();


                        txtManualEntryCountText.Typeface = tf;
                        txtManualEntryCountText.Invalidate();

                        txtVoiceLengthText.Typeface = tf;
                        txtVoiceLengthText.Invalidate();

                        txtBarcodeEntryCountText.Typeface = tf;
                        txtBarcodeEntryCountText.Invalidate();
                        locationdetailsdialog.SetCanceledOnTouchOutside(false);

                        locationdetailsdialog.Show();
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(context, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                    }
                };
                dialog.SetCanceledOnTouchOutside(false);
                dialog.Show();
                i = i + 1;

            }
            else
            {
                ISharedPreferences prefs;
                // context.StartActivity(typeof(Main));
                prefs = PreferenceManager.GetDefaultSharedPreferences(context);
                ISharedPreferencesEditor editor = prefs.Edit();

                Dialog locationeditdialog = new Dialog(context);
                dialog = new Dialog(context);

                dialog.RequestWindowFeature((int)WindowFeatures.NoTitle);
                dialog.SetContentView(Resource.Layout.LocationEditPopUp);
                ImageView imgCloseLocationEditPopUp = dialog.FindViewById<ImageView>(Resource.Id.imgCloseLocationEditPopUp);
                RelativeLayout rlAddLocation = dialog.FindViewById<RelativeLayout>(Resource.Id.rlAddLocation);
                RelativeLayout rlRenameLocation = dialog.FindViewById<RelativeLayout>(Resource.Id.rlRenameLocation);
                RelativeLayout rlDeleteLocation = dialog.FindViewById<RelativeLayout>(Resource.Id.rlDeleteLocation);
                RelativeLayout rlDetailsLocation = dialog.FindViewById<RelativeLayout>(Resource.Id.rlDetailsLocation);
                RelativeLayout rlListLocationItem = dialog.FindViewById<RelativeLayout>(Resource.Id.rlListLocationItem);

                if (inventorytype.Trim() == "Manual" || inventorytype.Trim() == "Barcode")
                {
                    rlListLocationItem.Visibility = ViewStates.Visible;
                }
                else
                {

                    rlListLocationItem.Visibility = ViewStates.Gone;
                }
                imgCloseLocationEditPopUp.Click += delegate
                {
                    dialog.Dismiss();
                };
                rlListLocationItem.Click += delegate
                {
                    try
                    {
                        editor.PutString("LocationNameFromLocationList", locationname);
                        if (inventorytype.Trim() == "Manual")
                        {
                            editor.PutString("InventoryTypeFromLocationList", "1");

                        }
                        if (inventorytype.Trim() == "Barcode")
                        {
                            editor.PutString("InventoryTypeFromLocationList", "3");

                        }
                        editor.Commit();
                        // applies changes synchronously on older APIs
                        editor.Apply();


                        try
                        {
                            context.Finish();
                        }
                        catch
                        {
                        }

                        context.StartActivity(typeof(LocationListItem));
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(context, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                    }
                };
                rlAddLocation.Click += delegate
                {
                    try
                    {
                        if (inventorytype.Trim() == "Manual")
                        {
                            editor.PutLong("InventoryType", 1);
                            editor.PutString("LocationName", locationname);
                            context.StartActivity(typeof(EntryTab));


                        }
                        if (inventorytype.Trim() == "Barcode")
                        {
                            editor.PutLong("InventoryType", 3);
                            editor.PutString("LocationName", locationname);
                            context.StartActivity(typeof(EntryTab));

                        }
                        if (inventorytype.Trim() == "Voice")
                        {
                            editor.PutLong("InventoryType", 2);
                            editor.PutString("LocationName", locationname);
                            editor.PutInt("FromVoiceEdit", 1);
                            context.StartActivity(typeof(EntryTab));

                        }
                        editor.Commit();
                        // applies changes synchronously on older APIs
                        editor.Apply();
                        dialog.Dismiss();
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(context, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                    }
                };
                rlDeleteLocation.Click += delegate
                {

                    try
                    {
                        db = context.OpenOrCreateDatabase("ImInventory", FileCreationMode.Private, null);
                        AlertDialog.Builder builder = new AlertDialog.Builder(context);
                        builder.SetMessage("Are you sure want to delete this location?");
                        builder.SetCancelable(false);
                        //builder .SetIcon (Resource.Drawable.Icon);
                        builder.SetPositiveButton("Yes", (object sender, DialogClickEventArgs e) =>
                        {


                            if (inventorytype.Trim() == "Manual")
                            {
                                try
                                {
                                    db.ExecSQL("delete from " + "tbl_Inventory where ID='" + id + "'");
                                    context.Finish();
                                    context.StartActivity(typeof(LocationList));
                                    Toast.MakeText(context, "Location deleted successfully", ToastLength.Long).Show();
                                }
                                catch
                                {
                                   // Toast.MakeText(context, "An error occured. Pls try again later", ToastLength.Long).Show();

                                }
                            }

                            if (inventorytype.Trim() == "Barcode")
                            {
                                try
                                {
                                    db.ExecSQL("delete from " + "tbl_Inventory where ID='" + id + "'");
                                    context.Finish();
                                    context.StartActivity(typeof(LocationList));
                                    Toast.MakeText(context, "Location deleted successfully", ToastLength.Long).Show();
                                }
                                catch
                                {

                                   // Toast.MakeText(context, "An error occured. Pls try again later", ToastLength.Long).Show();
                                }
                            }

                            if (inventorytype.Trim() == "Voice")
                            {
                                try
                                {
                                    db.ExecSQL("delete from " + "tbl_Inventory where ID='" + id + "'");
                                    context.Finish();
                                    context.StartActivity(typeof(LocationList));
                                    Toast.MakeText(context, "Location deleted successfully", ToastLength.Long).Show();
                                }
                                catch
                                {
                                    //Toast.MakeText(context, "An error occured. Pls try again later", ToastLength.Long).Show();

                                }

                            }
                        });

                        builder.SetNegativeButton("No", (object sender, DialogClickEventArgs e) =>
                        {

                        });
                        AlertDialog alertdialog = builder.Create();
                        alertdialog.Show();
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(context, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                    }
                };

                rlRenameLocation.Click += delegate
                {
                    try
                    {
                        locationeditdialog = new Dialog(context);
                        locationeditdialog.RequestWindowFeature((int)WindowFeatures.NoTitle);
                        locationeditdialog.SetContentView(Resource.Layout.EditLocationFromPopUp);
                        ImageView btnEditLocationFromPopUp = locationeditdialog.FindViewById<ImageView>(Resource.Id.btnEditLocationFromPopUp);
                        EditText txtEditPopUpLocationName = locationeditdialog.FindViewById<EditText>(Resource.Id.txtEditPopUpLocationName);
						ImageView btnCancelEditLocationFromPopUp = locationeditdialog.FindViewById<ImageView>(Resource.Id.btnCancelEditLocationFromPopUp);
                        Typeface tf = Typeface.CreateFromAsset(context.Assets, "Fonts/ROBOTO-LIGHT.TTF");
                        txtEditPopUpLocationName.Typeface = tf;
                        txtEditPopUpLocationName.Invalidate();
                        txtEditPopUpLocationName.Text = locationname;
                        locationeditdialog.Show();

						btnCancelEditLocationFromPopUp.Click += delegate
						{
							locationeditdialog.Dismiss();
						};
                        btnEditLocationFromPopUp.Click += delegate
                        {
                            try
                            {
                                string updatequery = "";

                                ContentValues values = new ContentValues();
                                values.Put("Location", locationname);

                                db = context.OpenOrCreateDatabase("ImInventory", FileCreationMode.Private, null);
                                if (inventorytype.Trim() == "Manual")
                                {
                                    updatequery = "update" + " tbl_Inventory set Location='" + txtEditPopUpLocationName.Text + "'   where Location='" + locationname + "'";

                                    db.ExecSQL("update" + " tbl_Inventory set Location='" + txtEditPopUpLocationName.Text + "' where ID = " + id);

                                }
                                if (inventorytype.Trim() == "Barcode")
                                {
                                    updatequery = "update" + " tbl_Inventory set Location='" + txtEditPopUpLocationName.Text + "' where Location='" + locationname + "'";

                                    db.ExecSQL("update" + " tbl_Inventory set Location='" + txtEditPopUpLocationName.Text + "' where ID = " + id);
                                }
                                if (inventorytype.Trim() == "Voice")
                                {

                                    db.ExecSQL("update" + " tbl_Inventory set Location='" + txtEditPopUpLocationName.Text + "' where ID = " + id);
                                }
                                context.Finish();
                                context.StartActivity(typeof(LocationList));
                                Toast.MakeText(context, "Location updated successfully", ToastLength.Long).Show();
                            }
                            catch (Exception ex)
                            {
                                Toast.MakeText(context, "Location updation failed", ToastLength.Long).Show();

                            }

                        };
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(context, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                    }
                };

                rlDetailsLocation.Click += delegate
                {
                    try
                    {
                        db = context.OpenOrCreateDatabase("ImInventory", FileCreationMode.Private, null);
                        int manualcount = 0;
                        int voicecount = 0;
                        int barcodecount = 0;
                        ICursor c1 = db.RawQuery("SELECT * FROM " + "tbl_Inventory Where InventoryType=1 AND Location='" + locationname + "'", null);
                        manualcount = c1.Count;


                        c1 = db.RawQuery("SELECT * FROM " + "tbl_Inventory Where InventoryType=3 AND Location='" + locationname + "'", null);
                        barcodecount = c1.Count;

                        c1 = db.RawQuery("SELECT * FROM " + "tbl_Inventory Where InventoryType=2 AND Location='" + locationname + "'", null);
                        voicecount = c1.Count;

                        Dialog locationdetailsdialog = new Dialog(context);
                        locationdetailsdialog.RequestWindowFeature((int)WindowFeatures.NoTitle);
                        locationdetailsdialog.SetContentView(Resource.Layout.LocationDetails);
                        TextView txtManualEntryCount = locationdetailsdialog.FindViewById<TextView>(Resource.Id.txtManualEntryCount);
                        TextView txtVoiceLength = locationdetailsdialog.FindViewById<TextView>(Resource.Id.txtVoiceLength);
                        TextView txtBarcodeEntryCount = locationdetailsdialog.FindViewById<TextView>(Resource.Id.txtBarcodeEntryCount);

                        TextView txtManualEntryCountText = locationdetailsdialog.FindViewById<TextView>(Resource.Id.txtManualEntryCountText);
                        TextView txtVoiceLengthText = locationdetailsdialog.FindViewById<TextView>(Resource.Id.txtVoiceLengthText);
                        TextView txtBarcodeEntryCountText = locationdetailsdialog.FindViewById<TextView>(Resource.Id.txtBarcodeEntryCountText);

                        ImageView imgCloseLocationCountPopUp = locationdetailsdialog.FindViewById<ImageView>(Resource.Id.imgCloseLocationCountPopUp);
                        imgCloseLocationCountPopUp.Click += delegate
                        {
                            locationdetailsdialog.Dismiss();
                        };
                        string audiopath = CreateDirectoryForAudio();
                        String duration = "";
                        Int32 totalduration = 0;
                        string timeduration = "";

                        int hourcount = 0;
                        int mincount = 0;
                        int seccount = 0;
                        string hour = "";
                        string min = "";
                        string sec = "";
                        c1.MoveToFirst();
                        if (voicecount > 0)
                        {
                            //locationnamecolumn = c1.GetColumnIndex("Location");
                            //addeddate = c1.GetColumnIndex("Addeddate");
                            int voicecolumn = c1.GetColumnIndex("AudioFileName");

                            do
                            {
                                String AudioFileName = c1.GetString(voicecolumn);
                                if (AudioFileName != "")
                                {
                                    if (AudioFileName.Contains("|"))
                                    {
                                        string[] ArrAudioFileName = AudioFileName.Split('|');
                                        foreach (var item in ArrAudioFileName)
                                        {
                                            if (item.ToString() != "")
                                            {
                                                string AudioFilePath = "";
                                                AudioFilePath = audiopath + "/" + item.ToString();
                                                MediaMetadataRetriever mmr = new MediaMetadataRetriever();
                                                mmr.SetDataSource(AudioFilePath);
                                                duration = mmr.ExtractMetadata(MetadataKey.Duration);
                                                totalduration = totalduration + Convert.ToInt32(duration);
                                                TimeSpan ts = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(duration));
                                                ts = TimeSpan.FromMilliseconds(Convert.ToDouble(totalduration));
                                                hourcount = hourcount + ts.Hours;
                                                mincount = mincount + ts.Minutes;
                                                seccount = seccount + ts.Seconds;



                                            }
                                        }
                                    }
                                    else
                                    {
                                        MediaMetadataRetriever mmr = new MediaMetadataRetriever();
                                        mmr.SetDataSource(audiopath + "/" + AudioFileName);
                                        duration = mmr.ExtractMetadata(MetadataKey.Duration);
                                        totalduration = totalduration + Convert.ToInt32(duration);
                                        TimeSpan ts = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(duration));
                                        ts = TimeSpan.FromMilliseconds(Convert.ToDouble(totalduration));
                                        hourcount = hourcount + ts.Hours;
                                        mincount = mincount + ts.Minutes;
                                        seccount = seccount + ts.Seconds;

                                    }
                                }
                                //mmr.SetDataSource(
                            }
                            while (c1.MoveToNext());

                        }
                        if (hourcount < 10)
                        {
                            hour = "0" + hourcount.ToString();

                        }
                        else
                        {
                            hour = hourcount.ToString();

                        }

                        if (mincount < 10)
                        {
                            min = "0" + mincount.ToString();

                        }
                        else
                        {
                            min = mincount.ToString();

                        }

                        if (seccount < 10)
                        {
                            sec = "0" + seccount.ToString();

                        }
                        else
                        {
                            sec = seccount.ToString();

                        }
                        timeduration = string.Format("{0}:{1}:{2}", hour, min, sec);
                        Typeface tf = Typeface.CreateFromAsset(context.Assets, "Fonts/ROBOTO-LIGHT.TTF");
                        txtBarcodeEntryCount.Typeface = tf;
                        txtBarcodeEntryCount.Invalidate();
                        txtManualEntryCount.Text = manualcount.ToString(); ;

                        txtVoiceLength.Typeface = tf;
                        txtVoiceLength.Invalidate();
                        txtVoiceLength.Text = timeduration;

                        txtBarcodeEntryCount.Typeface = tf;
                        txtBarcodeEntryCount.Invalidate();
                        txtBarcodeEntryCount.Text = barcodecount.ToString();


                        txtManualEntryCountText.Typeface = tf;
                        txtManualEntryCountText.Invalidate();

                        txtVoiceLengthText.Typeface = tf;
                        txtVoiceLengthText.Invalidate();

                        txtBarcodeEntryCountText.Typeface = tf;
                        txtBarcodeEntryCountText.Invalidate();


                        locationdetailsdialog.SetCanceledOnTouchOutside(false);

                        locationdetailsdialog.Show();
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(context, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                    }
                };
                dialog.SetCanceledOnTouchOutside(false);
                dialog.Show();
                i = i + 1;
            }


        }


        private string CreateDirectoryForAudio()
        {
            String path = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryMusic).ToString() + "/ImInventory";
            if (!Directory.Exists(path))
            {

                Directory.CreateDirectory(path);

            }
            return path;
        }

    }


}


