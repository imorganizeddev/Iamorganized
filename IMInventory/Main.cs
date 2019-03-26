
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
using Android.Graphics;
using IMInventory.iminventory;
using System.Data;
using Android.Support.V4.Widget;
using Android.Net;
using Android.Net.Wifi;
using Xamarin.InAppBilling;
using Xamarin.InAppBilling.Utilities;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using Java.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Text.RegularExpressions;
using Android.Database.Sqlite;
using Android.Database;

namespace IMInventory
{
    [Activity(Label = "Main", MainLauncher = false, ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorPortrait)]
    public class Main : BaseClass
    {

        long ProjectID;
        long IsInternal;
        string projectname;
        string clientname;
        int inventorytype;
        long EmpID;
		SQLiteDatabase db = null;
        ISharedPreferences prefs;
        Dialog dialog;
        Dialog locationdialog;
        DataTable dataTable;
        DrawerLayout mDrawerLayout;
        LinearLayout mLeftDrawer;
        ProgressDialog pdialog;
        ProgressDialog projectcreateloadingdialog;
		long UserType;
        string AccessFindITExport = "";
        string AccessExport = "";
        string AccessBarCodeScan = "";
        string UserPlan;
		string planid = "";
		string plan = "";
		string subscriptionid = "";
		string userid = "";
		int EmployeeID;
		Decimal PaymentAmount = 0;
		int selecteditempositin = 0;
		ProgressDialog paymentprogressdialog;
		ProgressDialog planfetchdialog;
		Stripe.Token token = null;
		Stripe.Token tokenforsubscription = null;
		string Email="";
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.Main);
            pdialog = ProgressDialog.Show(this, "Loading", "Please wait.....");
			pdialog.Dismiss ();

