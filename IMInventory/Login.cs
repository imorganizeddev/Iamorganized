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
using Java.Util.Regex;
using IMInventory.iminventory;
using System.Data;
using Android.Net;
using Android.Preferences;
using Android.Graphics;
using Android.Database.Sqlite;
using Android.Net.Wifi;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using Java.Net;
using System.Security.Cryptography;
using Java.IO;

namespace IMInventory
{
    [Activity(Label = "Login", ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorPortrait, MainLauncher = false)]
    public class Login : Activity
    {
        private ProgressDialog dialog;
        SQLiteDatabase db = null;
        int selecteditempositin = 0;
        int empid;
        ProgressDialog planfetchdialog;
        Dialog forgotpassword;

		ISharedPreferences prefs;

		long Backfrommain = 0;
		long EmpID = 0;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
			prefs = PreferenceManager.GetDefaultSharedPreferences (this);
			Backfrommain = prefs.GetLong ("Backfrommain", 0);
			EmpID = prefs.GetLong ("EmpID", 0);

			if (Backfrommain == 0) {
				if (EmpID > 0) {



					StartActivity (typeof (Main));
					this.Finish ();
				}

			} 
			
			this.RequestWindowFeature (WindowFeatures.NoTitle);
			SetContentView (Resource.Layout.Login);

			db = this.OpenOrCreateDatabase ("ImInventory", FileCreationMode.Private, null);

			//String path = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).ToString() + "/ImInventory";
			//var files = Directory.GetFiles (db.Path);
			//db.ExecSQL("DROP TABLE IF EXISTS " + "tbl_Inventory");
			db.ExecSQL ("CREATE TABLE IF NOT EXISTS "
				+ "tbl_Inventory"
				+ " (ID INTEGER PRIMARY KEY AUTOINCREMENT, EmpID INTEGER,ProjectID VARCHAR, ProjectName VARCHAR, ClientName VARCHAR,Location VARCHAR, Image1 VARCHAR , Image2 VARCHAR, Image3 VARCHAR, Image4 VARCHAR," +
				"ItemDescription VARCHAR, Brand VARCHAR, Quantity VARCHAR, ModelNumber VARCHAR, UnitCost VARCHAR, Notes VARCHAR , Addeddate VARCHAR,BarCodeNumber VARCHAR,AudioFileName VARCHAR,InventoryType VARCHAR,UploadURL VARCHAR,UploadedBytes VARCHAR,UploadedStatus VARCHAR" +
				"" +
				"" +
				");");


			db.ExecSQL ("CREATE TABLE IF NOT EXISTS "
			   + "tbl_UserDetails"
			   + " (ID INTEGER PRIMARY KEY AUTOINCREMENT, EmpID INTEGER,Name VARCHAR, Email VARCHAR, Company VARCHAR,Phone VARCHAR, Address VARCHAR , Alert VARCHAR, Vibrate VARCHAR, Wifi VARCHAR," +
			   "Bluetooth VARCHAR" + ");");

			InitializetypefaceandClickevents ();
			

            // Create your application here
        }

        private void InitializetypefaceandClickevents()
        {

            Typeface tf = Typeface.CreateFromAsset(Assets, "Fonts/ROBOTO-LIGHT.TTF");
            TextView lblmember = FindViewById<TextView>(Resource.Id.lblmember);
            lblmember.Typeface = tf;
            lblmember.Invalidate();

            EditText txtusername = FindViewById<EditText>(Resource.Id.txtUserName);
            txtusername.Typeface = tf;
            txtusername.Invalidate();

            EditText txtpassword = FindViewById<EditText>(Resource.Id.txtPassword);
            txtpassword.Typeface = tf;
            txtpassword.Invalidate();
            TextView lblprojectname = FindViewById<TextView>(Resource.Id.lblprojectname);
            lblprojectname.Typeface = tf;
            lblprojectname.Invalidate();
            // Get our button from the layout resource,
            // and attach an event to it
            ImageView button = FindViewById<ImageView>(Resource.Id.btnsignin);

            button.Click += Button_Click;

            ImageView buttonRedirectSignup = FindViewById<ImageView>(Resource.Id.btnRedirectSignup);
            buttonRedirectSignup.Click += buttonRedirectSignup_Click;
            TextView lblforgotpassword = FindViewById<TextView>(Resource.Id.lblforgotpassword);
            lblforgotpassword.Click += lblforgotpassword_Click;
        }

