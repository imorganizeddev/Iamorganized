
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Android.App;
using Android.Content;
using Android.Database;
using Android.Database.Sqlite;
using Android.Graphics;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using DK.Ostebaronen.FloatingActionButton;
using IMInventory.iminventory;
using Java.IO;
using Java.Nio;
using System.Text.RegularExpressions;
using Android.Media;

namespace IMInventory
{
    [Activity(Label = "ManualEntry", MainLauncher = false, ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorPortrait)]
    public class ManualEntry : Activity
    {
        long empid;
        long projectid;
        string projectname;
        string clientname;
        string locationname;
        string capturedimagename;
		string EditInventory;
		String EditInventoryID;
        static int currentPreviewcount = 0;
        int number = 0;

        ImageView Cam1;
        ImageView Cam2;
        ImageView Cam3;
        ImageView Cam4;
        Fab _fab;

        Random rnd;
        SQLiteDatabase db = null;
        ISharedPreferencesEditor editor;
        ISharedPreferences prefs;

        protected override void OnCreate(Bundle bundle)
        {
			
            base.OnCreate(bundle);
            this.RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.ManualEntry);

            // ========================= Get Global Shared Data
            prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            empid = prefs.GetLong("EmpID", 0);
            projectid = prefs.GetLong("ProjectID", 0);
			if (projectid == 0) {
				projectid = IMApplication.projectid;
			}
            projectname = prefs.GetString("ProjectName", "");
			if (projectname =="") {
				projectname = IMApplication.projectname;
			}
            clientname = prefs.GetString("ClientName", "");
			if (clientname =="") {
				clientname = IMApplication.clientname;
			}
			ISharedPreferencesEditor editor = prefs.Edit();

			editor.PutLong("ProjectID", projectid);
			editor.PutString("ProjectName", projectname);
			editor.PutString("ClientName", clientname);
			editor.Commit();
			// applies changes synchronously on older APIs
			editor.Apply();
            locationname = prefs.GetString("LocationName", "");
			EditInventoryID= prefs.GetString("EditFromItemList", "");


			if (IMApplication.player != null) {
				IMApplication.player.Stop ();
				IMApplication.player = null;
			}

            // ========================= Get Global Shared Data

            // ========================= Rounded Button
            _fab = FindViewById<Fab>(Resource.Id.btnSave);
            _fab.FabColor = Color.Blue;
            _fab.FabDrawable = Resources.GetDrawable(Resource.Drawable.icon_save2x);
            _fab.Show();
            _fab.Click += delegate
            {
                SaveDataToLocalDataBase();
            };
            // ========================= Rounded Button

            // ========================= Button Event
            ImageView _backtolist = FindViewById<ImageView>(Resource.Id.btnManualBacktolist);
            _backtolist.Click += delegate
            {

                ClearAll();
                try
                {
                    StartActivity(typeof(LocationList));
                }
                catch
                {
                    Toast.MakeText(this, "Oops…something happened, Please try again", ToastLength.Short).Show();

                }
            };
            // ========================= Button Event

            // ========================= Set Font
            Typeface tf = Typeface.CreateFromAsset(Assets, "Fonts/ROBOTO-LIGHT.TTF");

            EditText txtprojectname = FindViewById<EditText>(Resource.Id.txtProjectName);
            txtprojectname.Typeface = tf;
            txtprojectname.Invalidate();

            EditText txtclientname = FindViewById<EditText>(Resource.Id.txtClientName);
            txtclientname.Typeface = tf;
            txtclientname.Invalidate();

            EditText txtlocation = FindViewById<EditText>(Resource.Id.txtLocation);
            txtlocation.Typeface = tf;
            txtlocation.Invalidate();

            EditText txtItemDescription = FindViewById<EditText>(Resource.Id.txtItemDescription);
            txtItemDescription.Typeface = tf;
            txtItemDescription.Invalidate();

            EditText txtbrand = FindViewById<EditText>(Resource.Id.txtBrand);
            txtbrand.Typeface = tf;
            txtbrand.Invalidate();


            EditText txtquantity = FindViewById<EditText>(Resource.Id.txtQuantity);
            txtquantity.Typeface = tf;
            txtquantity.Invalidate();

            EditText txtModelNumber = FindViewById<EditText>(Resource.Id.txtModelNumber);
            txtModelNumber.Typeface = tf;
            txtModelNumber.Invalidate();

            EditText txtUnitCost = FindViewById<EditText>(Resource.Id.txtUnitCost);
            txtUnitCost.Typeface = tf;
            txtUnitCost.Invalidate();

            EditText txtNotes = FindViewById<EditText>(Resource.Id.txtNotes);
            txtNotes.Typeface = tf;
            txtNotes.Invalidate();
            // ========================= Set Font

            txtlocation.SetText(locationname, TextView.BufferType.Editable);
            editor = prefs.Edit();
            txtlocation.TextChanged += delegate
            {
                editor.PutString("LocationName", txtlocation.Text);
                editor.Commit();

            };
            txtprojectname.SetText(projectname, TextView.BufferType.Editable);
            txtclientname.SetText(clientname, TextView.BufferType.Editable);

