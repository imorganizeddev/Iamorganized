
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
using Android.Bluetooth;
using Java.Util;
using Java.Lang.Reflect;
using System.Net;

using System.IO;
using Android.Graphics;
using Android.Database.Sqlite;
using Android.Database;
using System.Xml;
using Android.Preferences;
using IMInventory.iminventory;
using DK.Ostebaronen.FloatingActionButton;
using Android.Net.Wifi;
using Xamarin.InAppBilling.Utilities;
using Xamarin.InAppBilling;
using System.Threading.Tasks;
using System.Data;
using Android.Content.Res;

namespace IMInventory
{
    [Activity(Label = "BarcodeEntry", ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorPortrait, MainLauncher = false)]
    public class BarcodeEntry : BaseClass
    {

        SQLiteDatabase db = null;
        long empid;
        long projectid;
        string projectname;
        string clientname;
        int AlertVibrate;
        int AlertTone;
        private Fab _fab;
		private ImageView _fabbacktolist;
        ISharedPreferencesEditor editor;
        ISharedPreferences prefs;
        string locationname;
        string AccessFindITExport = "";
        string AccessExport = "";
        string UserPlan = "";
        public Android.Hardware.Camera camera;
        public Android.Hardware.Camera.Parameters parameters;
        System.Timers.Timer tmr;
        public ZXing.Mobile.MobileBarcodeScanner scanner;
        String EditInventoryID;
        string AccessBarCodeScan = "";
        IList<Product> _products;
        InAppBillingServiceConnection _serviceConnection;
        protected override void OnCreate(Bundle bundle)
        {
			StartSetup ();
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.BarcodeEntry);

            prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            locationname = prefs.GetString("LocationName", "");
            EditInventoryID = prefs.GetString("EditFromItemListForBarcode", "");
            AccessBarCodeScan = prefs.GetString("AccessBarCodeScan", "");
            AccessFindITExport = prefs.GetString("AccessFindITExport", "");
            AccessExport = prefs.GetString("AccessExport", "");
            editor = prefs.Edit();

            _fab = FindViewById<Fab>(Resource.Id.btnSaveBarCode);
            _fab.FabColor = Color.Blue;
            _fab.FabDrawable = Resources.GetDrawable(Resource.Drawable.icon_save2x);
            _fab.Show();
			_fabbacktolist = FindViewById<ImageView>(Resource.Id.btnBarBacktolist);
            
           

            empid = prefs.GetLong("EmpID", 0);
            projectid = prefs.GetLong("ProjectID", 0);
            projectname = prefs.GetString("ProjectName", "");
            clientname = prefs.GetString("ClientName", "");
            AlertVibrate = prefs.GetInt("AlertVibrate", 0);
            AlertTone = prefs.GetInt("AlertTone", 0);
            editor = prefs.Edit();

			SetTypeFace();


        }