        void lblforgotpassword_Click(object sender, EventArgs e)
        {
            forgotpassword = new Dialog(this);
            forgotpassword.RequestWindowFeature((int)WindowFeatures.NoTitle);
            forgotpassword.SetContentView(Resource.Layout.ForgotPassword);
            ImageView btnSendForgetPassword = (ImageView)forgotpassword.FindViewById(Resource.Id.btnSendForgetPassword);
            ImageView btnCancelSendForgetPassword = (ImageView)forgotpassword.FindViewById(Resource.Id.btnCancelSendForgetPassword);
            Typeface tf = Typeface.CreateFromAsset(Assets, "Fonts/ROBOTO-LIGHT.TTF");
            EditText txtPopUpForgotpasswordEmailID = (EditText)forgotpassword.FindViewById(Resource.Id.txtPopUpForgotpasswordEmailID);
            txtPopUpForgotpasswordEmailID.Typeface = tf;
            txtPopUpForgotpasswordEmailID.Invalidate();

            btnSendForgetPassword.Click += delegate
            {
                WebService objWebService = new WebService();
                if (!isValidEmail(txtPopUpForgotpasswordEmailID.Text.Trim()))
                {
                    Toast.MakeText(this, "Please give proper email", ToastLength.Long).Show();
                }
                else
                {
                    objWebService.ForgotPasswordAsync(txtPopUpForgotpasswordEmailID.Text.Trim());
                    objWebService.ForgotPasswordCompleted += objWebService_ForgotPasswordCompleted;
                }
            };

            btnCancelSendForgetPassword.Click += delegate
            {
                forgotpassword.Dismiss();
            };
            forgotpassword.Show();
        }

        void objWebService_ForgotPasswordCompleted(object sender, ForgotPasswordCompletedEventArgs e)
        {
            try
            {
                if (e.Result.InnerXml.Contains("success"))
                {
					Toast.MakeText(this, "An email has been sent with the login credentials for your I.M Organized account", ToastLength.Long).Show();
                    forgotpassword.Dismiss();
                }
                else {
                    Toast.MakeText(this, "Something error happend. Please try again later.", ToastLength.Long).Show();
                }

            }
            catch { }
        }


        public override void OnBackPressed()
        {
            createDialog();
        }