            Cam1 = FindViewById<ImageView>(Resource.Id.cam1);
            TextView Cam1Path = FindViewById<TextView>(Resource.Id.cam1path);
            Cam1.Click += delegate
            {
				if (IMApplication.camera != null) {
					IMApplication.camera.Release ();
					IMApplication.camera = null;
				}

                if (Cam1Path.Text.Trim() == "")
                {
                    rnd = new Random();
                    number = rnd.Next(10000, 99999);
                    Intent intent = new Intent(Android.Provider.MediaStore.ActionImageCapture);
                    string path = CreateDirectoryForPictures();
                    string picname = String.Format("img_{0}.jpg", number);
                    Java.IO.File objFile = new Java.IO.File(path, picname);
                    intent.PutExtra(Android.Provider.MediaStore.ExtraOutput, Android.Net.Uri.FromFile(objFile));
                    intent.PutExtra("height", 100);

                    capturedimagename = picname;
                    StartActivityForResult(intent, 1);
                }
                else
                {
					string picname = CreateDirectoryForPictures() + "/" + Cam1Path.Text.Trim();
                    OnPreviewImage(picname);
                    currentPreviewcount = 1;
                }
            };
            Cam2 = FindViewById<ImageView>(Resource.Id.cam2);
            TextView Cam2path = FindViewById<TextView>(Resource.Id.cam2path);
            Cam2.Click += delegate
            {
				if (IMApplication.camera != null) {
					IMApplication.camera.Release ();
					IMApplication.camera = null;
				}

                if (Cam2path.Text.Trim() == "")
                {
                    rnd = new Random();
                    number = rnd.Next(10000, 99999);
                    Intent intent = new Intent(Android.Provider.MediaStore.ActionImageCapture);
                    string path = CreateDirectoryForPictures();
                    string picname = String.Format("img_{0}.jpg", number);
                    Java.IO.File objFile = new Java.IO.File(path, picname);
                    intent.PutExtra(Android.Provider.MediaStore.ExtraOutput, Android.Net.Uri.FromFile(objFile));
                    capturedimagename = picname;
                    StartActivityForResult(intent, 2);
                }
                else
                {
					string picname = CreateDirectoryForPictures() + "/" + Cam2path.Text.Trim();
                    OnPreviewImage(picname);
                    currentPreviewcount = 2;
                }
            };

            Cam3 = FindViewById<ImageView>(Resource.Id.cam3);
            TextView Cam3path = FindViewById<TextView>(Resource.Id.cam3path);
            Cam3.Click += delegate
            {
				if (IMApplication.camera != null) {
					IMApplication.camera.Release ();
					IMApplication.camera = null;
				}

                if (Cam3path.Text.Trim() == "")
                {
                    rnd = new Random();
                    number = rnd.Next(10000, 99999);
                    Intent intent = new Intent(Android.Provider.MediaStore.ActionImageCapture);

                    string path = CreateDirectoryForPictures();
                    string picname = String.Format("img_{0}.jpg", number);
                    Java.IO.File objFile = new Java.IO.File(path, picname);
                    intent.PutExtra(Android.Provider.MediaStore.ExtraOutput, Android.Net.Uri.FromFile(objFile));
                    intent.PutExtra(Android.Provider.MediaStore.ExtraSizeLimit, 300);
                    capturedimagename = picname;
                    StartActivityForResult(intent, 3);
                }
                else
                {
					string picname = CreateDirectoryForPictures() + "/" + Cam3path.Text.Trim();
                    OnPreviewImage(picname);
                    currentPreviewcount = 3;
                }
            };

            Cam4 = FindViewById<ImageView>(Resource.Id.cam4);
            TextView Cam4path = FindViewById<TextView>(Resource.Id.cam4path);
            Cam4.Click += delegate
            {
				if (IMApplication.camera != null) {
					IMApplication.camera.Release ();
					IMApplication.camera = null;
				}

                if (Cam4path.Text.Trim() == "")
                {
                    rnd = new Random();
                    number = rnd.Next(10000, 99999);
                    Intent intent = new Intent(Android.Provider.MediaStore.ActionImageCapture);
                    string path = CreateDirectoryForPictures();
                    string picname = String.Format("img_{0}.jpg", number);
                    Java.IO.File objFile = new Java.IO.File(path, picname);
                    intent.PutExtra(Android.Provider.MediaStore.ExtraOutput, Android.Net.Uri.FromFile(objFile));
                    capturedimagename = picname;
                    StartActivityForResult(intent, 4);
                }
                else
                {
					string picname = CreateDirectoryForPictures() + "/" + Cam4path.Text.Trim();
                    OnPreviewImage(picname);
                    currentPreviewcount = 4;
                }
            };

            Button btnSavePreview = FindViewById<Button>(Resource.Id.btnSavePreview);
            btnSavePreview.Click += delegate {
                LinearLayout LayoutManualEntry = FindViewById<LinearLayout>(Resource.Id.LayoutManualEntry);
                LinearLayout LayoutManualPreview = FindViewById<LinearLayout>(Resource.Id.LayoutManualPreview);
                LayoutManualEntry.Visibility = ViewStates.Visible;
                LayoutManualPreview.Visibility = ViewStates.Gone;
            };

