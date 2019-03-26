
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
using Android.Hardware;
using Android.Views.Animations;
using Android.Support.V4.Widget;
using System.Data;
using IMInventory.iminventory;
using Android.Net;
using Android.Media;
using System.Threading;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using Android.Net.Wifi;
using Java.Net;
using System.Security.Cryptography;
using Android.Database.Sqlite;
using Android.Database;

namespace IMInventory
{
    [Activity(Label = "EntryTab", MainLauncher = false, ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorPortrait)]
    public class EntryTab : TabActivity
    {
        long InventoryType;
        long ProjectID;
        long EmpID;
		long UserType;
		string UserPlan;
        List<string> mLeftItems = new List<string>();
		SQLiteDatabase db = null;
        Android.Widget.TabHost.TabSpec spec;
        Android.Hardware.Camera.Parameters parameters;
        ISharedPreferences prefs;
        Camera camera;
        DrawerLayout mDrawerLayout;
        LinearLayout mLeftDrawer;
        ProgressDialog pdialog;
        DataTable dataTable;
		ProgressDialog planfetchdialog;
		long IsInternal;
		int selecteditempositin = 0;
		string plan = "";
		string planid = "";
		Decimal PaymentAmount = 0;
		string Email = "";
		Stripe.Token token = null;
		Stripe.Token tokenforsubscription = null;
		const string FLASH_MODE_TORCH = Android.Hardware.Camera.Parameters.FlashModeTorch;
		const string FLASH_MODE_ON = Android.Hardware.Camera.Parameters.FlashModeOn;
		const string FLASH_MODE_OFF = Android.Hardware.Camera.Parameters.FlashModeOff;
        protected override void OnCreate(Bundle bundle)
        {
			
            base.OnCreate(bundle);
            this.RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.EntryTab);
            pdialog = ProgressDialog.Show(this, "Loading", "Please wait ");
			pdialog.Dismiss ();
            GetPreferenceManager();// Preference Manage Section
            SetDrawerLayout(); //Drawer Layout Section 
            SetTabHost();// Set Tab Host
            DeclareClickEvents();
            DeclareSwitchEvents();
           
        }

        private void GetPreferenceManager()
        {
			
            prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            ProjectID = prefs.GetLong("ProjectID", 0);

				
				if (ProjectID == 0) {
				if (isNetworkConnected ()) {
					prefs = IMApplication.pref;
					ProjectID = IMApplication.projectid;

				} else {
					Toast.MakeText (this, "Please check data connection. Select a project. Then try back !!!!", ToastLength.Short).Show ();



				}
			}
            EmpID = prefs.GetLong("EmpID", 5);
			UserType = prefs.GetLong("UserType", 1);
			UserPlan = prefs.GetString("UserPlan", "");
			IsInternal = prefs.GetLong ("IsInternal", 0);
        }