        void buttonRedirectSignup_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(Signup));
        }
        public void createDialog()
        {

            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetMessage("Are you sure want to exit from Im Inventory?");
            builder.SetCancelable(false);
            //builder .SetIcon (Resource.Drawable.Icon);
            builder.SetPositiveButton("Yes", (object sender, DialogClickEventArgs e) =>
            {
                this.Finish();
                ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                ISharedPreferencesEditor editor = prefs.Edit();

				editor.PutLong ("Backfrommain", 0);
				editor.Commit ();
				editor.Apply ();
               // editor.Remove("EmpID");
               // editor.Remove("ProjectID");
               // editor.Remove("InventoryType");
              //  editor.Remove("ProjectName");
              //  editor.Remove("ClientName");
             //   editor.Commit();


            });
            builder.SetNegativeButton("No", (object sender, DialogClickEventArgs e) =>
            {

            });



            AlertDialog alertdialog = builder.Create();
            alertdialog.Show();
        }
        void Button_Click(object sender, EventArgs e)
        {
            try
            {

                //ProgressBar pb= FindViewById<ProgressBar> (Resource.Id.imgprogress);

                //pb.Progress=60;




                EditText txtusername = FindViewById<EditText>(Resource.Id.txtUserName);
                string username = txtusername.Text;

                EditText txtpassword = FindViewById<EditText>(Resource.Id.txtPassword);
                string password = txtpassword.Text;

              //  if (isNetworkConnected() == false)
               // {

                 //   Toast.MakeText(this, "Please check network connection and try again !!!", ToastLength.Long).Show();

                //}

                //else
                    if (username.Trim() == string.Empty)
                    {
                        //pb.Visibility=ViewStates.Invisible;
                        //pb.Progress=0;
                        //dialog.Dismiss();
                        Toast.MakeText(this, "Please enter username", ToastLength.Long).Show();
                    }

                    //else if(!isValidEmail(username.Trim())){

                        //dialog.Hide();
                    //Toast.MakeText (this, "Please give proper email", ToastLength.Long).Show ();
                    //}
                    else if (password.Trim() == string.Empty)
                    {
                        //dialog.Hide();
                        Toast.MakeText(this, "Please enter password", ToastLength.Long).Show();

                    }
                    else
                    {

					if (isNetworkConnected ()) {
						dialog = ProgressDialog.Show (this, "Log In", "Please wait.....");
						WifiManager wifiManager = (WifiManager)GetSystemService (Context.WifiService);
						WifiInfo wInfo = wifiManager.ConnectionInfo;
						String MACAdress = Android.Provider.Settings.Secure.GetString (ContentResolver, Android.Provider.Settings.Secure.AndroidId);
						WebService objime = new WebService ();
						objime.Timeout = 1;
						objime.EmpLoginAsync (username, password, MACAdress, "Android");

						objime.EmpLoginCompleted += getempxml;

					} else { 
					
					Toast.MakeText (this, "Please connect to network. And try again!!!", ToastLength.Long).Show ();
					
					}

                        //System.Xml.XmlNode xml=objime.EmpLogin(username,password);




                        //StartActivity(typeof(Main));
                    }



            }
            catch
            {
                Toast.MakeText(this, "Invalid username or password", ToastLength.Short).Show();

                dialog.Dismiss();
            }
        }

        private Boolean isNetworkConnected()
        {
            var connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            var activeConnection = connectivityManager.ActiveNetworkInfo;
            if ((activeConnection != null) && activeConnection.IsConnected)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void getempxml(object sender, EmpLoginCompletedEventArgs e)
        {

            try
            {
                Payment objPayment = new Payment();
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

                empid = Convert.ToInt16(dataTable.Rows[0]["EmpID"]);
                int isinternal = Convert.ToInt16(dataTable.Rows[0]["IsInternal"]);
                int UserType = Convert.ToInt16(dataTable.Rows[0]["UserType"]);
                string UserPlan = Convert.ToString(dataTable.Rows[0]["UserPlan"]);
				string Email = Convert.ToString(dataTable.Rows[0]["Email"]);
                string StripeSubscriptionID = Convert.ToString(dataTable.Rows[0]["StripeSubscriptionID"]);
                string AccessFindITExport = Convert.ToString(dataTable.Rows[0]["AccessFindITExport"]);
                string AccessBarCodeScan = Convert.ToString(dataTable.Rows[0]["AccessBarCodeScan"]);
                string AccessExport = Convert.ToString(dataTable.Rows[0]["AccessExport"]);
                string UploadedSize = Convert.ToString(dataTable.Rows[0]["UploadedSize"]);
				string Name = Convert.ToString (dataTable.Rows [0] ["Name"]);
                if (empid > 0)
                {
                    //Toast.MakeText (this, "Your successfully log in", ToastLength.Short).Show ();
                    //dialog.Hide();

					if(isinternal==0){
						if(UserType==2 && StripeSubscriptionID==""){

							StripeSubscriptionID="null|||null";
						}
                    if (StripeSubscriptionID != "")
                    {
                        bool SubscriptionStatus = objPayment.GetSubscriptionStatus(StripeSubscriptionID.Split(new String[] { "|||" }, StringSplitOptions.None)[1].ToString(), StripeSubscriptionID.Split(new String[] { "|||" }, StringSplitOptions.None)[0].ToString());
                        if (SubscriptionStatus)
                        {
                            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                            ISharedPreferencesEditor editor = prefs.Edit();
                            //editor.Clear();
                            editor.PutLong("EmpID", empid);
                            editor.PutLong("IsInternal", isinternal);
                            editor.PutString("UserPlan", UserPlan);
                            editor.PutLong("UserType", UserType);
                            editor.PutString("StripeSubscriptionID", StripeSubscriptionID);
                            editor.PutString("AccessFindITExport", AccessFindITExport);
                            editor.PutString("AccessBarCodeScan", AccessBarCodeScan);
                            editor.PutString("AccessExport", AccessExport);
                            editor.PutString("UploadedSize", UploadedSize);
							editor.PutString("Email", Email);
                            editor.PutString ("Name", Name);
                            editor.Commit();
                            // applies changes synchronously on older APIs
                            editor.Apply();
                            dialog.Dismiss();
                            //Toast.MakeText (this, isinternal.ToString(), ToastLength.Long).Show ();
                            EditText txtusername = FindViewById<EditText>(Resource.Id.txtUserName);
                            txtusername.Text = "";

                            EditText txtpassword = FindViewById<EditText>(Resource.Id.txtPassword);
                            txtpassword.Text = "";
								this.Finish ();
                            StartActivity(typeof(Main));
                        }
                        else
                        {
                            AlertDialog.Builder builder = new AlertDialog.Builder(this);
                            builder.SetTitle("Renew Subscription");
							builder.SetMessage("Your subscription was not renewed. Would you like to create a new subscription?");
                            // builder.SetSingleChoiceItems(arrlist, 0, ListClicked);
                            builder.SetPositiveButton("Yes", (object se, DialogClickEventArgs ev) =>
                            {
                                OpenPlanPopUp(Email);
                            });
                            builder.SetNegativeButton("No", (object se, DialogClickEventArgs ev) =>
                           {
                               WifiManager wifiManager = (WifiManager)GetSystemService(Context.WifiService);
                               WifiInfo wInfo = wifiManager.ConnectionInfo;
                               String MACAdress = Android.Provider.Settings.Secure.GetString(ContentResolver, Android.Provider.Settings.Secure.AndroidId);
                               WebService objWebService = new WebService();
                               objWebService.ExternalSignUpUserUpdateAsync(empid.ToString(), "", "", MACAdress);
                               ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                               ISharedPreferencesEditor editor = prefs.Edit();
                               editor.Clear();
                               editor.PutLong("EmpID", empid);
                               editor.PutLong("IsInternal", isinternal);
                               editor.PutString("UserPlan", "");
                               editor.PutLong("UserType", 1);
                               editor.PutString("StripeSubscriptionID", "");
                               editor.PutString("AccessFindITExport", "0");
                               editor.PutString("AccessBarCodeScan", "0");
                               editor.PutString("AccessExport", "0");
                               editor.PutString("UploadedSize", "0");
									editor.PutString ("Name", Name);
                               editor.Commit();
                               // applies changes synchronously on older APIs
                               editor.Apply();
                               dialog.Dismiss();
                               //Toast.MakeText (this, isinternal.ToString(), ToastLength.Long).Show ();
                               EditText txtusername = FindViewById<EditText>(Resource.Id.txtUserName);
                               txtusername.Text = "";

                               EditText txtpassword = FindViewById<EditText>(Resource.Id.txtPassword);
                               txtpassword.Text = "";
									this.Finish ();
                               StartActivity(typeof(Main));
                               //objWebService.ExternalSignUpUserUpdateCompleted+=getexterneluserupdatexml;
                           });
                            builder.SetCancelable(false);
                            builder.Show();
                        }
                    }
                    else
                    {
                        ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                        ISharedPreferencesEditor editor = prefs.Edit();
                        editor.Clear();
                        editor.PutLong("EmpID", empid);
                        editor.PutLong("IsInternal", isinternal);
                        editor.PutString("UserPlan", UserPlan);
                        editor.PutLong("UserType", UserType);
                        editor.PutString("StripeSubscriptionID", StripeSubscriptionID);
                        editor.PutString("AccessFindITExport", AccessFindITExport);
                        editor.PutString("AccessBarCodeScan", AccessBarCodeScan);
                        editor.PutString("AccessExport", AccessExport);
                        editor.PutString("UploadedSize", UploadedSize);
							editor.PutString ("Name", Name);
                        editor.Commit();
                        // applies changes synchronously on older APIs
                        editor.Apply();
                        dialog.Dismiss();
                        //Toast.MakeText (this, isinternal.ToString(), ToastLength.Long).Show ();
                        EditText txtusername = FindViewById<EditText>(Resource.Id.txtUserName);
                        txtusername.Text = "";

                        EditText txtpassword = FindViewById<EditText>(Resource.Id.txtPassword);
                        txtpassword.Text = "";
							this.Finish ();
                        StartActivity(typeof(Main));

						}
					}
					else{
						ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
						ISharedPreferencesEditor editor = prefs.Edit();
						editor.Clear();
						editor.PutLong("EmpID", empid);
						editor.PutLong("IsInternal", isinternal);
						editor.PutString("UserPlan", "Platinum");
						editor.PutLong("UserType", 2);
						editor.PutString("StripeSubscriptionID", StripeSubscriptionID);
						editor.PutString("AccessFindITExport", "1");
						editor.PutString("AccessBarCodeScan", "1");
						editor.PutString("AccessExport", "1");
						editor.PutString("UploadedSize", "max");
						editor.PutString ("Name", Name);
						editor.Commit();
						// applies changes synchronously on older APIs
						editor.Apply();
						dialog.Dismiss();
						//Toast.MakeText (this, isinternal.ToString(), ToastLength.Long).Show ();
						EditText txtusername = FindViewById<EditText>(Resource.Id.txtUserName);
						txtusername.Text = "";

						EditText txtpassword = FindViewById<EditText>(Resource.Id.txtPassword);
						txtpassword.Text = "";
						this.Finish ();
						StartActivity(typeof(Main));

					}
                }
                else
                {
                    //dialog.Hide();

                    Toast.MakeText(this, "Invalid username or password", ToastLength.Short).Show();
                    dialog.Dismiss();

                }
            }
            catch (Exception ex)
            {
                dialog.Dismiss();
                Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
            }
        }
        private void OpenPlanPopUp(string email)
        {
            string[] arrplans = null;
            try
            {
                GeneralValues objGeneralValues = new GeneralValues();
                planfetchdialog = ProgressDialog.Show(this, "Fectching Plan", "Please wait.....");
                StreamReader reader = null;
                var webrequest = (HttpWebRequest)WebRequest.Create("https://api.stripe.com/v1/plans?limit=3");
                webrequest.Method = "GET";
                webrequest.Headers.Add("Authorization", "Bearer " + objGeneralValues.SecretKey);
                HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
                reader = new StreamReader(webresponse.GetResponseStream());
                string obj = reader.ReadToEnd();
                var parsedjson = JObject.Parse(obj)["data"].Value<JArray>();
                string[] arrlist = new string[parsedjson.Count];
                arrplans = new string[parsedjson.Count];
                int i = 0;
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
                planfetchdialog.Dismiss();
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetTitle("Choose plan");
                builder.SetSingleChoiceItems(arrlist, 0, ListClicked);
                builder.SetNeutralButton("Ok", (object sender, DialogClickEventArgs e) =>
                {
                    string item = arrplans[selecteditempositin].ToString();
                    var token = CreateToken(item, email);
                });

                builder.SetCancelable(false);
                builder.Show();
            }
            catch (Exception ex)
            {
            }
        }
        private Stripe.Token CreateToken(string item, string email)
        {
            try
            {

                GeneralValues objGeneralValues = new GeneralValues();
                string plan = item.Split(new String[] { "|||" }, StringSplitOptions.None)[0].ToString();
                WifiManager wifiManager = (WifiManager)GetSystemService(Context.WifiService);
                WifiInfo wInfo = wifiManager.ConnectionInfo;
                String MACAdress = Android.Provider.Settings.Secure.GetString(ContentResolver, Android.Provider.Settings.Secure.AndroidId);
                string amount = item.Split(new String[] { "|||" }, StringSplitOptions.None)[1].ToString();
                string id = item.Split(new String[] { "|||" }, StringSplitOptions.None)[2].ToString();
                string planid = id;
                decimal PaymentAmount = Convert.ToDecimal(amount);

                Dialog paymentdialog = new Dialog(this);
                paymentdialog.RequestWindowFeature((int)WindowFeatures.NoTitle);
                paymentdialog.SetContentView(Resource.Layout.Payment);
                paymentdialog.SetCanceledOnTouchOutside(false);
                Typeface tf = Typeface.CreateFromAsset(Assets, "Fonts/ROBOTO-LIGHT.TTF");
                string encryptedplanname = URLEncoder.Encode(Encrypt(plan), "UTF-8");
                string encryptedamount = URLEncoder.Encode(Encrypt(amount), "UTF-8");
                string encryptedplanid = URLEncoder.Encode(Encrypt(planid), "UTF-8");
                string encrypteduserid = URLEncoder.Encode(Encrypt(empid.ToString()), "UTF-8");
                string encryptedmacadress = URLEncoder.Encode(Encrypt(MACAdress), "UTF-8");
                string encryptedemailaddress = URLEncoder.Encode(Encrypt(email), "UTF-8");
                string url = objGeneralValues.PaymentURL + "?planname=" + encryptedplanname + "&amount=" + encryptedamount + "&planid=" + encryptedplanid + "&user=" + encrypteduserid + "&macaddress=" + encryptedmacadress + "&email=" + encryptedemailaddress;
                Intent browserintent = new Intent(Intent.ActionView, Android.Net.Uri.Parse("http://59.162.181.91/dtswork/ImInventoryWebPayment/default.aspx?planname=" + encryptedplanname + "&amount=" + encryptedamount + "&planid=" + encryptedplanid + "&user=" + encrypteduserid + "&macaddress=" + encryptedmacadress + "&email=" + encryptedemailaddress));
                selecteditempositin = 0;
                StartActivity(browserintent);
            }
            catch (Exception ex)
            {

            }
            return new Stripe.Token();
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
        private bool isValidEmail(String email)
        {
            String EMAIL_PATTERN = "^[_A-Za-z0-9-\\+]+(\\.[_A-Za-z0-9-]+)*@"
                + "[A-Za-z0-9-]+(\\.[A-Za-z0-9]+)*(\\.[A-Za-z]{2,})$";

            Java.Util.Regex.Pattern pattern = Java.Util.Regex.Pattern.Compile(EMAIL_PATTERN);
            Matcher matcher = pattern.Matcher(email);
            return matcher.Matches();
        }
    }
}