        #region Events
        protected void tmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            scanner.ToggleTorch();
            tmr.Stop();
        }

        #endregion

        #region Method

        private void SetTypeFace()
        {
            Typeface tf = Typeface.CreateFromAsset(Assets, "Fonts/ROBOTO-LIGHT.TTF");
            EditText txtclientname = FindViewById<EditText>(Resource.Id.txtbarClientName);
            txtclientname.Typeface = tf;
            txtclientname.Invalidate();

            EditText txtprojectname = FindViewById<EditText>(Resource.Id.txtbarProjectname);
            txtprojectname.Typeface = tf;
            txtprojectname.Invalidate();

            EditText txtlocation = FindViewById<EditText>(Resource.Id.txtbarLocation);
            txtlocation.Typeface = tf;
            txtlocation.Invalidate();
            txtlocation.SetText(locationname, TextView.BufferType.Editable);
            EditText txtquantity = FindViewById<EditText>(Resource.Id.txtbarquantity);
            txtquantity.Typeface = tf;
            txtquantity.Invalidate();

            EditText txtUnitCost = FindViewById<EditText>(Resource.Id.txtbarunitcost);
            txtUnitCost.Typeface = tf;
            txtUnitCost.Invalidate();

            TextView lblbluetoothscan = FindViewById<TextView>(Resource.Id.lblbluetoothscan);
            lblbluetoothscan.Typeface = tf;
            lblbluetoothscan.Invalidate();

            TextView lblscanbarcode = FindViewById<TextView>(Resource.Id.lblscanbarcode);
            lblscanbarcode.Typeface = tf;
            lblscanbarcode.Invalidate();

			TextView txtbaritemname = FindViewById<TextView>(Resource.Id.txtbaritemname);
			txtbaritemname.Typeface = tf;
			txtbaritemname.Invalidate();

            //Typeface tfArial = Typeface.CreateFromAsset (Assets, "Fonts/ARIALBOLD.TTF");
            EditText txtbarcodenumber = FindViewById<EditText>(Resource.Id.txtbarbarcodenumber);
            //txtbarcodenumber.Typeface = tfArial;
            //txtbarcodenumber.Invalidate ();

            txtlocation.TextChanged += delegate
            {

                editor.PutString("LocationName", txtlocation.Text);
                editor.Commit();
            };

            txtprojectname.SetText(projectname, TextView.BufferType.Editable);
            txtclientname.SetText(clientname, TextView.BufferType.Editable);

            Switch swscanbluetooth = FindViewById<Switch>(Resource.Id.swscanbluetooth);
            ImageView imgscanbarcode = FindViewById<ImageView>(Resource.Id.imgScanBarcode);
            LinearLayout llscanbarcode = FindViewById<LinearLayout>(Resource.Id.llscanbarcode);
            LinearLayout llafterscanbarcode = FindViewById<LinearLayout>(Resource.Id.llafterscan);

            imgscanbarcode.Click += async (sender, e) =>
            {
                try
                {
                   // if (AccessBarCodeScan.Trim() == "1")
                  //  {

                       // var options = new ZXing.Mobile.MobileBarcodeScanningOptions();
                        scanner = new ZXing.Mobile.MobileBarcodeScanner(this);
                       // scanner.FlashButtonText = "On Flash";
                       // scanner.Torch(true);
                        if (IMApplication.camera != null)
                        {
                            IMApplication.camera.Release();
                            IMApplication.camera = null;
                        }

                        var result = await scanner.Scan();
                        if (result != null)
                        {

                            llafterscanbarcode.Visibility = ViewStates.Visible;
                            llscanbarcode.Visibility = ViewStates.Gone;
                            txtbarcodenumber.SetText(result.Text, TextView.BufferType.Editable);

                            AlertVibrate = prefs.GetInt("AlertVibrate", 0);
                            AlertTone = prefs.GetInt("AlertTone", 0);
                            // Vibrate Device
                            if (AlertVibrate == 1)
                                VibrateDevice();
                            // Play tone
                            if (AlertTone == 1)
                                PlayNitificationTone();
                        }
                   // }
                  //  else {
                       // OpenInAppPurchasePopUp();
                  //  }
                    
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                }

            };

            ImageView imgScanBarcodeSmall = FindViewById<ImageView>(Resource.Id.imgScanBarcodeSmall);
            imgScanBarcodeSmall.Click += delegate
            {
                try
                {
                    llafterscanbarcode.Visibility = ViewStates.Gone;
                    llscanbarcode.Visibility = ViewStates.Visible;
                    txtbarcodenumber.SetText("", TextView.BufferType.Editable);
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                }
            };

            swscanbluetooth.CheckedChange += delegate(object sender, CompoundButton.CheckedChangeEventArgs e)
            {

                try
                {
                    bool boolchecked = e.IsChecked;
                    if (boolchecked)
                    {
                        BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;

                        if (adapter == null)
                        {
                            Toast.MakeText(this, "No Bluetooth adapter found.", ToastLength.Long).Show();

                        }
                        if (!adapter.IsEnabled)
                        {
                            //throw new Exception("Bluetooth adapter is not enabled.");
                            Toast.MakeText(this, "Bluetooth adapter is not enabled. TurningOn Bluetooth ", ToastLength.Long).Show();
                            adapter.Enable();
                            llafterscanbarcode.Visibility = ViewStates.Visible;
                            llscanbarcode.Visibility = ViewStates.Gone;
                            txtbarcodenumber.RequestFocus();
                        }
                        else
                        {

                            llafterscanbarcode.Visibility = ViewStates.Visible;
                            llscanbarcode.Visibility = ViewStates.Gone;
                            txtbarcodenumber.RequestFocus();
                        }

                    }
                    else
                    {
                        llafterscanbarcode.Visibility = ViewStates.Gone;
                        llscanbarcode.Visibility = ViewStates.Visible;

                    }
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                }
            };

            _fab.Click += delegate
            {
                try
                {
                    SaveDataToLocalDataBase();
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                }
            };
            _fabbacktolist.Click += delegate
            {
                try
                {
                    txtquantity = FindViewById<EditText>(Resource.Id.txtbarquantity);
                    txtUnitCost = FindViewById<EditText>(Resource.Id.txtbarunitcost);
                    txtbarcodenumber = FindViewById<EditText>(Resource.Id.txtbarbarcodenumber);
					txtbaritemname = FindViewById<EditText>(Resource.Id.txtbaritemname);
                    llscanbarcode = FindViewById<LinearLayout>(Resource.Id.llscanbarcode);
                    llafterscanbarcode = FindViewById<LinearLayout>(Resource.Id.llafterscan);
                    txtquantity.SetText("", TextView.BufferType.Editable);
                    txtUnitCost.SetText("", TextView.BufferType.Editable);
                    txtbarcodenumber.SetText("", TextView.BufferType.Editable);
					txtbaritemname.SetText("", TextView.BufferType.Editable);
                    llafterscanbarcode.Visibility = ViewStates.Gone;
                    llscanbarcode.Visibility = ViewStates.Visible;
                    EditInventoryID = "";
                    editor.PutString("EditFromItemListForBarcode", "");
                    editor.Commit();
                    editor.Apply();
                    StartActivity(typeof(LocationList));
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                }
            };
            if (EditInventoryID.Trim() != "")
            {
                PopulateDataFromDatabase(EditInventoryID);
            }
        }


        private void SaveDataToLocalDataBase()
        {

            EditText txtproname = FindViewById<EditText>(Resource.Id.txtbarProjectname);
            EditText txtclientname = FindViewById<EditText>(Resource.Id.txtbarClientName);
            EditText txtlocation = FindViewById<EditText>(Resource.Id.txtbarLocation);
            EditText txtquantity = FindViewById<EditText>(Resource.Id.txtbarquantity);
            EditText txtUnitCost = FindViewById<EditText>(Resource.Id.txtbarunitcost);
            EditText txtbarcodenumber = FindViewById<EditText>(Resource.Id.txtbarbarcodenumber);
			EditText txtbaritemname = FindViewById<EditText>(Resource.Id.txtbaritemname);
            LinearLayout llscanbarcode = FindViewById<LinearLayout>(Resource.Id.llscanbarcode);
            LinearLayout llafterscanbarcode = FindViewById<LinearLayout>(Resource.Id.llafterscan);

            string projectname = txtproname.Text;
            string clientname = txtclientname.Text;
            string location = txtlocation.Text;
            string quantity = txtquantity.Text;
            string unitcost = txtUnitCost.Text;
			string itemname = txtbaritemname.Text;
            string barcodenumber = txtbarcodenumber.Text;
            string dtnow = DateTime.Now.ToString("MM-dd-yyyy HH:mm");
            db = this.OpenOrCreateDatabase("ImInventory", FileCreationMode.Private, null);
            db.ExecSQL("CREATE TABLE IF NOT EXISTS "
                + "tbl_Inventory"
                + " (ID INTEGER PRIMARY KEY AUTOINCREMENT,EmpID INTEGER,ProjectID VARCHAR, ProjectName VARCHAR, ClientName VARCHAR,Location VARCHAR, Image1 VARCHAR , Image2 VARCHAR, Image3 VARCHAR, Image4 VARCHAR," +
                "ItemDescription VARCHAR, Brand VARCHAR, Quantity VARCHAR, ModelNumber VARCHAR, UnitCost VARCHAR, Notes VARCHAR , Addeddate VARCHAR, AudioFileName VARCHAR,BarCodeNumber VARCHAR,InventoryType VARCHAR" +
                "" +
                "" +
                ");");
			ContentValues values = new ContentValues ();
			values.Put ("EmpID", empid);
			values.Put ("ProjectID", projectid);
			values.Put ("ProjectName", projectname);
			values.Put ("ClientName", clientname);
			values.Put ("Location", location);
			values.Put ("Quantity", quantity);
			values.Put ("UnitCost", unitcost);
			values.Put ("BarCodeNumber", barcodenumber);
			values.Put ("AudioFileName", "");
			values.Put ("Image1", "");
			values.Put ("Image2", "");
			values.Put ("Image3", "");
			values.Put ("Image4", "");
			values.Put ("ItemDescription", "");
			values.Put ("Notes", itemname);
			values.Put ("Brand", "");
            if (location.Trim() != "")
            {
				
				if (barcodenumber != "")
                {
					if (quantity.Trim() != "") {
						if (unitcost == "") {
							unitcost="0";
						}
							
							if (EditInventoryID != "") {
								
								db.Update ("tbl_Inventory", values, "ID = " + EditInventoryID, null);

							} else {
							values.Put ("InventoryType", "3");
							values.Put ("Addeddate", dtnow);
							db.Insert("tbl_Inventory",null,values);
							}

							txtquantity.SetText ("", TextView.BufferType.Editable);
							txtUnitCost.SetText ("", TextView.BufferType.Editable);
							txtbarcodenumber.SetText ("", TextView.BufferType.Editable);
						    txtbaritemname.SetText ("", TextView.BufferType.Editable);
							llafterscanbarcode.Visibility = ViewStates.Gone;
							llscanbarcode.Visibility = ViewStates.Visible;
							Toast.MakeText (this, "Data saved successfully", ToastLength.Long).Show ();
							EditInventoryID = "";
							editor.PutString ("EditFromItemListForBarcode", "");
							editor.Commit ();
							editor.Apply ();
						
					} else {
						Toast.MakeText (this, "Please enter quantity", ToastLength.Long).Show ();
					}

                }
                else
                {
					
					Toast.MakeText(this, "Please enter barcodenumber", ToastLength.Long).Show();

                }
            }


            if (location.Trim() != "")
            {

            }
            else
            {
                Toast.MakeText(this, "Please enter location name", ToastLength.Long).Show();
            }
        }

        private void PopulateDataFromDatabase(String ID)
        {
            //ProjectName VARCHAR, ClientName
            //Image1,Image2,Image3,Image4,ItemDescription,Brand,Quantity,
            //ModelNumber,UnitCost,Notes,Addeddate,BarCodeNumber,AudioFileName,InventoryType)"" +
            //	"" +
            //	"
            db = this.OpenOrCreateDatabase("ImInventory", FileCreationMode.Private, null);
            ICursor c = db.RawQuery("SELECT * FROM " + "tbl_Inventory WHERE ID = " + ID, null);
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
            int barcodenumbercolumn = 0;
            c.MoveToFirst();
            projectcolumn = c.GetColumnIndex("ProjectName");
            clientcolumn = c.GetColumnIndex("ClientName");
            locationnamecolumn = c.GetColumnIndex("Location");
            clientnamecolumn = c.GetColumnIndex("ClientName");
            image1column = c.GetColumnIndex("Image1");
            image2column = c.GetColumnIndex("Image2");
            image3column = c.GetColumnIndex("Image3");
            image4column = c.GetColumnIndex("Image4");
            itemdescriptioncolumn = c.GetColumnIndex("ItemDescription");
            brandcolumn = c.GetColumnIndex("Brand");
            quantitycolumn = c.GetColumnIndex("Quantity");
            modelnumbercolumn = c.GetColumnIndex("ModelNumber");
            unitCostcolumn = c.GetColumnIndex("UnitCost");
            notescolumn = c.GetColumnIndex("Notes");
            barcodenumbercolumn = c.GetColumnIndex("BarCodeNumber");

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
            String Barcodenumber = c.GetString(barcodenumbercolumn);


            EditText txtproname = FindViewById<EditText>(Resource.Id.txtbarProjectname);

            EditText txtclientname = FindViewById<EditText>(Resource.Id.txtbarClientName);

            EditText txtlocation = FindViewById<EditText>(Resource.Id.txtbarLocation);


            EditText txtquantity = FindViewById<EditText>(Resource.Id.txtbarquantity);

            EditText txtUnitCost = FindViewById<EditText>(Resource.Id.txtbarunitcost);
            EditText txtbarcodenumber = FindViewById<EditText>(Resource.Id.txtbarbarcodenumber);
            LinearLayout llscanbarcode = FindViewById<LinearLayout>(Resource.Id.llscanbarcode);
            LinearLayout llafterscanbarcode = FindViewById<LinearLayout>(Resource.Id.llafterscan);
            txtproname.Text = Projectname;
            txtclientname.Text = Clientname;
            txtlocation.Text = Locationname;
            txtquantity.Text = Quantity;
			if (UnitCost.Trim () == "0") {
				UnitCost = "";
			}
            txtUnitCost.Text = UnitCost;
            if (Barcodenumber != "")
            {
                llafterscanbarcode.Visibility = ViewStates.Visible;
                llscanbarcode.Visibility = ViewStates.Gone;
                txtbarcodenumber.Text = Barcodenumber;
            }



        }
        public override void OnBackPressed()
        {

        }
        private void OpenInAppPurchasePopUp()
        {
            
            WifiManager wifiManager = (WifiManager)GetSystemService(Context.WifiService);
            WifiInfo wInfo = wifiManager.ConnectionInfo;
            String MACAdress = Android.Provider.Settings.Secure.GetString(ContentResolver, Android.Provider.Settings.Secure.AndroidId);

            AlertDialog.Builder builder = new AlertDialog.Builder(this);
			builder.SetMessage("To access this feature you will need to make a purchase of $1.99 for the lifetime use on this device. Would you like to continue and purchase this add-on?");
            builder.SetCancelable(false);
            builder.SetPositiveButton("Yes", (object sender, DialogClickEventArgs e) =>
            {
                try
                {
                    _serviceConnection.BillingHandler.BuyProduct(_products[0]);
                }
                catch(Exception ex)
                {

                }
            });
            builder.SetNegativeButton("No", (object sender, DialogClickEventArgs e) =>
            {

            });
            AlertDialog alertdialog = builder.Create();
            alertdialog.Show();

        }

        #region InAppPurchase


        private void CreateInAppPurchasePopUp()
        {

            // StartSetup();

        }
        public void StartSetup()
        {
			if (InAppPurchase._products != null) {
				_products = InAppPurchase._products;
				_serviceConnection = InAppPurchase._serviceConnection;
			}
		
        }

       
        private async Task GetInventory()
        {
            
            _products = await _serviceConnection.BillingHandler.QueryInventoryAsync(new List<string> {
				"bar_code",
				ReservedTestProductIDs.Purchased
			}, ItemType.Product);
            // Were any products returned?
            if (_products == null)
            {
                // No, abort
                return;
            }

        }

        private void LoadPurchasedItems()
        {
            // Ask the open connection's billing handler to get any purchases
            var purchases = _serviceConnection.BillingHandler.GetPurchases(ItemType.Product);

            // Display any existing purchases

        }

        private void UpdatePurchasedItems()
        {            // Ask the open connection's billing handler to get any purchases
            var purchases = _serviceConnection.BillingHandler.GetPurchases(ItemType.Product);
            _serviceConnection.BillingHandler.ConsumePurchase(purchases[0]);
            // Is there a data adapter for purchases?

        }
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                _serviceConnection.BillingHandler.HandleActivityResult(requestCode, resultCode, data);
                WifiManager wifiManager = (WifiManager)GetSystemService(Context.WifiService);
                WifiInfo wInfo = wifiManager.ConnectionInfo;
                String MACAdress = Android.Provider.Settings.Secure.GetString(ContentResolver, Android.Provider.Settings.Secure.AndroidId);
                //TODO: Use a call back to update the purchased items
                UpdatePurchasedItems();

                if (resultCode == Result.Ok)
                {

                    WebService objWebService = new WebService();
                    objWebService.ExternalSignUpUserSubscriptionAsync(empid.ToString(), MACAdress, AccessFindITExport, "1", AccessExport);
                    // objWebService.GetEmpDetailsByEmpIDAsync(empid.ToString(), MACAdress);
                    //objWebService.GetEmpDetailsByEmpIDCompleted += getempxml;
                    objWebService.ExternalSignUpUserSubscriptionCompleted += getExternalSignUpUserSubscriptionxml;

                    // long sizetoupload = -102400 + Convert.ToInt64(UploadedSize);
                    //  objWebService.FreeUserUploadSizeUpdateAsync(empid.ToString(), MACAdress, sizetoupload.ToString());



                }
                else
                {
                    WebService objWebService = new WebService();
                    objWebService.ExternalSignUpUserSubscriptionAsync(empid.ToString(), MACAdress, AccessFindITExport, "0", AccessExport);
                    // objWebService.GetEmpDetailsByEmpIDAsync(empid.ToString(), MACAdress);
                    //objWebService.GetEmpDetailsByEmpIDCompleted += getempxml;
                    objWebService.ExternalSignUpUserSubscriptionCompleted += getExternalSignUpUserSubscriptionxml;
                }
            }
            catch { }
        }
       
        void getExternalSignUpUserSubscriptionxml(object sender, ExternalSignUpUserSubscriptionCompletedEventArgs e)
        {
            WifiManager wifiManager = (WifiManager)GetSystemService(Context.WifiService);
            WifiInfo wInfo = wifiManager.ConnectionInfo;
            String MACAdress = Android.Provider.Settings.Secure.GetString(ContentResolver, Android.Provider.Settings.Secure.AndroidId);
            WebService objWebService = new WebService();
            objWebService.GetEmpDetailsByEmpIDAsync(empid.ToString(), MACAdress);
            objWebService.GetEmpDetailsByEmpIDCompleted += getempxml;

        }
        private void getempxml(object sender, GetEmpDetailsByEmpIDCompletedEventArgs e)
        {

            try
            {
                DataSet ds = new DataSet();
                string innerxml = e.Result.InnerXml.ToString();
                innerxml = "<ds><table1>" + innerxml + "</table1></ds>";
                DataTable dataTable = new DataTable("table1");
                dataTable.Columns.Add("EmpID", typeof(string));
                dataTable.Columns.Add("Name", typeof(string));
                dataTable.Columns.Add("Email", typeof(string));
                dataTable.Columns.Add("Company", typeof(string));
                dataTable.Columns.Add("IsInternal", typeof(string));
                dataTable.Columns.Add("UserType", typeof(string));
                dataTable.Columns.Add("Device", typeof(string));
                dataTable.Columns.Add("UserPlan", typeof(string));
                dataTable.Columns.Add("StripeSubscriptionID", typeof(string));
                dataTable.Columns.Add("AccessFindITExport", typeof(string));
                dataTable.Columns.Add("AccessBarCodeScan", typeof(string));
                dataTable.Columns.Add("AccessExport", typeof(string));
                dataTable.Columns.Add("UploadedSize", typeof(string));
                ds.Tables.Add(dataTable);
                System.IO.StringReader xmlSR = new System.IO.StringReader(innerxml);
                ds.ReadXml(xmlSR, XmlReadMode.IgnoreSchema);
                dataTable = ds.Tables[0];
                int empid = Convert.ToInt16(dataTable.Rows[0]["EmpID"]);
                int isinternal = Convert.ToInt16(dataTable.Rows[0]["IsInternal"]);
                int UserType = Convert.ToInt16(dataTable.Rows[0]["UserType"]);
                string UserPlans = Convert.ToString(dataTable.Rows[0]["UserPlan"]);
                string StripeSubscriptionID = Convert.ToString(dataTable.Rows[0]["StripeSubscriptionID"]);
                string AccessFindITExports = Convert.ToString(dataTable.Rows[0]["AccessFindITExport"]);
                string AccessBarCodeScans = Convert.ToString(dataTable.Rows[0]["AccessBarCodeScan"]);
                string AccessExports = Convert.ToString(dataTable.Rows[0]["AccessExport"]);
                string UploadedSize = Convert.ToString(dataTable.Rows[0]["UploadedSize"]);
                if (empid > 0)
                {
                    //Toast.MakeText (this, "Your successfully log in", ToastLength.Short).Show ();
                    //dialog.Hide();
                    ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                    ISharedPreferencesEditor editor = prefs.Edit();
                    //editor.Clear();
                    AccessExport = AccessExports;
                    //AccessBarCodeScan = AccessBarCodeScans;
                    AccessFindITExport = AccessFindITExports;
                    editor.PutLong("EmpID", empid);
                    editor.PutLong("IsInternal", isinternal);
                    editor.PutString("UserPlan", UserPlan);
                    editor.PutLong("UserType", UserType);
                    editor.PutString("StripeSubscriptionID", StripeSubscriptionID);
                    editor.PutString("AccessFindITExport", AccessFindITExports);
                    editor.PutString("AccessBarCodeScan", AccessBarCodeScan);
                    editor.PutString("AccessExport", AccessExports);
                    editor.PutString("UploadedSize", UploadedSize);
                    editor.Commit();
                    // applies changes synchronously on older APIs
                    editor.Apply();
                    UserPlan = UserPlans;
                    AccessExport = AccessExports;
                    AccessFindITExport = AccessFindITExports;
                    AccessBarCodeScan = AccessBarCodeScans;
                    //Toast.MakeText (this, isinternal.ToString(), ToastLength.Long).Show ();
                    //EditText txtusername = FindViewById<EditText> (Resource.Id.txtUserName);
                    //txtusername.Text="";

                    //EditText txtpassword = FindViewById<EditText> (Resource.Id.txtPassword);
                    //txtpassword.Text="";

                    //StartActivity(typeof(Main));


                }
                else
                {
                    //dialog.Hide();

                    Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();


                }
            }
            catch (Exception ex)
            {

                Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
            }
        }
        #endregion
        #endregion
    }
}