        private void SetTabHost()
        {
            CreateTab(typeof(ManualEntry), "Manual", "Manual");

				CreateTab (typeof(VoiceDictation), "Dictation", "Dictation");


            CreateTab(typeof(BarcodeEntry), "Barcode", "Barcode");
        }
        private void CreateTab(Type activityType, string tag, string label)
        {
            Intent intent = new Intent(this, activityType);
            intent.AddFlags(ActivityFlags.ClearTop);

            spec = TabHost.NewTabSpec(tag);
            spec.SetIndicator(label);
			spec.SetContent (intent);
				
            TabHost.AddTab(spec);
            TabHost.TabWidget.SetBackgroundColor(Android.Graphics.Color.ParseColor("#3597d4"));
            TabHost.TabChanged += TabHost_TabChanged;

            InventoryType = prefs.GetLong("InventoryType", 0);
            TextView tabTextHeader = FindViewById<TextView>(Resource.Id.tabTextHeader);
            ImageView imgsearchicon = FindViewById<ImageView>(Resource.Id.imgsearchicon);
            ImageView imgflashicon = FindViewById<ImageView>(Resource.Id.imgflashicon);

            if (InventoryType > 0)
            {
                if (InventoryType == 1)
                {
                    TabHost.CurrentTab = 0;
                    tabTextHeader.SetText("Manual Entry Inventory", TextView.BufferType.Normal);
                    imgflashicon.Visibility = ViewStates.Visible;
                    if (camera != null)
                    {
                        parameters = camera.GetParameters();
                        parameters.FlashMode = Camera.Parameters.FlashModeOff;
                        camera.SetParameters(parameters);
                        camera.StopPreview();
                        camera.Release();
                    }
                }
                if (InventoryType == 2)
                {
					
						TabHost.CurrentTab = 1;
						tabTextHeader.SetText ("Voice Dictation Inventory", TextView.BufferType.Normal);
						imgflashicon.Visibility = ViewStates.Visible;
						if (camera != null) {
							parameters = camera.GetParameters ();
							parameters.FlashMode = Camera.Parameters.FlashModeOff;
							camera.SetParameters (parameters);
							camera.StopPreview ();
							camera.Release ();
						}

                }
                if (InventoryType == 3)
                {
                    TabHost.CurrentTab = 2;
                    tabTextHeader.SetText("Barcode Entry Inventory", TextView.BufferType.Normal);
                    imgflashicon.Visibility = ViewStates.Visible;
                    if (camera != null)
                    {
                        camera.StopPreview();
                        camera.Release();
                    }
                }
            }
            else
            {
				TabHost.CurrentTab = 0;
				tabTextHeader.SetText("Manual Entry Inventory", TextView.BufferType.Normal);
				imgflashicon.Visibility = ViewStates.Visible;
				if (camera != null)
				{
					parameters = camera.GetParameters();
					parameters.FlashMode = Camera.Parameters.FlashModeOff;
					camera.SetParameters(parameters);
					camera.StopPreview();
					camera.Release();
				}
            }
        }
        private void SetDrawerLayout()
        {
            mDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.myDrawer);
            mLeftDrawer = FindViewById<LinearLayout>(Resource.Id.leftListView);

            // Declate Text Views
            TextView txtTabNDName = FindViewById<TextView>(Resource.Id.txtTabNDName);
            TextView txtTabNDCompanyName = FindViewById<TextView>(Resource.Id.txtTabNDCompanyName);
            TextView txtTabNDEmail = FindViewById<TextView>(Resource.Id.txtTabNDEmail);
            TextView txtTabNDMobile = FindViewById<TextView>(Resource.Id.txtTabNDMobile);
            TextView txtTabNDAddress = FindViewById<TextView>(Resource.Id.txtTabNDAddress);
			txtTabNDEmail.SetOnKeyListener (null);

            // Declare Switches
            Switch swTabNDOptionsAlert = FindViewById<Switch>(Resource.Id.swTabNDOptionsAlert);
            Switch swTabNDOptionsVibration = FindViewById<Switch>(Resource.Id.swTabNDOptionsVibration);
            Switch swTabNDOptionsUploadOnWifi = FindViewById<Switch>(Resource.Id.swTabNDOptionsUploadOnWifi);
			Switch swTabNDOptionsScanViaBlutooth = FindViewById<Switch>(Resource.Id.swTabNDOptionsScanViaBlutooth);

            // Set font face
            Android.Graphics.Typeface tf = Android.Graphics.Typeface.CreateFromAsset(Assets, "Fonts/ROBOTO-LIGHT.TTF");
            txtTabNDName.Typeface = tf;
            txtTabNDName.Invalidate();
            txtTabNDCompanyName.Typeface = tf;
            txtTabNDCompanyName.Invalidate();
            txtTabNDEmail.Typeface = tf;
            txtTabNDEmail.Invalidate();
            txtTabNDMobile.Typeface = tf;
            txtTabNDMobile.Invalidate();
            txtTabNDAddress.Typeface = tf;
            txtTabNDAddress.Invalidate();

            // Set As Last Input Field
            txtTabNDAddress.ImeOptions = Android.Views.InputMethods.ImeAction.Done;