            Button btnDiscardPreview = FindViewById<Button>(Resource.Id.btnDiscardPreview);
            btnDiscardPreview.Click += delegate
            {
                if (currentPreviewcount != 0)
                {
                    rnd = new Random();
                    number = rnd.Next(10000, 99999);
                    Intent intent = new Intent(Android.Provider.MediaStore.ActionImageCapture);
                    string path = CreateDirectoryForPictures();
                    string picname = String.Format("img_{0}.jpg", number);
                    Java.IO.File objFile = new Java.IO.File(path, picname);
                    intent.PutExtra(Android.Provider.MediaStore.ExtraOutput, Android.Net.Uri.FromFile(objFile));
                    capturedimagename = picname;
                    StartActivityForResult(intent, currentPreviewcount);
                    currentPreviewcount = 0;
                }

            };
			if (EditInventoryID.Trim() != "") {
				PopulateDataFromDatabase (EditInventoryID);
			}
        }


        #region Save

        private void SaveDataToLocalDataBase()
        {
            try
            {
                string path = CreateDirectoryForPictures();
                EditText txtproname = FindViewById<EditText>(Resource.Id.txtProjectName);

                EditText txtclientname = FindViewById<EditText>(Resource.Id.txtClientName);

                EditText txtlocation = FindViewById<EditText>(Resource.Id.txtLocation);
                EditText img1path = FindViewById<EditText>(Resource.Id.cam1path);
                EditText img2path = FindViewById<EditText>(Resource.Id.cam2path);
                EditText img3path = FindViewById<EditText>(Resource.Id.cam3path);
                EditText img4path = FindViewById<EditText>(Resource.Id.cam4path);
                EditText txtItemDescription = FindViewById<EditText>(Resource.Id.txtItemDescription);
                EditText txtbrand = FindViewById<EditText>(Resource.Id.txtBrand);
                EditText txtquantity = FindViewById<EditText>(Resource.Id.txtQuantity);
                EditText txtModelNumber = FindViewById<EditText>(Resource.Id.txtModelNumber);
                EditText txtUnitCost = FindViewById<EditText>(Resource.Id.txtUnitCost);
                EditText txtNotes = FindViewById<EditText>(Resource.Id.txtNotes);

                string projectname = txtproname.Text;
                string clientname = txtclientname.Text;
                string location = txtlocation.Text;
                Bitmap image1 = null;
                byte[] image1byte = null;
                byte[] image2byte = null;
                byte[] image3byte = null;
                byte[] image4byte = null;

                string description = txtItemDescription.Text;
                string brand = txtbrand.Text;
                string quantity = txtquantity.Text;
                string model = txtModelNumber.Text;
                string unitcost = txtUnitCost.Text;
                string notes = txtNotes.Text;
                string dtnow = DateTime.Now.ToString("MM-dd-yyyy HH:mm");

				//description=description.Replace("'","\'");
				//brand=brand.Replace("'","\'");
				//model=model.Replace("'","\'");
				//notes=notes.Replace("'","\'");
                //string img = img1.Background;
                //this.DeleteDatabase ("ImInventory");
                db = this.OpenOrCreateDatabase("ImInventory", FileCreationMode.Private, null);
                //db.ExecSQL("DROP TABLE IF EXISTS " + "tbl_ManualEntry");
                db.ExecSQL("CREATE TABLE IF NOT EXISTS "
                    + "tbl_Inventory"
					+ " (ID INTEGER PRIMARY KEY AUTOINCREMENT,EmpID INTEGER,ProjectID VARCHAR,ProjectName VARCHAR, ClientName VARCHAR,Location VARCHAR, Image1 VARCHAR , Image2 VARCHAR, Image3 VARCHAR, Image4 VARCHAR," +
                    "ItemDescription VARCHAR, Brand VARCHAR, Quantity VARCHAR, ModelNumber VARCHAR, UnitCost VARCHAR, Notes VARCHAR , Addeddate VARCHAR,BarCodeNumber VARCHAR,AudioFileName VARCHAR,InventoryType VARCHAR" +
                "" +
                "" +
                ");");

				ContentValues values = new ContentValues ();
				values.Put ("EmpID", empid);
				values.Put ("ProjectID", projectid);
				values.Put ("ProjectName", projectname);
				values.Put ("ClientName", clientname);
				values.Put ("Location", location);
				values.Put ("AudioFileName", "");
				values.Put ("Image1", img1path.Text);
				values.Put ("Image2", img2path.Text);
				values.Put ("Image3", img3path.Text);
				values.Put ("Image4", img4path.Text);
				values.Put ("ItemDescription", description);
				values.Put ("Brand", brand);
				values.Put ("Quantity", quantity);
				values.Put ("ModelNumber", model);
				values.Put ("UnitCost", unitcost);
				values.Put ("BarCodeNumber", "");
				values.Put ("Notes", notes);

                //db.ExecSQL("delete from "+ "tbl_ManualEntry");
                if (location.Trim() != "")
                {
					
					if (description!= "") {
						try{
							if(quantity.Trim () !=""){
								if(unitcost==""){

									unitcost="0";
								}
						if(EditInventoryID!=""){
							
							db.Update("tbl_Inventory",values,"ID = "+EditInventoryID,null);


						}
						else{
                   // db.ExecSQL("INSERT INTO "
                     //   + "tbl_Inventory"
						//	+ " (EmpID,ProjectID,ProjectName, ClientName,Location,Image1,Image2,Image3,Image4,ItemDescription,Brand,Quantity,ModelNumber,UnitCost,Notes,Addeddate,BarCodeNumber,AudioFileName,InventoryType)"
							//	+ " VALUES ('" + empid + "','" + projectid + "','" + projectname + "','" + clientname + "','" + location + "','" + img1path.Text + "','" + img2path.Text + "','" + img3path.Text + "','" + img4path.Text + "','" + description + "','" + brand + "','" + quantity + "','" + model + "','" + unitcost + "','" + notes + "','" + dtnow + "','" + "" + "','" + "" + "','1')");
									values.Put ("InventoryType", "1");
									values.Put ("Addeddate", dtnow);
									db.Insert("tbl_Inventory",null,values);
							}
							

						




						txtItemDescription.SetText("", TextView.BufferType.Editable);
						txtbrand.SetText("", TextView.BufferType.Editable);
						txtquantity.SetText("", TextView.BufferType.Editable);
						txtModelNumber.SetText("", TextView.BufferType.Editable);
						txtUnitCost.SetText("", TextView.BufferType.Editable);
						txtNotes.SetText("", TextView.BufferType.Editable);

						TextView lblCam1PicName = FindViewById<TextView>(Resource.Id.lblCam1PicName);
						TextView lblCam2PicName = FindViewById<TextView>(Resource.Id.lblCam2PicName);
						TextView lblCam3PicName = FindViewById<TextView>(Resource.Id.lblCam3PicName);
						TextView lblCam4PicName = FindViewById<TextView>(Resource.Id.lblCam4PicName);

						lblCam1PicName.SetText("", TextView.BufferType.Editable);
						lblCam2PicName.SetText("", TextView.BufferType.Editable);
						lblCam3PicName.SetText("", TextView.BufferType.Editable);
						lblCam4PicName.SetText("", TextView.BufferType.Editable);

						Cam1.SetImageDrawable(null);
						Cam2.SetImageDrawable(null);
						Cam3.SetImageDrawable(null);
						Cam4.SetImageDrawable(null);


						Cam1.SetBackgroundResource(Resource.Drawable.icon_camera2x);
						Cam2.SetBackgroundResource(Resource.Drawable.icon_camera2x);
						Cam3.SetBackgroundResource(Resource.Drawable.icon_camera2x);
						Cam4.SetBackgroundResource(Resource.Drawable.icon_camera2x);
						img1path.SetText("", TextView.BufferType.Editable);
						img2path.SetText("", TextView.BufferType.Editable);
						img3path.SetText("", TextView.BufferType.Editable);
						img4path.SetText("", TextView.BufferType.Editable);

						Toast.MakeText(this, "Data saved successfully", ToastLength.Long).Show();
						EditInventoryID = "";
						editor = prefs.Edit();
						editor.PutString("EditFromItemList", "");
						editor.Commit ();
						editor.Apply ();

							}
							else{
								Toast.MakeText(this, "Please enter quantity", ToastLength.Long).Show();

							}
						}
						catch(Exception ex){


						}
					}
					else{
						

						Toast.MakeText(this, "Please enter description", ToastLength.Long).Show();

					}
                }

                //txtlocation.SetText ("", TextView.BufferType.Editable);
                
                if (location.Trim() != "")
                {
                  

                }
                else
                {

                    Toast.MakeText(this, "Please enter location name", ToastLength.Long).Show();
                }
            }
            catch (Exception ex)
            {

            }
        }