            GetPreferenceManager();	// Preference Manage Section
            SetDrawerLayout(); 		//Drawer Layout Section 
            DeclareClickEvents();
            DeclareSwitchEvents();
			pdialog.Dismiss ();
			//  pdialog.Dismiss();
			if (isNetworkConnected ()) {
				GetProjectCount ();
			}
            if (IMApplication.player != null)
            {
                IMApplication.player.Stop();
                IMApplication.player = null;
            }
        }

        #region Private Methods

		private void GetProjectCount(){
			WebService objime = new WebService ();
			objime.ProjectListAsync ("", EmpID.ToString ());
			objime.ProjectListCompleted += getprojectxml; 

		}
		private void getprojectxml (object sender, ProjectListCompletedEventArgs e)
		{
			try{
				DataSet ds = new DataSet ();
				string innerxml = e.Result.InnerXml.ToString ();
				innerxml = "<ds>" + innerxml + "</ds>";
				DataTable dataTable = new DataTable ("Project");
				dataTable.Columns.Add ("ProjectID", typeof(string));
				dataTable.Columns.Add ("Projectname", typeof(string));
				dataTable.Columns.Add ("clientname", typeof(string));
				dataTable.Columns.Add ("addeddate", typeof(string));
				ds.Tables.Add (dataTable);
				System.IO.StringReader xmlSR = new System.IO.StringReader (innerxml);
				ds.ReadXml (xmlSR, XmlReadMode.IgnoreSchema);
				dataTable = ds.Tables [0];
			
				ImageView imgCreateProjectFromMain = FindViewById<ImageView>(Resource.Id.imgCreateProjectFromMain);
				if (dataTable.Rows.Count > 0) {
					if(UserType==1){

						if(dataTable.Rows.Count==1){
							foreach (DataRow dr in dataTable.Rows) {
								if(dr ["ProjectID"].ToString () != "0"){
									imgCreateProjectFromMain.Visibility=ViewStates.Gone;

								}
								else{
									imgCreateProjectFromMain.Visibility=ViewStates.Visible;}

							}

						}
						else{
							imgCreateProjectFromMain.Visibility=ViewStates.Gone;

						}

					}



				} 

			}
			catch (Exception ex){
				
			}
			//listView.ItemClick += OnListItemClick;
			//listView.SetFooterDividersEnabled(false);
		}

        private void showcreateprojectdialog()
        {
            dialog = new Dialog(this);
            dialog.RequestWindowFeature((int)WindowFeatures.NoTitle);
            dialog.SetContentView(Resource.Layout.CreateProject);
            ImageView btnCreateProject = (ImageView)dialog.FindViewById(Resource.Id.btnCreateProject);
            ImageView btnCancelCreateProject = (ImageView)dialog.FindViewById(Resource.Id.btnCancelCreateProject);

            Typeface tf = Typeface.CreateFromAsset(Assets, "Fonts/ROBOTO-LIGHT.TTF");
            EditText txtCreateProjectName = (EditText)dialog.FindViewById(Resource.Id.txtCreateProjectName);
            txtCreateProjectName.Typeface = tf;
            txtCreateProjectName.Invalidate();
            EditText txtCreateProjectClientName = (EditText)dialog.FindViewById(Resource.Id.txtCreateProjectClientName);
            txtCreateProjectClientName.Typeface = tf;
            txtCreateProjectClientName.Invalidate();

            btnCreateProject.Click += delegate
            {
                CreateProject();
            };
            btnCancelCreateProject.Click += delegate
            {
                dialog.Dismiss();
            };

            dialog.Show();
        }
        private void CreateProject()
        {
            projectcreateloadingdialog = ProgressDialog.Show(this, "Creating project", "Please wait.....");
            EditText txtCreateProjectName = (EditText)dialog.FindViewById(Resource.Id.txtCreateProjectName);
            EditText txtCreateProjectClientName = (EditText)dialog.FindViewById(Resource.Id.txtCreateProjectClientName);
            WebService objime = new WebService();
            projectname = txtCreateProjectName.Text;
            clientname = txtCreateProjectClientName.Text;

            if (projectname.Trim() == "")
            {
                projectcreateloadingdialog.Dismiss();
                Toast.MakeText(this, "Please give project name", ToastLength.Long).Show();
            }
            else if (clientname.Trim() == "")
            {
                projectcreateloadingdialog.Dismiss();
                Toast.MakeText(this, "Please give client name", ToastLength.Long).Show();
            }
            else
            {
                string dtnow = DateTime.Now.ToString("yyyy-MM-dd");
                objime.ExternalCreateProjectAsync(EmpID.ToString(), projectname, clientname, dtnow);
                objime.ExternalCreateProjectCompleted += getexternalcreateprojectxml;
            }
        }
        private void SetDrawerLayout()
        {
            mDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.myDrawer);
            mLeftDrawer = FindViewById<LinearLayout>(Resource.Id.leftListView);

            // Declate Text Views
            TextView txtNDName = FindViewById<TextView>(Resource.Id.txtNDName);
            TextView txtNDCompanyName = FindViewById<TextView>(Resource.Id.txtNDCompanyName);
            TextView txtNDEmail = FindViewById<TextView>(Resource.Id.txtNDEmail);
            TextView txtNDMobile = FindViewById<TextView>(Resource.Id.txtNDMobile);
            TextView txtNDAddress = FindViewById<TextView>(Resource.Id.txtNDAddress);
            txtNDEmail.SetOnKeyListener(null);
            // Declare Switches
            Switch swNDOptionsAlert = FindViewById<Switch>(Resource.Id.swNDOptionsAlert);
            Switch swNDOptionsVibration = FindViewById<Switch>(Resource.Id.swNDOptionsVibration);
            Switch swNDOptionsUploadOnWifi = FindViewById<Switch>(Resource.Id.swNDOptionsUploadOnWifi);
            Switch swNDOptionsScanViaBlutooth = FindViewById<Switch>(Resource.Id.swNDOptionsScanViaBlutooth);

            // Set font face

            Typeface tf = Typeface.CreateFromAsset(Assets, "Fonts/ROBOTO-LIGHT.TTF");
            txtNDName.Typeface = tf;
            txtNDName.Invalidate();
            txtNDCompanyName.Typeface = tf;
            txtNDCompanyName.Invalidate();
            txtNDEmail.Typeface = tf;
            txtNDEmail.Invalidate();
            txtNDMobile.Typeface = tf;
            txtNDMobile.Invalidate();
            txtNDAddress.Typeface = tf;
            txtNDAddress.Invalidate();
            // Set As Last Input Field
            txtNDAddress.ImeOptions = Android.Views.InputMethods.ImeAction.Done;

            // Get Content In Drawer Layout
            DataTable dtProfile = PopulateProfile();
            if (dtProfile != null)
            {
                if (dtProfile.Rows.Count > 0)
                {
                    txtNDName.Text = Convert.ToString(dtProfile.Rows[0]["Name"]);
                    txtNDCompanyName.Text = Convert.ToString(dtProfile.Rows[0]["Company"]);
                    txtNDEmail.Text = Convert.ToString(dtProfile.Rows[0]["Email"]);
                    txtNDMobile.Text = Convert.ToString(dtProfile.Rows[0]["Phone"]);
                    txtNDAddress.Text = Convert.ToString(dtProfile.Rows[0]["Address"]);
                    swNDOptionsAlert.Checked = Convert.ToString(dtProfile.Rows[0]["Alert"]) == "0" ? false : true;
                    swNDOptionsVibration.Checked = Convert.ToString(dtProfile.Rows[0]["Vibrate"]) == "0" ? false : true;
                    swNDOptionsUploadOnWifi.Checked = Convert.ToString(dtProfile.Rows[0]["Wifi"]) == "0" ? false : true;
                    swNDOptionsScanViaBlutooth.Checked = Convert.ToString(dtProfile.Rows[0]["Bluetooth"]) == "1" ? true : false;
                    SetPreferenceManager<int>("AlertVibrate", Convert.ToInt32(dtProfile.Rows[0]["Vibrate"]), this);
                    SetPreferenceManager<int>("AlertTone", Convert.ToInt32(dtProfile.Rows[0]["Alert"]), this);
                    SetPreferenceManager<int>("WifiUpload", Convert.ToInt32(dtProfile.Rows[0]["Wifi"]), this);
                    SetPreferenceManager<int>("Bluetooth", Convert.ToInt32(dtProfile.Rows[0]["Bluetooth"]), this);
                }
            }
        }
        private void GetPreferenceManager()
        {
            prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            ProjectID = prefs.GetLong("ProjectID", 0);
            IsInternal = prefs.GetLong("IsInternal", 0);
            EmpID = prefs.GetLong("EmpID", 5);
			UserPlan = prefs.GetString("UserPlan", "");
			UserType = prefs.GetLong("UserType", 1);
            AccessFindITExport = prefs.GetString("AccessFindITExport", "");
            AccessExport = prefs.GetString("AccessExport", "");
            AccessBarCodeScan = prefs.GetString("AccessBarCodeScan", "");
			Email=prefs.GetString("Email", "");
        }

        private void DeclareClickEvents()
        {
            StartSetup();
            ImageView imglist = FindViewById<ImageView>(Resource.Id.imglist);
            imglist.Click += delegate(object sender, EventArgs e)
            {
                mDrawerLayout.OpenDrawer(mLeftDrawer);
            };

            LinearLayout manualentry = FindViewById<LinearLayout>(Resource.Id.llmanu);
            LinearLayout barcodeentry = FindViewById<LinearLayout>(Resource.Id.llbar);
            LinearLayout voicedictation = FindViewById<LinearLayout>(Resource.Id.llvoice);
            LinearLayout llfindit = FindViewById<LinearLayout>(Resource.Id.llfindit);
            ImageView search = FindViewById<ImageView>(Resource.Id.imgsearchicon);
            ImageView imgNDCheck = FindViewById<ImageView>(Resource.Id.imgNDCheck);
            ImageView imgNDLogOut = FindViewById<ImageView>(Resource.Id.imgNDLogOut);
            ImageView imgNDHelp = FindViewById<ImageView>(Resource.Id.imgNDHelp);
			ImageView btnUpgrade = FindViewById<ImageView>(Resource.Id.btnUpgrade);
			TextView txtUpgrade = FindViewById<TextView>(Resource.Id.txtUpgrade);
			if (IsInternal == 1) {
				btnUpgrade.Visibility = ViewStates.Gone;
				txtUpgrade.Visibility = ViewStates.Gone;
			} else {

				if (UserPlan.ToLower ().Contains ("platinum")) {
					btnUpgrade.Visibility = ViewStates.Gone;
					txtUpgrade.Visibility = ViewStates.Visible;
					txtUpgrade.Text = "You are a platinum member";
				}
				else if (UserPlan.ToLower ().Contains ("gold")) {
					btnUpgrade.Visibility = ViewStates.Visible;
					txtUpgrade.Visibility = ViewStates.Visible;
					txtUpgrade.Text = "You are a gold member";
				} else {
					btnUpgrade.Visibility = ViewStates.Visible;
					txtUpgrade.Visibility = ViewStates.Visible;
					txtUpgrade.Text = "You are a free member";

				}
			}

			btnUpgrade.Click += delegate {
				planfetchdialog = ProgressDialog.Show(this, "Fectching Plan", "Please wait.....", true);

				new Thread(new ThreadStart(delegate
					{
						//LOAD METHOD TO GET ACCOUNT INFO
						RunOnUiThread(() => OpenPlanPopUp());
						//HIDE PROGRESS DIALOG
						RunOnUiThread(() => planfetchdialog.Dismiss());
					})).Start();
			};
            barcodeentry.Click += BarcodeEntry_Click;
            manualentry.Click += ManualEntry_Click;
            voicedictation.Click += VoiceDictation_Click;
            llfindit.Click += async (sender, e) =>
            {
                try
                {
                    //  Intent intent = new Intent("com.google.zxing.client.android.SCAN");
                    //intent.PutExtra("com.google.zxing.client.android.SCAN.SCAN_MODE", "QR_CODE_MODE");
                   //  var options = new ZXing.Mobile.MobileBarcodeScanningOptions();
                        ZXing.Mobile.MobileBarcodeScanner scanner = new ZXing.Mobile.MobileBarcodeScanner(this);
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
                            try
                            {
							if (result.Text.Trim ().Contains ("|||")) {
								string [] arrdata = result.Text.Split (new String [] { "|||" }, StringSplitOptions.None);
								string locationname = arrdata [0].ToString ();
								string inventorytypeid = arrdata [1].ToString ();
								ISharedPreferencesEditor editor = prefs.Edit ();
								editor.PutString ("LocationNameFromLocationList", locationname);
								editor.PutString ("InventoryTypeFromLocationList", inventorytypeid);
								editor.Commit ();
								editor.Apply ();
								StartActivity (typeof (FindItLocationListItem));
							} else {


								 
								string [] splitdata = result.Text.Split (new String [] { "|$|" }, StringSplitOptions.None);

								string itemid = splitdata [1].ToString ();
								ISharedPreferencesEditor editor = prefs.Edit ();
								editor.PutString ("LocationItemIDFromScan", itemid);
								editor.Commit ();
								editor.Apply ();
								StartActivity (typeof (FindItLocationListItem));

							}
                            }
                            catch (Exception ex)
                            {
                                Toast.MakeText(this, "No location item found", ToastLength.Long).Show();
                            }
                        }
                   
                }
                catch
                {


                }
            };
            search.Click += search_Click;
            imgNDCheck.Click += imgNDCheck_Click;
            imgNDLogOut.Click += imgNDLogOut_Click;
            imgNDHelp.Click += imgNDHelp_Click;
            ImageView imgCreateProjectFromMain = FindViewById<ImageView>(Resource.Id.imgCreateProjectFromMain);
            if (IsInternal == 1)
            {
                imgCreateProjectFromMain.Visibility = ViewStates.Gone;
            }
            else if (IsInternal == 0)
            {
                imgCreateProjectFromMain.Visibility = ViewStates.Visible;

            }
            imgCreateProjectFromMain.Click += delegate
            {
                showcreateprojectdialog();
            };

        }
        private void DeclareSwitchEvents()
        {
            Switch swNDOptionsVibration = FindViewById<Switch>(Resource.Id.swNDOptionsVibration);
            swNDOptionsVibration.CheckedChange += delegate(object sender, CompoundButton.CheckedChangeEventArgs e)
            {
                bool boolchecked = e.IsChecked;
                if (boolchecked)
                {
                    VibrateDevice();
                    SetPreferenceManager<int>("AlertVibrate", 1, this);
                }
                else
                {
                    SetPreferenceManager<int>("AlertVibrate", 0, this);
                }
            };

            Switch swNDOptionsAlert = FindViewById<Switch>(Resource.Id.swNDOptionsAlert);
            swNDOptionsAlert.CheckedChange += delegate(object sender, CompoundButton.CheckedChangeEventArgs e)
            {
                bool boolchecked = e.IsChecked;
                if (boolchecked)
                {
                    PlayNitificationTone();
                    SetPreferenceManager<int>("AlertTone", 1, this);
                }
                else
                {
                    SetPreferenceManager<int>("AlertTone", 0, this);
                }
            };
        }
        private void OpenInAppPurchasePopUp()
        {
            WifiManager wifiManager = (WifiManager)GetSystemService(Context.WifiService);
            WifiInfo wInfo = wifiManager.ConnectionInfo;
            String MACAdress = Android.Provider.Settings.Secure.GetString(ContentResolver, Android.Provider.Settings.Secure.AndroidId);

            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetMessage("To access this feature you will need to purchase this for $1.99 for lifetime for this device.Would you like to purchase?");
            builder.SetCancelable(false);
            builder.SetPositiveButton("Yes", (object sender, DialogClickEventArgs e) =>
            {
                _serviceConnection.BillingHandler.BuyProduct(_products[2]);
            });
            builder.SetNegativeButton("No", (object sender, DialogClickEventArgs e) =>
            {

            });
            AlertDialog alertdialog = builder.Create();
            alertdialog.Show();

        }
        #endregion

		#region
		public string[] arrplans = null;
		private void OpenPlanPopUp()
		{
			try
			{
				GeneralValues objGeneralValues = new GeneralValues();
				//planfetchdialog = ProgressDialog.Show(this, "Fectching Plan", "Please wait.....");
				StreamReader reader = null;
				var webrequest = (HttpWebRequest)WebRequest.Create("https://api.stripe.com/v1/plans?limit=2");
				webrequest.Method = "GET";
				webrequest.Headers.Add("Authorization", "Bearer " + objGeneralValues.SecretKey);
				HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
				reader = new StreamReader(webresponse.GetResponseStream());
				string obj = reader.ReadToEnd();
				var parsedjson = JObject.Parse(obj)["data"].Value<JArray>();
				string[] arrlist = new string[parsedjson.Count];

				int i = 0;
				if(UserPlan.ToLower().Contains("gold")){
					arrplans = new string[1];
					arrlist = new string[1];
				foreach (var item in parsedjson)
				{

					var parseditemjson = JObject.Parse(item.ToString().Replace("{{", "{").Replace("}}", "}"));
					string planname = parseditemjson["name"].ToString();
					string amount = parseditemjson["amount"].ToString();
					string currency = parseditemjson["currency"].ToString();
					string id = parseditemjson["id"].ToString();
						if(planname.ToLower().Contains("platinum")){
					arrlist[0] = planname;
					arrplans[0] = planname + "|||" + amount + "|||" + id;
					i = i + 1;
						}
				}
			   }
				else{
					arrplans = new string[parsedjson.Count];
					foreach (var item in parsedjson)
					{

						var parseditemjson = JObject.Parse(item.ToString().Replace("{{", "{").Replace("}}", "}"));
						string planname = parseditemjson["name"].ToString();
						string amount = parseditemjson["amount"].ToString();
						string currency = parseditemjson["currency"].ToString();
						string id = parseditemjson["id"].ToString();
						arrlist[i] = planname;
						arrplans[i] = planname + "|||" + amount + "|||" + id;
						i = i + 1;
					}


				}
				planfetchdialog.Dismiss();
				AlertDialog.Builder builder = new AlertDialog.Builder(this);
				builder.SetTitle("Choose plan");
				builder.SetSingleChoiceItems(arrlist, 0, ListClicked);
				builder.SetNeutralButton("Ok", (object sender, DialogClickEventArgs e) =>
					{
						string item = arrplans[selecteditempositin].ToString();
						var token = CreateToken(item);
					});

				builder.SetCancelable(false);
				builder.Show();
			}
			catch (Exception ex)
			{
			}
		}
		private void ListClicked(object sender, DialogClickEventArgs args)
		{
			try
			{
				selecteditempositin = args.Which;
			}
			catch (Exception ex)
			{
			}
		}

		private Stripe.Token CreateToken(string item)
		{
			try
			{
				GeneralValues objGeneralValues=new GeneralValues();
				WebService objWebService = new WebService();
				EditText txtemail = FindViewById<EditText>(Resource.Id.txtSignUpEmail);
				plan = item.Split(new String[] { "|||" }, StringSplitOptions.None)[0].ToString();
				WifiManager wifiManager = (WifiManager)GetSystemService(Context.WifiService);
				WifiInfo wInfo = wifiManager.ConnectionInfo;
				String MACAdress = Android.Provider.Settings.Secure.GetString(ContentResolver, Android.Provider.Settings.Secure.AndroidId);
				string amount = item.Split(new String[] { "|||" }, StringSplitOptions.None)[1].ToString();
				string id = item.Split(new String[] { "|||" }, StringSplitOptions.None)[2].ToString();
				planid = id;
				PaymentAmount = Convert.ToDecimal(amount);
				RadioButton rdbBusinessUse = FindViewById<RadioButton>(Resource.Id.rdbBusinessUse);
				Dialog paymentdialog = new Dialog(this);
				paymentdialog.RequestWindowFeature((int)WindowFeatures.NoTitle);
				paymentdialog.SetContentView(Resource.Layout.Payment);
				paymentdialog.SetCanceledOnTouchOutside(false);
				Typeface tf = Typeface.CreateFromAsset(Assets, "Fonts/ROBOTO-LIGHT.TTF");
				string encryptedplanname = URLEncoder.Encode(Encrypt(plan), "UTF-8");
				string encryptedamount = URLEncoder.Encode(Encrypt(amount), "UTF-8");
				string encryptedplanid = URLEncoder.Encode(Encrypt(planid), "UTF-8");
				string encrypteduserid = URLEncoder.Encode(Encrypt(EmpID.ToString()), "UTF-8");
				string encryptedmacadress = URLEncoder.Encode(Encrypt(MACAdress), "UTF-8");
				string encryptedemailaddress = URLEncoder.Encode(Encrypt(Email), "UTF-8");
				string url = objGeneralValues.PaymentURL+"?planname=" + encryptedplanname + "&amount=" + encryptedamount + "&planid=" + encryptedplanid + "&user=" + encrypteduserid + "&macaddress=" + encryptedmacadress + "&email=" + encryptedemailaddress;
				Intent browserintent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(objGeneralValues.PaymentURL+"?planname=" + encryptedplanname + "&amount=" + encryptedamount + "&planid=" + encryptedplanid + "&user=" + encrypteduserid + "&macaddress=" + encryptedmacadress + "&email=" + encryptedemailaddress));
				selecteditempositin = 0;
				if (plan.Trim().ToLower().Contains("platinum"))
				{
					objWebService.ExternalSignUpUserSubscriptionAsync(EmpID.ToString(), MACAdress, "1", "1", "1");
				}
				else
				{
					objWebService.ExternalSignUpUserSubscriptionAsync(EmpID.ToString(), MACAdress, "0", "0", "0");
				}
				StartActivity(browserintent);
			}
			catch (Exception ex)
			{

			}
			return token;
		}

		private string Encrypt(string clearText)
		{
			string EncryptionKey = "MAKV2SPBNI99212";
			byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
			using (Aes encryptor = Aes.Create())
			{
				Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
				encryptor.Key = pdb.GetBytes(32);
				encryptor.IV = pdb.GetBytes(16);
				using (MemoryStream ms = new MemoryStream())
				{
					using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
					{
						cs.Write(clearBytes, 0, clearBytes.Length);
						cs.Close();
					}
					clearText = Convert.ToBase64String(ms.ToArray());
				}
			}
			return clearText;
		}

		#endregion


        #region Service Async Callback

        private void getempxml(object sender, ProjectListCompletedEventArgs e)
        {
            try
            {
				
                DataSet ds = new DataSet();
                string innerxml = e.Result.InnerXml.ToString();
                innerxml = "<ds>" + innerxml + "</ds>";
                dataTable = new DataTable("Project");
                dataTable.Columns.Add("ProjectID", typeof(string));
                dataTable.Columns.Add("Projectname", typeof(string));
                dataTable.Columns.Add("clientname", typeof(string));
                dataTable.Columns.Add("addeddate", typeof(string));
                ds.Tables.Add(dataTable);
                System.IO.StringReader xmlSR = new System.IO.StringReader(innerxml);
                ds.ReadXml(xmlSR, XmlReadMode.IgnoreSchema);
                dataTable = ds.Tables[0];

                if (IsInternal == 1)
                {
                    if (ProjectID > 0)
                    {
						this.Finish ();
                        StartActivity(typeof(EntryTab));
                        ISharedPreferencesEditor editor = prefs.Edit();
                        editor.PutLong("InventoryType", inventorytype);
                        editor.Commit();
                        // applies changes synchronously on older APIs
                        editor.Apply();
                    }
                    else
                    {
                        ISharedPreferencesEditor editor = prefs.Edit();
                        editor.PutLong("InventoryType", inventorytype);
                        editor.Commit();
                        // applies changes synchronously on older APIs
                        editor.Apply();
                        StartActivity(typeof(Main_Search_A));
                    }
                }
                if (IsInternal == 0)
                {
                    if (ProjectID > 0)
                    {
						this.Finish ();
                        StartActivity(typeof(EntryTab));
                        ISharedPreferencesEditor editor = prefs.Edit();
                        editor.PutLong("InventoryType", inventorytype);
                        editor.Commit();
                        // applies changes synchronously on older APIs
                        editor.Apply();
                    }
                    else
                    {
                        if (dataTable.Rows.Count > 0)
                        {
                            ISharedPreferencesEditor editor = prefs.Edit();
                            editor.PutLong("InventoryType", inventorytype);
                            editor.Commit();
                            // applies changes synchronously on older APIs
                            editor.Apply();
                            StartActivity(typeof(Main_Search_A));
                        }
                        else
                        {
                            ISharedPreferencesEditor editor = prefs.Edit();
                            editor.PutLong("InventoryType", inventorytype);
                            editor.Commit();
                            // applies changes synchronously on older APIs
                            editor.Apply();
                            showcreateprojectdialog();
                        }
                    }
                }
            }
            catch
            {

                Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
            }
        }
        private void getexternalcreateprojectxml(object sender, ExternalCreateProjectCompletedEventArgs e)
        {
            try
            {

                DataSet ds = new DataSet();
                string innerxml = e.Result.InnerXml.ToString();
                innerxml = "<ds><table1>" + innerxml + "</table1></ds>";
                DataTable dataTable = new DataTable("table1");
                dataTable.Columns.Add("ProjectID", typeof(string));
                dataTable.Columns.Add("Message", typeof(string));
                ds.Tables.Add(dataTable);

                System.IO.StringReader xmlSR = new System.IO.StringReader(innerxml);
                ds.ReadXml(xmlSR, XmlReadMode.IgnoreSchema);
                dataTable = ds.Tables[0];
                if (dataTable.Rows.Count > 0)
                {
                    int projectid = Convert.ToInt16(dataTable.Rows[0]["ProjectID"]);
                    if (projectid > 0)
                    {

                        ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                        ISharedPreferencesEditor editor = prefs.Edit();
                        editor.PutLong("ProjectID", projectid);
                        editor.PutString("ProjectName", projectname);
                        editor.PutString("ClientName", clientname);
                        //editor.PutLong ("InventoryType", inventorytype);
                        editor.Commit();
                        // applies changes synchronously on older APIs
                        editor.Apply();
                        dialog.Dismiss();

                        locationdialog = new Dialog(this);
                        locationdialog.RequestWindowFeature((int)WindowFeatures.NoTitle);
                        locationdialog.SetContentView(Resource.Layout.CreateLocation);
                        ImageView btnCreateLocation = (ImageView)locationdialog.FindViewById(Resource.Id.btnCreateLocation);
                        ImageView btnCancelCreateLocation = (ImageView)locationdialog.FindViewById(Resource.Id.btnCancelCreateLocation);
                        Typeface tf = Typeface.CreateFromAsset(Assets, "Fonts/ROBOTO-LIGHT.TTF");
                        EditText txtPopUpLocationName = (EditText)locationdialog.FindViewById(Resource.Id.txtPopUpLocationName);
                        txtPopUpLocationName.Typeface = tf;
                        txtPopUpLocationName.Invalidate();
                        btnCreateLocation.Click += delegate
                        {
                            //EditText txtCreateProjectName = (EditText)dialog.FindViewById (Resource.Id.txtCreateProjectName);
                            editor.PutString("LocationName", txtPopUpLocationName.Text);
                            editor.Commit();
                            editor.Apply();

                            StartActivity(typeof(EntryTab));
                        };

                        btnCancelCreateLocation.Click += delegate
                        {
                            editor.PutString("LocationName", txtPopUpLocationName.Text);
                            editor.Commit();
                            editor.Apply();
                            StartActivity(typeof(EntryTab));
                        };
                        //dialog.Dismiss ();
                        locationdialog.Show();
                        projectcreateloadingdialog.Dismiss();
                    }
                    else
                    {
                        string Message = Convert.ToString(dataTable.Rows[0]["Message"]);
                        projectcreateloadingdialog.Dismiss();
                        Toast.MakeText(this, Message, ToastLength.Short).Show();

                    }


                }
            }
            catch
            {
                Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
            }
        }

        #endregion

        #region Service Callback

        private DataTable PopulateProfile()
        {
            DataSet ds = new DataSet();
            WebService objime = new WebService();

            if (isNetworkConnected() == false)
            {
				// If not connected to internet
				//  pdialog = ProgressDialog.Show(this, "No Network", "Could not connect to the network.");


				int empidcolumn = 0;
				int namecolumn = 0;
				int emailcolumn = 0;
				int companycolumn = 0;
				int phonecolumn = 0;
				int addresscolumn = 0;
				int alertcolumn = 0;
				int vibratecolumn = 0;
				int wificolumn = 0;
				int bluetoothcolumn = 0;


				// If not connected to internet
				pdialog = ProgressDialog.Show (this, "No Network", "Could not connect to the network.");
				pdialog.Dismiss ();
				db = this.OpenOrCreateDatabase ("ImInventory", FileCreationMode.Private, null);
				ICursor c1 = db.RawQuery ("SELECT * FROM tbl_UserDetails ", null);
				dataTable = new DataTable ("SettingDetails");
				dataTable.Columns.Add ("EmpID", typeof (string));
				dataTable.Columns.Add ("Name", typeof (string));
				dataTable.Columns.Add ("Email", typeof (string));
				dataTable.Columns.Add ("Company", typeof (string));
				dataTable.Columns.Add ("Phone", typeof (string));
				dataTable.Columns.Add ("Address", typeof (string));
				dataTable.Columns.Add ("Alert", typeof (string));
				dataTable.Columns.Add ("Vibrate", typeof (string));
				dataTable.Columns.Add ("Wifi", typeof (string));
				dataTable.Columns.Add ("Bluetooth", typeof (string));
				ds.Tables.Add (dataTable);
				if (c1.Count > 0) {

					empidcolumn = c1.GetColumnIndex ("EmpID");
					namecolumn = c1.GetColumnIndex ("Name");
					emailcolumn = c1.GetColumnIndex ("Email");
					companycolumn = c1.GetColumnIndex ("Company");
					phonecolumn = c1.GetColumnIndex ("Phone");
					addresscolumn = c1.GetColumnIndex ("Address");
					alertcolumn = c1.GetColumnIndex ("Alert");
					vibratecolumn = c1.GetColumnIndex ("Vibrate");
					wificolumn = c1.GetColumnIndex ("Wifi");
					bluetoothcolumn = c1.GetColumnIndex ("Bluetooth");
					c1.MoveToFirst ();
					String empid = c1.GetString (empidcolumn);
					String name = c1.GetString (namecolumn);
					String email = c1.GetString (emailcolumn);
					String company = c1.GetString (companycolumn);
					String phone = c1.GetString (phonecolumn);
					String address = c1.GetString (addresscolumn);
					String alert = c1.GetString (alertcolumn);
					String vibrate = c1.GetString (vibratecolumn);
					String wifi = c1.GetString (wificolumn);
					String bluetooth = c1.GetString (bluetoothcolumn);

					dataTable.Rows.Add (empid, name, email, company, phone, address, alert, vibrate, wifi, bluetooth);
				}


			}
            else
            {
                string innerxml = objime.PopulateProfile(EmpID.ToString()).InnerXml;
                innerxml = "<ds><SettingDetails>" + innerxml + "</SettingDetails></ds>";
                dataTable = new DataTable("SettingDetails");
                dataTable.Columns.Add("EmpID", typeof(string));
                dataTable.Columns.Add("Name", typeof(string));
                dataTable.Columns.Add("Email", typeof(string));
                dataTable.Columns.Add("Company", typeof(string));
                dataTable.Columns.Add("Phone", typeof(string));
                dataTable.Columns.Add("Address", typeof(string));
                dataTable.Columns.Add("Alert", typeof(string));
                dataTable.Columns.Add("Vibrate", typeof(string));
                dataTable.Columns.Add("Wifi", typeof(string));
                dataTable.Columns.Add("Bluetooth", typeof(string));
                ds.Tables.Add(dataTable);
                System.IO.StringReader xmlSR = new System.IO.StringReader(innerxml);
                ds.ReadXml(xmlSR, XmlReadMode.IgnoreSchema);
                dataTable = ds.Tables[0];

				db = this.OpenOrCreateDatabase ("ImInventory", FileCreationMode.Private, null);
				db.ExecSQL ("DROP TABLE IF EXISTS " + "tbl_UserDetails");
				db.ExecSQL ("CREATE TABLE IF NOT EXISTS "
			   + "tbl_UserDetails"
			   + " (ID INTEGER PRIMARY KEY AUTOINCREMENT, EmpID INTEGER,Name VARCHAR, Email VARCHAR, Company VARCHAR,Phone VARCHAR, Address VARCHAR , Alert VARCHAR, Vibrate VARCHAR, Wifi VARCHAR," +
			   "Bluetooth VARCHAR" + ");");
				ContentValues values = new ContentValues ();
				values.Put ("EmpID",Convert.ToString(dataTable.Rows[0]["EmpID"]));
				values.Put ("Name", Convert.ToString (dataTable.Rows [0] ["Name"]));
				values.Put ("Email", Convert.ToString (dataTable.Rows [0] ["Email"]));
				values.Put ("Company", Convert.ToString (dataTable.Rows [0] ["Company"]));
				values.Put ("Phone", Convert.ToString (dataTable.Rows [0] ["Phone"]));
				values.Put ("Address", Convert.ToString (dataTable.Rows [0] ["Address"]));
				values.Put ("Alert", Convert.ToString (dataTable.Rows [0] ["Alert"]));
				values.Put ("Vibrate", Convert.ToString (dataTable.Rows [0] ["Vibrate"]));
				values.Put ("Wifi", Convert.ToString (dataTable.Rows [0] ["Wifi"]));
				values.Put ("Bluetooth", Convert.ToString (dataTable.Rows [0] ["Bluetooth"]));
				db.Insert ("tbl_UserDetails", null, values);

            }

            return dataTable;
        }

        #endregion

        #region Click Events
        void imgNDLogOut_Click(object sender, EventArgs e)
        {
            try
            {
                //this.Finish();
                ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                ISharedPreferencesEditor editor = prefs.Edit();
                editor.Clear();
                editor.Commit();
                editor.Apply();
                FinishAffinity();
            }
            catch
            {
            }
            StartActivity(typeof(Login));
        }


        void imgNDHelp_Click(object sender, EventArgs e)
        {
            try
            {

                StartActivity(typeof(Help));
            }
            catch
            {
            }

        }
        void BarcodeEntry_Click(object sender, EventArgs e)
        {
			if (isNetworkConnected ()) {

				dataTable = new DataTable ();
				WebService objime = new WebService ();
				objime.ProjectListAsync ("", EmpID.ToString ());
				objime.ProjectListCompleted += getempxml;
				inventorytype = 3;
			} else {



				GetPreferenceManager ();
				if (ProjectID > 0) {
					this.Finish ();
					StartActivity (typeof (EntryTab));
					ISharedPreferencesEditor editor = prefs.Edit ();
					editor.PutLong ("InventoryType", inventorytype);
					editor.Commit ();
					// applies changes synchronously on older APIs
					editor.Apply ();
				} else { 
				Toast.MakeText (this, "Please check data connection. Select a project. Then try back !!!!", ToastLength.Short).Show ();
				}
			}
			
        }
        void ManualEntry_Click(object sender, EventArgs e)
        {
			if (isNetworkConnected ()) {
				dataTable = new DataTable ();
				WebService objime = new WebService ();
				objime.ProjectListAsync ("", EmpID.ToString ());
				objime.ProjectListCompleted += getempxml;
				inventorytype = 1;
			}else {
				if (ProjectID > 0) {
					GetPreferenceManager ();
					this.Finish ();
					StartActivity (typeof (EntryTab));
					ISharedPreferencesEditor editor = prefs.Edit ();
					editor.PutLong ("InventoryType", inventorytype);
					editor.Commit ();
					// applies changes synchronously on older APIs
					editor.Apply ();
				}

				else {
					Toast.MakeText (this, "Please check data connection. Select a project. Then try back !!!!", ToastLength.Short).Show ();
				}
			}
        }
        void VoiceDictation_Click(object sender, EventArgs e)
        {
			if (isNetworkConnected ()) {
				dataTable = new DataTable ();
				WebService objime = new WebService ();
				objime.ProjectListAsync ("", EmpID.ToString ());
				objime.ProjectListCompleted += getempxml;
				inventorytype = 2;
			}else {
				if (ProjectID > 0) {
					GetPreferenceManager ();
					this.Finish ();
					StartActivity (typeof (EntryTab));
					ISharedPreferencesEditor editor = prefs.Edit ();
					editor.PutLong ("InventoryType", inventorytype);
					editor.Commit ();
					// applies changes synchronously on older APIs
					editor.Apply ();
				}
				else {
					Toast.MakeText (this, "Please check data connection. Select a project. Then try back !!!!", ToastLength.Short).Show ();
				}
			}
        }


        void search_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(Main_Search_A));
        }
        void imgNDCheck_Click(object sender, EventArgs e)
        {
            try
            {
                // Declate Text Views
                TextView txtNDName = FindViewById<TextView>(Resource.Id.txtNDName);
                TextView txtNDCompanyName = FindViewById<TextView>(Resource.Id.txtNDCompanyName);
                TextView txtNDEmail = FindViewById<TextView>(Resource.Id.txtNDEmail);
                TextView txtNDMobile = FindViewById<TextView>(Resource.Id.txtNDMobile);
                TextView txtNDAddress = FindViewById<TextView>(Resource.Id.txtNDAddress);

                // Declare Switches
                Switch swNDOptionsAlert = FindViewById<Switch>(Resource.Id.swNDOptionsAlert);
                Switch swNDOptionsVibration = FindViewById<Switch>(Resource.Id.swNDOptionsVibration);
                Switch swNDOptionsUploadOnWifi = FindViewById<Switch>(Resource.Id.swNDOptionsUploadOnWifi);
                Switch swNDOptionsScanViaBlutooth = FindViewById<Switch>(Resource.Id.swNDOptionsScanViaBlutooth);

                string name = txtNDName.Text;
                string companyname = txtNDCompanyName.Text;
                string email = txtNDEmail.Text;
                string mobile = txtNDMobile.Text;
                string address = txtNDAddress.Text;
                string alert = swNDOptionsAlert.Checked == true ? "1" : "0";
                string vibrate = swNDOptionsVibration.Checked == true ? "1" : "0";
                string wifi = swNDOptionsUploadOnWifi.Checked == true ? "1" : "0";
                string bluetooth = swNDOptionsScanViaBlutooth.Checked == true ? "1" : "0";

                if (isNetworkConnected() == false)
                {
                    // If not connected to internet
                    Toast.MakeText(this, "You are not connected to the internet", ToastLength.Short).Show();
                }
                else
                {
                    if (isAllInputOk(name, companyname, email, mobile, address))
                    {
                        pdialog = ProgressDialog.Show(this, "Saving Settings", "Please wait.....");
                        WebService objime = new WebService();
                        objime.SaveProfileAsync(EmpID.ToString(), name, companyname, email, mobile, address, alert, vibrate, wifi, bluetooth);
                        objime.SaveProfileCompleted += delegate(object psender, SaveProfileCompletedEventArgs pe)
                        {

                            try
                            {
                                DataSet ds = new DataSet();
                                string innerxml = pe.Result.InnerXml;
                                if (innerxml.Contains("Success"))
                                {
                                    BaseClass bc = new BaseClass();
                                    bc.SetPreferenceManager<int>("AlertVibrate", Convert.ToInt32(vibrate), this);
                                    bc.SetPreferenceManager<int>("AlertTone", Convert.ToInt32(alert), this);
                                    bc.SetPreferenceManager<int>("Bluetooth", Convert.ToInt32(bluetooth), this);
                                    bc.SetPreferenceManager<int>("WifiUpload", Convert.ToInt32(wifi), this);
                                    Toast.MakeText(this, "Settings & Company details updated", ToastLength.Short).Show();
                                }
                                else
                                {
                                    Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                                }
                                pdialog.Dismiss();
                            }
                            catch (Exception ex)
                            {
                                pdialog.Dismiss();
                                Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                            }

                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
            }
        }

        #endregion

        #region Validation Methods

        private Boolean isAllInputOk(string name, string companyname, string email, string phone, string address)
        {
            bool _isallinputok = true;

            // If name is not supplied
            if (name.Trim() == string.Empty)
            {
                Toast.MakeText(this, "Please provide name", ToastLength.Long).Show();
                return false;
            }

            // If company name is not supplied
            //if(companyname.Trim() == string.Empty){				
            //Toast.MakeText (this, "Please provide company name", ToastLength.Long).Show ();
            //return false;
            //}	

            // If email is not supplied
            if (email.Trim() == string.Empty)
            {
                Toast.MakeText(this, "Please provide email", ToastLength.Long).Show();
                return false;
            }
            else
            {
                // If email supplied is not valid
                if (!isValidEmail(email.Trim()))
                {
                    Toast.MakeText(this, "Please provide proper email", ToastLength.Long).Show();
                    return false;
                }
            }



            return _isallinputok;
        }

        private bool isValidEmail(String email)
        {
            String EMAIL_PATTERN = "^[_A-Za-z0-9-\\+]+(\\.[_A-Za-z0-9-]+)*@"
                + "[A-Za-z0-9-]+(\\.[A-Za-z0-9]+)*(\\.[A-Za-z]{2,})$";

            Java.Util.Regex.Pattern pattern = Java.Util.Regex.Pattern.Compile(EMAIL_PATTERN);
            Java.Util.Regex.Matcher matcher = pattern.Matcher(email);
            return matcher.Matches();
        }
        private Boolean isNetworkConnected()
        {
            bool isNetworkConnected = true;

            var connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            var activeConnection = connectivityManager.ActiveNetworkInfo;

            if ((activeConnection != null) && activeConnection.IsConnected)
            {
                isNetworkConnected = true;
            }
            else
            {
                isNetworkConnected = false;
            }

            return isNetworkConnected;
        }
        #endregion
        #region InAppPurchase
        IList<Product> _products;
        InAppBillingServiceConnection _serviceConnection;
        string producttopurchase = "findit";

        private void CreateInAppPurchasePopUp()
        {

            StartSetup();

        }
        public void StartSetup()
        {

            string value = Security.Unify(
                new string[] { "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAkKjTesOfop/n54p0bkVv07HKoyzRzBBd8SFfGxI1wn" ,
					"GOR+Vifr8txSTln6mSsSoWHoiCddES2hH3nnr5mLoT2jb9ZO9sweUa6E3six1TACVuMxm4F" ,
					"jsmeCTVoRmSxHH57hjYrhtlJXIqHwGzqaT7KjBCRStkSmUahj0FMOj8uXE5fFgqKOQdTvnFd0lMsjGnekcDYlafa9DKRB+mW1lPAeLN" ,
					"FlWb9tXjo26OdIER8ac6K7kCK6annSyFJ2dp2EVUTwKlcLAhMV4bxacSCdF2/rttTfNvLxly776jZ40a0+ptd3VAyet43tW71I0tHq6P7" ,
					"PLj4dA/SYUP0PwMSQdrkQIDAQAB" },
                new int[] { 0, 1, 2, 3, 4 });
            _serviceConnection = new InAppBillingServiceConnection(this, value);
            _serviceConnection.OnConnected += () =>
            {
                GetInventory();
                LoadPurchasedItems();
            };
            // Attempt to connect to the service
            _serviceConnection.Connect();
        }
        private async Task GetInventory()
        {
			
            _products = await _serviceConnection.BillingHandler.QueryInventoryAsync(new List<string> {
				"bar_code"
			}, ItemType.Product);
			InAppPurchase._products = _products;
			InAppPurchase._serviceConnection = _serviceConnection;
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
        {
            // Ask the open connection's billing handler to get any purchases
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
						objWebService.ExternalSignUpUserSubscriptionAsync(EmpID.ToString(), MACAdress, AccessFindITExport, "1", AccessExport);
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
            objWebService.GetEmpDetailsByEmpIDAsync(EmpID.ToString(), MACAdress);
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
					AccessBarCodeScan=AccessBarCodeScans;
                    //AccessBarCodeScan = AccessBarCodeScans;
                    AccessFindITExport = AccessFindITExports;
                    editor.PutLong("EmpID", empid);
                    editor.PutLong("IsInternal", isinternal);
                    editor.PutString("UserPlan", UserPlan);
                    editor.PutLong("UserType", UserType);
                    editor.PutString("StripeSubscriptionID", StripeSubscriptionID);
                    editor.PutString("AccessFindITExport", AccessFindITExports);
					editor.PutString("AccessBarCodeScan", AccessBarCodeScans);
                    editor.PutString("AccessExport", AccessExports);
                    editor.PutString("UploadedSize", UploadedSize);
                    editor.Commit();
                    // applies changes synchronously on older APIs
                    editor.Apply();
                    UserPlan = UserPlans;
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

		public override void OnBackPressed ()
		{
			this.Finish ();
			StartActivity (typeof (Login));
			ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences (this);
			ISharedPreferencesEditor editor = prefs.Edit ();
			editor.PutLong ("Backfrommain", 1);
			editor.Commit ();
			// applies changes synchronously on older APIs
			editor.Apply ();
		}

	
        #endregion
       
    }
}