            // Get Content In Drawer Layout
            DataTable dtProfile = PopulateProfile();
            if (dtProfile.Rows.Count > 0)
            {
				pdialog.Dismiss ();
                txtTabNDName.Text = Convert.ToString(dtProfile.Rows[0]["Name"]);
                txtTabNDCompanyName.Text = Convert.ToString(dtProfile.Rows[0]["Company"]);
                txtTabNDEmail.Text = Convert.ToString(dtProfile.Rows[0]["Email"]);
                txtTabNDMobile.Text = Convert.ToString(dtProfile.Rows[0]["Phone"]);
                txtTabNDAddress.Text = Convert.ToString(dtProfile.Rows[0]["Address"]);
                swTabNDOptionsAlert.Checked = Convert.ToString(dtProfile.Rows[0]["Alert"]) == "0" ? false : true;
                swTabNDOptionsVibration.Checked = Convert.ToString(dtProfile.Rows[0]["Vibrate"]) == "0" ? false : true;
                swTabNDOptionsUploadOnWifi.Checked = Convert.ToString(dtProfile.Rows[0]["Wifi"]) == "0" ? false : true;
				swTabNDOptionsScanViaBlutooth.Checked = Convert.ToString(dtProfile.Rows[0]["Bluetooth"]) == "0" ? false : true;

                // Store Global Value
                prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                ISharedPreferencesEditor editor = prefs.Edit();
                editor.PutInt("AlertVibrate", Convert.ToInt32(dtProfile.Rows[0]["Vibrate"]));
                editor.PutInt("AlertTone", Convert.ToInt32(dtProfile.Rows[0]["Alert"]));
				editor.PutInt("WifiUpload", Convert.ToInt32(dtProfile.Rows[0]["Wifi"]));
				editor.PutInt("Bluetooth", Convert.ToInt32(dtProfile.Rows[0]["Bluetooth"]));
                editor.Commit();
                editor.Apply();
            }
        }

        private void DeclareClickEvents()
        {
            ImageView imglistinternal = FindViewById<ImageView>(Resource.Id.imglistinternal);
            imglistinternal.Click += delegate(object sender, EventArgs e)
            {
                try
                {
                    mDrawerLayout.OpenDrawer(mLeftDrawer);
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                }

            };

            ImageView imgsearchicon = FindViewById<ImageView>(Resource.Id.imgsearchicon);
            ImageView imgflashicon = FindViewById<ImageView>(Resource.Id.imgflashicon);
			ImageView imgAppLogo = FindViewById<ImageView>(Resource.Id.imgAppLogo);
			imgAppLogo.Click += delegate {
				this.Finish ();
				StartActivity(typeof(Main));	
			};
            imgsearchicon.Click += delegate
            {
                try
                {
                    StartActivity(typeof(Main_Search_A));
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                }

            };
            imgflashicon.Click += delegate
            {
				
                try
                {

					string manuName = Android.OS.Build.Manufacturer.ToLower();  // android.os.Build.MANUFACTURER.toLowerCase();
	                    if (IMApplication.camera != null)
	                    {
	                        IMApplication.camera.Release();
	                        IMApplication.camera = null;


	                    }


						if (camera == null)
						{

						if(manuName.Contains("samsung")){

							camera = Camera.Open();     
							Android.Hardware.Camera.Parameters param = camera.GetParameters();
							param.FlashMode=Android.Hardware.Camera.Parameters.FlashModeTorch;
							camera.SetParameters(param);
							camera.StartPreview();
							IMApplication.camera = camera;

						}
						else{
							camera = Camera.Open();
							parameters = camera.GetParameters();
							parameters.FlashMode = On ? FLASH_MODE_OFF : SupportedFlashModeOn;
							camera.SetParameters(parameters);
							IMApplication.camera = camera;
						}
						}
						else
						{
							camera.Release();
							camera = null;
						}





				
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                }
            };

            ImageView imgTabNDCheck = FindViewById<ImageView>(Resource.Id.imgTabNDCheck);
            imgTabNDCheck.Click += imgTabNDCheck_Click;

            ImageView imgNDTabLogOut = FindViewById<ImageView>(Resource.Id.imgNDTabLogOut);
            imgNDTabLogOut.Click += imgNDTabLogOut_Click;
			ImageView imgNDHelp = FindViewById<ImageView>(Resource.Id.imgTabNDHelp);
			imgNDHelp.Click += imgNDHelp_Click;
			ImageView btnUpgrade = FindViewById<ImageView> (Resource.Id.btnUpgrade);
			TextView txtUpgrade = FindViewById<TextView> (Resource.Id.txtUpgrade);
			if (IsInternal == 1) {
				btnUpgrade.Visibility = ViewStates.Gone;
				txtUpgrade.Visibility = ViewStates.Gone;
			} else {

				if (UserPlan.ToLower ().Contains ("platinum")) {
					btnUpgrade.Visibility = ViewStates.Gone;
					txtUpgrade.Visibility = ViewStates.Visible;
					txtUpgrade.Text = "You are a platinum member";
				} else if (UserPlan.ToLower ().Contains ("gold")) {
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
				planfetchdialog = ProgressDialog.Show (this, "Fectching Plan", "Please wait.....", true);

				new Thread (new ThreadStart (delegate {
					//LOAD METHOD TO GET ACCOUNT INFO
					RunOnUiThread (() => OpenPlanPopUp ());
					//HIDE PROGRESS DIALOG
					RunOnUiThread (() => planfetchdialog.Dismiss ());
				})).Start ();
			};

		} 

		private string _supportdFlashModeOn;
		private string SupportedFlashModeOn {
			get
			{
				if (_supportdFlashModeOn == null)
				{
					var supportedModes = parameters.SupportedFlashModes;

					if(supportedModes.Contains(FLASH_MODE_TORCH))
						_supportdFlashModeOn = FLASH_MODE_TORCH;
					else if(supportedModes.Contains(FLASH_MODE_ON))
						_supportdFlashModeOn = FLASH_MODE_ON;
				}
				return _supportdFlashModeOn;
			}
		}
		public bool On {
			get
			{
				return parameters.FlashMode == SupportedFlashModeOn;
			}
		}
		void HandleTorchError(Exception ex)
		{
			Android.Util.Log.Info("Torch", ex.Message);

		}
        private void DeclareSwitchEvents()
        {


            Switch swTabNDOptionsVibration = FindViewById<Switch>(Resource.Id.swTabNDOptionsVibration);
            swTabNDOptionsVibration.CheckedChange += delegate(object sender, CompoundButton.CheckedChangeEventArgs e)
            {
                bool boolchecked = e.IsChecked;
                // Store Global Value
                prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                ISharedPreferencesEditor editor = prefs.Edit();

                if (boolchecked)
                {
                    Vibrator v = (Vibrator)this.GetSystemService(Context.VibratorService); // Make phone vibrate
                    v.Vibrate(1000);

                    editor.PutInt("AlertVibrate", 1);
                }
                else
                {
                    editor.PutInt("AlertVibrate", 0);
                }

                editor.Commit();
                editor.Apply();
            };

            Switch swTabNDOptionsAlert = FindViewById<Switch>(Resource.Id.swTabNDOptionsAlert);
            swTabNDOptionsAlert.CheckedChange += delegate(object sender, CompoundButton.CheckedChangeEventArgs e)
            {
                bool boolchecked = e.IsChecked;
                // Store Global Value
                prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                ISharedPreferencesEditor editor = prefs.Edit();

                if (boolchecked)
                {
                    Android.Net.Uri notification = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
                    Ringtone r = RingtoneManager.GetRingtone(this, notification);
                    r.Play();

                    editor.PutInt("AlertTone", 1);
                }
                else
                {
                    editor.PutInt("AlertTone", 0);
                }

                editor.Commit();
                editor.Apply();
            };


        }
        void TabHost_TabChanged(object sender, TabHost.TabChangeEventArgs e)
        {

            TextView tabTextHeader = FindViewById<TextView>(Resource.Id.tabTextHeader);
            ImageView imgflashicon = FindViewById<ImageView>(Resource.Id.imgflashicon);
            TabHost tb = (TabHost)sender;
            if (e.TabId == "Manual")
            {
                tabTextHeader.SetText("Manual Entry Inventory", TextView.BufferType.Normal);
                imgflashicon.Visibility = ViewStates.Visible;
            }

            if (e.TabId == "Dictation")
            {
                tabTextHeader.SetText("Voice Dictation Inventory", TextView.BufferType.Normal);
                imgflashicon.Visibility = ViewStates.Visible;
            }
            if (e.TabId == "Barcode")
            {
                tabTextHeader.SetText("Barcode Entry Inventory", TextView.BufferType.Normal);
                imgflashicon.Visibility = ViewStates.Visible;

            }
			//return false;
        }

		#region Stripe
		public string [] arrplans = null;
		private void OpenPlanPopUp ()
		{
			try {
				GeneralValues objGeneralValues = new GeneralValues ();
				//planfetchdialog = ProgressDialog.Show(this, "Fectching Plan", "Please wait.....");
				StreamReader reader = null;
				var webrequest = (HttpWebRequest)WebRequest.Create ("https://api.stripe.com/v1/plans?limit=2");
				webrequest.Method = "GET";
				webrequest.Headers.Add ("Authorization", "Bearer " + objGeneralValues.SecretKey);
				HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse ();
				reader = new StreamReader (webresponse.GetResponseStream ());
				string obj = reader.ReadToEnd ();
				var parsedjson = JObject.Parse (obj) ["data"].Value<JArray> ();
				string [] arrlist = new string [parsedjson.Count];

				int i = 0;
				if (UserPlan.ToLower ().Contains ("gold")) {
					arrplans = new string [1];
					arrlist = new string [1];
					foreach (var item in parsedjson) {

						var parseditemjson = JObject.Parse (item.ToString ().Replace ("{{", "{").Replace ("}}", "}"));
						string planname = parseditemjson ["name"].ToString ();
						string amount = parseditemjson ["amount"].ToString ();
						string currency = parseditemjson ["currency"].ToString ();
						string id = parseditemjson ["id"].ToString ();
						if (planname.ToLower ().Contains ("platinum")) {
							arrlist [0] = planname;
							arrplans [0] = planname + "|||" + amount + "|||" + id;
							i = i + 1;
						}
					}
				} else {
					arrplans = new string [parsedjson.Count];
					foreach (var item in parsedjson) {

						var parseditemjson = JObject.Parse (item.ToString ().Replace ("{{", "{").Replace ("}}", "}"));
						string planname = parseditemjson ["name"].ToString ();
						string amount = parseditemjson ["amount"].ToString ();
						string currency = parseditemjson ["currency"].ToString ();
						string id = parseditemjson ["id"].ToString ();
						arrlist [i] = planname;
						arrplans [i] = planname + "|||" + amount + "|||" + id;
						i = i + 1;
					}


				}
				planfetchdialog.Dismiss ();
				AlertDialog.Builder builder = new AlertDialog.Builder (this);
				builder.SetTitle ("Choose plan");
				builder.SetSingleChoiceItems (arrlist, 0, ListClicked);
				builder.SetNeutralButton ("Ok", (object sender, DialogClickEventArgs e) => {
					string item = arrplans [selecteditempositin].ToString ();
					var token = CreateToken (item);
				});

				builder.SetCancelable (false);
				builder.Show ();
			} catch (Exception ex) {
			}
		}
		private void ListClicked (object sender, DialogClickEventArgs args)
		{
			try {
				selecteditempositin = args.Which;
			} catch (Exception ex) {
			}
		}

		private Stripe.Token CreateToken (string item)
		{
			try {
				GeneralValues objGeneralValues = new GeneralValues ();
				WebService objWebService = new WebService ();
				EditText txtemail = FindViewById<EditText> (Resource.Id.txtSignUpEmail);
				plan = item.Split (new String [] { "|||" }, StringSplitOptions.None) [0].ToString ();
				WifiManager wifiManager = (WifiManager)GetSystemService (Context.WifiService);
				WifiInfo wInfo = wifiManager.ConnectionInfo;
				String MACAdress = Android.Provider.Settings.Secure.GetString (ContentResolver, Android.Provider.Settings.Secure.AndroidId);
				string amount = item.Split (new String [] { "|||" }, StringSplitOptions.None) [1].ToString ();
				string id = item.Split (new String [] { "|||" }, StringSplitOptions.None) [2].ToString ();
				planid = id;
				PaymentAmount = Convert.ToDecimal (amount);
				RadioButton rdbBusinessUse = FindViewById<RadioButton> (Resource.Id.rdbBusinessUse);
				Dialog paymentdialog = new Dialog (this);
				paymentdialog.RequestWindowFeature ((int)WindowFeatures.NoTitle);
				paymentdialog.SetContentView (Resource.Layout.Payment);
				paymentdialog.SetCanceledOnTouchOutside (false);
				//Typeface tf = Typeface.CreateFromAsset (Assets, "Fonts/ROBOTO-LIGHT.TTF");
				string encryptedplanname = URLEncoder.Encode (Encrypt (plan), "UTF-8");
				string encryptedamount = URLEncoder.Encode (Encrypt (amount), "UTF-8");
				string encryptedplanid = URLEncoder.Encode (Encrypt (planid), "UTF-8");
				string encrypteduserid = URLEncoder.Encode (Encrypt (EmpID.ToString ()), "UTF-8");
				string encryptedmacadress = URLEncoder.Encode (Encrypt (MACAdress), "UTF-8");
				string encryptedemailaddress = URLEncoder.Encode (Encrypt (Email), "UTF-8");
				string url = objGeneralValues.PaymentURL + "?planname=" + encryptedplanname + "&amount=" + encryptedamount + "&planid=" + encryptedplanid + "&user=" + encrypteduserid + "&macaddress=" + encryptedmacadress + "&email=" + encryptedemailaddress;
				Intent browserintent = new Intent (Intent.ActionView, Android.Net.Uri.Parse (objGeneralValues.PaymentURL + "?planname=" + encryptedplanname + "&amount=" + encryptedamount + "&planid=" + encryptedplanid + "&user=" + encrypteduserid + "&macaddress=" + encryptedmacadress + "&email=" + encryptedemailaddress));
				selecteditempositin = 0;
				if (plan.Trim ().ToLower ().Contains ("platinum")) {
					objWebService.ExternalSignUpUserSubscriptionAsync (EmpID.ToString (), MACAdress, "1", "1", "1");
				} else {
					objWebService.ExternalSignUpUserSubscriptionAsync (EmpID.ToString (), MACAdress, "0", "0", "0");
				}
				StartActivity (browserintent);
			} catch (Exception ex) {

			}
			return token;
		}

		private string Encrypt (string clearText)
		{
			string EncryptionKey = "MAKV2SPBNI99212";
			byte [] clearBytes = System.Text.Encoding.Unicode.GetBytes (clearText);
			using (Aes encryptor = Aes.Create ()) {
				Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes (EncryptionKey, new byte [] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
				encryptor.Key = pdb.GetBytes (32);
				encryptor.IV = pdb.GetBytes (16);
				using (MemoryStream ms = new MemoryStream ()) {
					using (CryptoStream cs = new CryptoStream (ms, encryptor.CreateEncryptor (), CryptoStreamMode.Write)) {
						cs.Write (clearBytes, 0, clearBytes.Length);
						cs.Close ();
					}
					clearText = Convert.ToBase64String (ms.ToArray ());
				}
			}
			return clearText;
		}

		#endregion


		#region Service Callback

		private DataTable PopulateProfile()
        {
            DataSet ds = new DataSet();
            WebService objime = new WebService();



            if (isNetworkConnected() == false)
            {



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
                pdialog = ProgressDialog.Show(this, "No Network", "Could not connect to the network.");
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

					dataTable.Rows.Add (empid,name,email,company,phone,address,alert,vibrate,wifi,bluetooth);
				}


            }
            else
            {
				try{
				//pdialog = ProgressDialog.Show(this, "Loading", "Please wait");
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


				
				}
				catch{
					pdialog.Dismiss();
					StartActivity (typeof(Main));
				}
            }
			pdialog.Dismiss ();
            return dataTable;
        }

        #endregion

        #region Click Events

        void imgTabNDCheck_Click(object sender, EventArgs e)
        {
            try
            {
                // Declate Text Views
                TextView txtTabNDName = FindViewById<TextView>(Resource.Id.txtTabNDName);
                TextView txtTabNDCompanyName = FindViewById<TextView>(Resource.Id.txtTabNDCompanyName);
                TextView txtTabNDEmail = FindViewById<TextView>(Resource.Id.txtTabNDEmail);
                TextView txtTabNDMobile = FindViewById<TextView>(Resource.Id.txtTabNDMobile);
                TextView txtTabNDAddress = FindViewById<TextView>(Resource.Id.txtTabNDAddress);

                // Declare Switches
                Switch swTabNDOptionsAlert = FindViewById<Switch>(Resource.Id.swTabNDOptionsAlert);
                Switch swTabNDOptionsVibration = FindViewById<Switch>(Resource.Id.swTabNDOptionsVibration);
                Switch swTabNDOptionsUploadOnWifi = FindViewById<Switch>(Resource.Id.swTabNDOptionsUploadOnWifi);
				Switch swNDOptionsScanViaBlutooth = FindViewById<Switch>(Resource.Id.swTabNDOptionsScanViaBlutooth);

                string name = txtTabNDName.Text;
                string companyname = txtTabNDCompanyName.Text;
                string email = txtTabNDEmail.Text;
                string mobile = txtTabNDMobile.Text;
                string address = txtTabNDAddress.Text;
                string alert = swTabNDOptionsAlert.Checked == true ? "1" : "0";
                string vibrate = swTabNDOptionsVibration.Checked == true ? "1" : "0";
                string wifi = swTabNDOptionsUploadOnWifi.Checked == true ? "1" : "0";
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
						objime.SaveProfileAsync(EmpID.ToString(), name, companyname, email, mobile, address, alert, vibrate, wifi,bluetooth);
                        objime.SaveProfileCompleted += delegate(object psender, SaveProfileCompletedEventArgs pe)
                        {
                            try
                            {
                                DataSet ds = new DataSet();
                                string innerxml = pe.Result.InnerXml;
                                if (innerxml.Contains("Success"))
                                {
									BaseClass bc=new BaseClass();
									bc.SetPreferenceManager<int>("AlertVibrate", Convert.ToInt32(vibrate),this);
									bc.SetPreferenceManager<int>("AlertTone", Convert.ToInt32(alert),this);
									bc.SetPreferenceManager<int>("Bluetooth", Convert.ToInt32(bluetooth),this);
									bc.SetPreferenceManager<int>("WifiUpload", Convert.ToInt32(wifi),this);
                                    Toast.MakeText(this, "Settings & Company details updated", ToastLength.Short).Show();
                                }
                                else
                                {
									pdialog.Dismiss();
                                    Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                                }
                                pdialog.Dismiss();
                            }
                            catch
                            {
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

        void imgNDTabLogOut_Click(object sender, EventArgs e)
        {
            try{
		    FinishAffinity();
            //this.Finish();
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.Clear();
            editor.Commit();
            editor.Apply();
            StartActivity(typeof(Login));
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
            }

        }
		void imgNDHelp_Click(object sender, EventArgs e)
		{
			try{

				StartActivity(typeof(Help));
			}
			catch{
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
    }
}