        #endregion
        private void ClearAll() {

            EditText txtproname = FindViewById<EditText>(Resource.Id.txtProjectName);

            EditText txtclientname = FindViewById<EditText>(Resource.Id.txtClientName);

            EditText txtlocation = FindViewById<EditText>(Resource.Id.txtLocation);
            EditText img1path = FindViewById<EditText>(Resource.Id.cam1path);
            EditText img2path = FindViewById<EditText>(Resource.Id.cam2path);
            EditText img3path = FindViewById<EditText>(Resource.Id.cam3path);
            EditText img4path = FindViewById<EditText>(Resource.Id.cam4path);
            EditText txtItemDescription = FindViewById<EditText>(Resource.Id.txtItemDescription);
            EditText txtbrand = FindViewById<EditText>(Resource.Id.txtBrand);
            EditText txtquantity = FindViewById<EditText>(Resource.Id.txtQuantity);
            EditText txtModelNumber = FindViewById<EditText>(Resource.Id.txtModelNumber);
            EditText txtUnitCost = FindViewById<EditText>(Resource.Id.txtUnitCost);
            EditText txtNotes = FindViewById<EditText>(Resource.Id.txtNotes);


            txtItemDescription.SetText("", TextView.BufferType.Editable);
            txtbrand.SetText("", TextView.BufferType.Editable);
            txtquantity.SetText("", TextView.BufferType.Editable);
            txtModelNumber.SetText("", TextView.BufferType.Editable);
            txtUnitCost.SetText("", TextView.BufferType.Editable);
            txtNotes.SetText("", TextView.BufferType.Editable);

            TextView lblCam1PicName = FindViewById<TextView>(Resource.Id.lblCam1PicName);
            TextView lblCam2PicName = FindViewById<TextView>(Resource.Id.lblCam2PicName);
            TextView lblCam3PicName = FindViewById<TextView>(Resource.Id.lblCam3PicName);
            TextView lblCam4PicName = FindViewById<TextView>(Resource.Id.lblCam4PicName);

            lblCam1PicName.SetText("", TextView.BufferType.Editable);
            lblCam2PicName.SetText("", TextView.BufferType.Editable);
            lblCam3PicName.SetText("", TextView.BufferType.Editable);
            lblCam4PicName.SetText("", TextView.BufferType.Editable);

            Cam1.SetImageDrawable(null);
            Cam2.SetImageDrawable(null);
            Cam3.SetImageDrawable(null);
            Cam4.SetImageDrawable(null);


            Cam1.SetBackgroundResource(Resource.Drawable.icon_camera2x);
            Cam2.SetBackgroundResource(Resource.Drawable.icon_camera2x);
            Cam3.SetBackgroundResource(Resource.Drawable.icon_camera2x);
            Cam4.SetBackgroundResource(Resource.Drawable.icon_camera2x);
            img1path.SetText("", TextView.BufferType.Editable);
            img2path.SetText("", TextView.BufferType.Editable);
            img3path.SetText("", TextView.BufferType.Editable);
            img4path.SetText("", TextView.BufferType.Editable);

           // Toast.MakeText(this, "Data saved successfully", ToastLength.Long).Show();
            EditInventoryID = "";
            editor = prefs.Edit();
            editor.PutString("EditFromItemList", "");
            editor.Commit();
            editor.Apply();
        }
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
			try{
            LinearLayout LayoutManualEntry = FindViewById<LinearLayout>(Resource.Id.LayoutManualEntry);
            LinearLayout LayoutManualPreview = FindViewById<LinearLayout>(Resource.Id.LayoutManualPreview);
            LayoutManualEntry.Visibility = ViewStates.Visible;
            LayoutManualPreview.Visibility = ViewStates.Gone;

            string path = CreateDirectoryForPictures() + "/" + capturedimagename;
			string originalimagepath = CreateDirectoryForOriginalPictures () + "/" + capturedimagename;
		     
            if (requestCode == 1 && resultCode == Result.Ok)
            {
					ResizeAndSaveImage(path);
				//Bitmap reducedqualityBitmap = GetResizeBitmap (path, 1000, 1000, false);
					byte[] imagebyte = getByteArrayFromImage (path);
					Bitmap b = BitmapFactory.DecodeByteArray (imagebyte, 0, imagebyte.Length);
					//Bitmap reducedqualityBitmap = GetResizeBitmap (path, 1000, 1000, false);
					Cam1.SetImageBitmap(Bitmap.CreateScaledBitmap(b, 120, 120, false));
                TextView Cam1PicName = FindViewById<TextView>(Resource.Id.lblCam1PicName);
                Cam1PicName.Gravity = GravityFlags.Center;
                string randomnumberfromimage = Regex.Replace(capturedimagename, "[^0-9]+", string.Empty);
                Cam1PicName.SetText(randomnumberfromimage, TextView.BufferType.Editable);
                TextView Cam1Path = FindViewById<TextView>(Resource.Id.cam1path);
                Cam1Path.Text = capturedimagename;
				//resizedBitmap.Dispose();
					b.Recycle();
            }

            if (requestCode == 2 && resultCode == Result.Ok)
            {
					ResizeAndSaveImage(path);

					byte[] imagebyte = getByteArrayFromImage (path);
					Bitmap b = BitmapFactory.DecodeByteArray (imagebyte, 0, imagebyte.Length);
				//Bitmap reducedqualityBitmap = GetResizeBitmap (path, 1000, 1000, false);
					Cam2.SetImageBitmap(Bitmap.CreateScaledBitmap(b, 120, 120, false));
                TextView Cam2PicName = FindViewById<TextView>(Resource.Id.lblCam2PicName);
                Cam2PicName.Gravity = GravityFlags.Center;
                string randomnumberfromimage = Regex.Replace(capturedimagename, "[^0-9]+", string.Empty);
                Cam2PicName.SetText(randomnumberfromimage, TextView.BufferType.Editable);
                TextView Cam2Path = FindViewById<TextView>(Resource.Id.cam2path);
                Cam2Path.Text = capturedimagename;
				//resizedBitmap.Dispose();
					b.Recycle();
            }

            if (requestCode == 3 && resultCode == Result.Ok)
            {
					ResizeAndSaveImage(path);
				//Bitmap reducedqualityBitmap = GetResizeBitmap (path, 1000, 1000, false);
					byte[] imagebyte = getByteArrayFromImage (path);
					Bitmap b = BitmapFactory.DecodeByteArray (imagebyte, 0, imagebyte.Length);
					//Bitmap reducedqualityBitmap = GetResizeBitmap (path, 1000, 1000, false);
					Cam3.SetImageBitmap(Bitmap.CreateScaledBitmap(b, 120, 120, false));
                TextView Cam3PicName = FindViewById<TextView>(Resource.Id.lblCam3PicName);
                Cam3PicName.Gravity = GravityFlags.Center;
                string randomnumberfromimage = Regex.Replace(capturedimagename, "[^0-9]+", string.Empty);
                Cam3PicName.SetText(randomnumberfromimage, TextView.BufferType.Editable);
                TextView Cam3Path = FindViewById<TextView>(Resource.Id.cam3path);
                Cam3Path.Text = capturedimagename;
				//resizedBitmap.Dispose();
					b.Recycle();
            }

            if (requestCode == 4 && resultCode == Result.Ok)
            {
					ResizeAndSaveImage(path);
				//Bitmap reducedqualityBitmap = GetResizeBitmap (path, 1000, 1000, false);
				byte[] imagebyte = getByteArrayFromImage (path);
		         Bitmap b = BitmapFactory.DecodeByteArray (imagebyte, 0, imagebyte.Length);
					//Bitmap reducedqualityBitmap = GetResizeBitmap (path, 1000, 1000, false);
				Cam4.SetImageBitmap(Bitmap.CreateScaledBitmap(b, 120, 120, false));
                TextView Cam4PicName = FindViewById<TextView>(Resource.Id.lblCam4PicName);
                Cam4PicName.Gravity = GravityFlags.Center;
                string randomnumberfromimage = Regex.Replace(capturedimagename, "[^0-9]+", string.Empty);
                Cam4PicName.SetText(randomnumberfromimage, TextView.BufferType.Editable);
                TextView Cam4Path = FindViewById<TextView>(Resource.Id.cam4path);
                Cam4Path.Text = capturedimagename;
				//resizedBitmap.Dispose();
					b.Recycle();
				}
			}
			catch (Exception ex){
				Toast.MakeText(this, "Something went wrong. Please try back in few minutes."+ex.Message, ToastLength.Long).Show();
			}
        }
		private void ResizeAndSaveImage(string path){
			
			//File dir=Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM);

			try{
		
				Matrix mtx = new Matrix();
				ExifInterface exif = new ExifInterface(path);
				var orientation = (Android.Media.Orientation)exif.GetAttributeInt(ExifInterface.TagOrientation, (int)Android.Media.Orientation.Undefined);
				int rotate = 0;
				switch(orientation){


				case Android.Media.Orientation.Undefined: // Nexus 7 landscape...
					break;

				case Android.Media.Orientation.Normal: // landscape
					break;

				case Android.Media.Orientation.FlipHorizontal:
					break;

				case Android.Media.Orientation.Rotate180:
					rotate=180;
					break;
				case Android.Media.Orientation.FlipVertical:
					break;

				case Android.Media.Orientation.Transpose:
					break;

				case Android.Media.Orientation.Rotate90:
					rotate=90;
					break;

				case Android.Media.Orientation.Transverse:
					break;

				case Android.Media.Orientation.Rotate270: 
					rotate=270;
					break;
				
				}

				Bitmap b= BitmapFactory.DecodeFile(path);		
				System.IO.File.Delete (path);
				Bitmap outbitmap = null;

				if (b.Height > b.Width) {
					outbitmap = Bitmap.CreateScaledBitmap (b, 768, 1024, false);
				} else { 

					outbitmap = Bitmap.CreateScaledBitmap (b, 1024, 768, false);
				}
				System.IO.File.Delete(path);


			try {

				mtx.PreRotate(rotate);
				outbitmap=Bitmap.CreateBitmap(outbitmap, 0, 0, outbitmap.Width, outbitmap.Height, mtx, false); 
				
				Java.IO.File file = new Java.IO.File(path);
				var fOut = new FileStream(path, FileMode.Create);
				outbitmap.Compress(Bitmap.CompressFormat.Jpeg, 75, fOut);
				fOut.Close();
				b.Recycle();
				outbitmap.Recycle();   
				mtx.Dispose();
				mtx = null;
			}
			catch {
			}
			} 

			catch (Exception e) {


			}
		}
        protected void OnPreviewImage (string ImageResource_Cam)
		{
			LinearLayout LayoutManualEntry = FindViewById<LinearLayout> (Resource.Id.LayoutManualEntry);
			LinearLayout LayoutManualPreview = FindViewById<LinearLayout> (Resource.Id.LayoutManualPreview);
			ImageView ManualCamImagePreview = FindViewById<ImageView> (Resource.Id.ManualCamImagePreview);
			LayoutManualEntry.Visibility = ViewStates.Gone;
			LayoutManualPreview.Visibility = ViewStates.Visible;

			if (System.IO.File.Exists (ImageResource_Cam)) {
				try {
					Java.IO.File imageFile = new Java.IO.File (ImageResource_Cam);
					Bitmap bitmap = BitmapFactory.DecodeFile (imageFile.AbsolutePath);
					ManualCamImagePreview.SetImageBitmap (bitmap);
					bitmap.Dispose ();
				} catch (Exception ex) {
					Toast.MakeText (this, "Opps ! Something went wrong. Please check your phone memory usage and try again. Please problem remains contact support and report error " + ex.Message, ToastLength.Long).Show ();
				}
			}
		}

        #region Image Private Methods

        private Bitmap GetResizeBitmap(string path, int reqheight, int reqwidth, bool crop)
        {
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InJustDecodeBounds = false;
            options.InPreferredConfig = Android.Graphics.Bitmap.Config.Rgb565;
            options.InDither = true;
            options.InPurgeable = true;
           
            Bitmap photo = null;
            photo = BitmapFactory.DecodeFile(path, options);

            Matrix m = new Matrix();
            m.SetRectToRect(new RectF(0, 0, photo.Width, photo.Height), new RectF(0, 0, reqheight, reqwidth), Matrix.ScaleToFit.Center);

            Bitmap resizedBitmap = null;
            if (crop)
            {
                resizedBitmap = Bitmap.CreateBitmap(photo, 0, 0, reqwidth, reqheight);
            }
            else
            {
                resizedBitmap = Bitmap.CreateBitmap(photo, 0, 0, photo.Width, photo.Height, m, true);
            }

			//photo.Dispose ();
			photo.Recycle ();
            return resizedBitmap;
        }
	
        private byte[] getByteArrayFromImage(String filePath)
        {
            byte[] bytes = null;
            try
            {
                Java.IO.File file = new Java.IO.File(filePath);

                FileInputStream fis = new FileInputStream(file);
                //create FileInputStream which obtains input bytes from a file in a file system
                //FileInputStream is meant for reading streams of raw bytes such as image data. For reading streams of characters, consider using FileReader.

                ByteArrayOutputStream bos = new ByteArrayOutputStream();
                byte[] buf = new byte[1024];

                for (int readNum; (readNum = fis.Read(buf)) != -1; )
                {
                    bos.Write(buf, 0, readNum);
                    //no doubt here is 0
                    /*Writes len bytes from the specified byte array starting at offset 
                off to this byte array output stream.*/
                }

                bytes = bos.ToByteArray();
            }
            catch
            {
                return bytes;
            }
            return bytes;
        }
        private string CreateDirectoryForPictures()
        {
			
			//String path =System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal) + "/Images/ImInventory";
			String path = Android.OS.Environment.GetExternalStoragePublicDirectory (Android.OS.Environment.DirectoryPictures).ToString()+"/ImInventory";
            if (!Directory.Exists(path))
            {

                Directory.CreateDirectory(path);

            }
            return path;
        }
		private string CreateDirectoryForOriginalPictures()
		{
			String path = Android.OS.Environment.GetExternalStoragePublicDirectory (Android.OS.Environment.DirectoryPictures).ToString()+"/ImInventory/Original";
			if (! Directory.Exists (path)) {

				Directory.CreateDirectory (path);

			}
			return path;
		}

		private void PopulateDataFromDatabase(String ID){
			//ProjectName VARCHAR, ClientName
			//Image1,Image2,Image3,Image4,ItemDescription,Brand,Quantity,
			//ModelNumber,UnitCost,Notes,Addeddate,BarCodeNumber,AudioFileName,InventoryType)"" +
			//	"" +
			//	"
			db = this.OpenOrCreateDatabase("ImInventory", FileCreationMode.Private, null);
			ICursor c = db.RawQuery("SELECT * FROM " + "tbl_Inventory WHERE ID = "+ID,null);
			int projectcolumn = 0;
			int clientcolumn = 0;
			int locationnamecolumn = 0;
			int clientnamecolumn = 0;
			int image1column = 0;
			int image2column = 0;
			int image3column = 0;
			int image4column = 0;
			int itemdescriptioncolumn = 0;
			int brandcolumn = 0;
			int quantitycolumn = 0;
			int modelnumbercolumn = 0;
			int unitCostcolumn = 0;
			int notescolumn = 0;
			c.MoveToFirst();
			projectcolumn = c.GetColumnIndex ("ProjectName");
			clientcolumn = c.GetColumnIndex ("ClientName");
			locationnamecolumn = c.GetColumnIndex ("Location");
			clientnamecolumn = c.GetColumnIndex ("ClientName");
			image1column = c.GetColumnIndex ("Image1");
			image2column = c.GetColumnIndex ("Image2");
			image3column = c.GetColumnIndex ("Image3");
			image4column = c.GetColumnIndex ("Image4");
			itemdescriptioncolumn = c.GetColumnIndex ("ItemDescription");
			brandcolumn = c.GetColumnIndex ("Brand");
			quantitycolumn = c.GetColumnIndex ("Quantity");
			modelnumbercolumn = c.GetColumnIndex ("ModelNumber");
			unitCostcolumn = c.GetColumnIndex ("UnitCost");
			notescolumn = c.GetColumnIndex ("Notes");

			String Projectname = c.GetString(projectcolumn);
			String Clientname = c.GetString(clientcolumn);
			String Locationname = c.GetString(locationnamecolumn);
			String Image1name = c.GetString(image1column);
			String Image2name = c.GetString(image2column);
			String Image3name = c.GetString(image3column);
			String Image4name = c.GetString(image4column);
			String Itemdescription = c.GetString(itemdescriptioncolumn);
			String Brand = c.GetString(brandcolumn);
			String Quantity = c.GetString(quantitycolumn);
			String Modelnumber = c.GetString(modelnumbercolumn);
			String UnitCost = c.GetString(unitCostcolumn);
			String Notes = c.GetString(notescolumn);


			EditText txtproname = FindViewById<EditText>(Resource.Id.txtProjectName);

			EditText txtclientname = FindViewById<EditText>(Resource.Id.txtClientName);

			EditText txtlocation = FindViewById<EditText>(Resource.Id.txtLocation);
			EditText img1path = FindViewById<EditText>(Resource.Id.cam1path);
			EditText img2path = FindViewById<EditText>(Resource.Id.cam2path);
			EditText img3path = FindViewById<EditText>(Resource.Id.cam3path);
			EditText img4path = FindViewById<EditText>(Resource.Id.cam4path);
			EditText txtItemDescription = FindViewById<EditText>(Resource.Id.txtItemDescription);
			EditText txtbrand = FindViewById<EditText>(Resource.Id.txtBrand);
			EditText txtquantity = FindViewById<EditText>(Resource.Id.txtQuantity);
			EditText txtModelNumber = FindViewById<EditText>(Resource.Id.txtModelNumber);
			EditText txtUnitCost = FindViewById<EditText>(Resource.Id.txtUnitCost);
			EditText txtNotes = FindViewById<EditText>(Resource.Id.txtNotes);
			TextView Cam1PicName = FindViewById<TextView>(Resource.Id.lblCam1PicName);
			Cam1PicName.Gravity = GravityFlags.Center;
			TextView Cam2PicName = FindViewById<TextView>(Resource.Id.lblCam2PicName);
			Cam2PicName.Gravity = GravityFlags.Center;
			TextView Cam3PicName = FindViewById<TextView>(Resource.Id.lblCam3PicName);
			Cam3PicName.Gravity = GravityFlags.Center;
			TextView Cam4PicName = FindViewById<TextView>(Resource.Id.lblCam4PicName);
			Cam4PicName.Gravity = GravityFlags.Center;

			txtproname.Text = Projectname;
			txtclientname.Text = Clientname;
			txtlocation.Text = Locationname;
			string randomnumberfromimage = "";
			if (Image1name != "") {
				img1path.Text = Image1name;
				randomnumberfromimage = Regex.Replace (Image1name, "[^0-9]+", string.Empty);
				Cam1PicName.Text = randomnumberfromimage;
				Cam1.SetImageBitmap (GetResizeBitmap (CreateDirectoryForPictures () + "/" + Image1name, 120, 120, true));
			}
			if (Image2name != "") {
				img2path.Text = Image2name;
				randomnumberfromimage = Regex.Replace (Image2name, "[^0-9]+", string.Empty);
				Cam2PicName.Text = randomnumberfromimage;
				Cam2.SetImageBitmap (GetResizeBitmap (CreateDirectoryForPictures () + "/" + Image2name, 120, 120, true));
			}
			if (Image3name != "") {
				img3path.Text = Image3name;
				randomnumberfromimage = Regex.Replace (Image3name, "[^0-9]+", string.Empty);
				Cam3PicName.Text = randomnumberfromimage;
				Cam3.SetImageBitmap (GetResizeBitmap (CreateDirectoryForPictures () + "/" + Image3name, 120, 120, true));
			}
			if (Image4name != "") {
				img4path.Text = Image4name;
				randomnumberfromimage = Regex.Replace (Image4name, "[^0-9]+", string.Empty);
				Cam4PicName.Text = randomnumberfromimage;
				Cam4.SetImageBitmap (GetResizeBitmap (CreateDirectoryForPictures () + "/" + Image4name, 120, 120, true));
			}
			txtItemDescription.Text = Itemdescription;
			txtbrand.Text = Brand;

			txtquantity.Text = Quantity;
			txtModelNumber.Text = Modelnumber;
			if (UnitCost.Trim () == "0") {
				UnitCost = "";
			}
			txtUnitCost.Text = UnitCost;
			txtNotes.Text = Notes;

		}

		public override void OnBackPressed ()
		{
			//this.Finish ();
			//StartActivity (typeof (Main));
		}
        #endregion
    }
}

