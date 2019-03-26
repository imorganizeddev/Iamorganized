using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Util.Regex;
using IMInventory.iminventory;
using System.Data;
using Android.Net;
using Android.Preferences;
using Android.Graphics;
using Android.Text;
using Stripe;
using Android.Net.Wifi;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading;
using Android.Webkit;
using System.Text;
using System.Security.Cryptography;
using System.Security.Policy;
using Java.Net;


namespace IMInventory
{
	[Activity(Label = "Signup", ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorPortrait, MainLauncher = false)]
    public class Signup : Activity
    {
        WebView paymentwebview;
        private ProgressDialog dialog;
        WebService objime = new WebService();
        StripeView stripeView;

        EditText name;
        EditText address1;
        EditText address2;
        EditText city;
        EditText state;
        EditText zip;
        EditText country;
        Android.Widget.Button buttonToken;
        ProgressDialog paymentprogressdialog;
        ProgressDialog planfetchdialog;
        Stripe.Token token = null;
        Stripe.Token tokenforsubscription = null;
        bool IsBusinessUser = false;
        Decimal PaymentAmount = 0;
        int selecteditempositin = 0;
        string customerid = "";
        string planid = "";
        string plan = "";
        string subscriptionid = "";
        string userid = "";
        int EmployeeID;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.Signup);
            this.Window.SetSoftInputMode(SoftInput.AdjustPan);
            // Define the font face
            Typeface tf = Typeface.CreateFromAsset(Assets, "Fonts/ROBOTO-LIGHT.TTF");

            EditText txtfirstname = FindViewById<EditText>(Resource.Id.txtSignUpFirstName);
            txtfirstname.Typeface = tf;
            txtfirstname.Invalidate();

            EditText txtlastname = FindViewById<EditText>(Resource.Id.txtSignUpLastName);
            txtlastname.Typeface = tf;
            txtlastname.Invalidate();

            EditText txtusername = FindViewById<EditText>(Resource.Id.txtSignUpUserName);
            txtusername.Typeface = tf;
            txtusername.Invalidate();

            EditText txtemail = FindViewById<EditText>(Resource.Id.txtSignUpEmail);
            txtemail.Typeface = tf;
            txtemail.Invalidate();

            EditText txtpassword = FindViewById<EditText>(Resource.Id.txtSignUpPassword);
            txtpassword.Typeface = tf;
            txtpassword.Invalidate();

            EditText txtrepassword = FindViewById<EditText>(Resource.Id.txtSignUpRePassword);
            txtrepassword.Typeface = tf;
            txtrepassword.Invalidate();

            TextView lblaccept = FindViewById<TextView>(Resource.Id.lblaccept);
            lblaccept.Typeface = tf;
            lblaccept.Invalidate();

            TextView lblterms = FindViewById<TextView>(Resource.Id.lblterm);
            lblterms.Typeface = tf;
            lblterms.Invalidate();

            RadioButton rdbFreeUse = FindViewById<RadioButton>(Resource.Id.rdbFreeUse);
            rdbFreeUse.Typeface = tf;
            rdbFreeUse.Invalidate();
            RadioButton rdbBusinessUse = FindViewById<RadioButton>(Resource.Id.rdbBusinessUse);
            rdbBusinessUse.Typeface = tf;
            rdbBusinessUse.Invalidate();
            if (IMApplication.player != null)
            {
                IMApplication.player.Stop();
                IMApplication.player = null;
            }
            //lblterms.Text=Html.FromHtml("<h2>Title</h2><br><p>Description here</p>").ToString();
            //WebService objime = new WebService();

            // Free User Checked
            rdbFreeUse.Click += delegate
            {
                if (rdbFreeUse.Checked)
                {
                    objime = new WebService();
                    rdbBusinessUse.Checked = false;
                    objime.SignupPlanAsync("1");
                    objime.SignupPlanCompleted += getSubscriptionxml;
                    IsBusinessUser = false;
                }
            };

            // Business User Checked
            rdbBusinessUse.Click += delegate
            {
                EditText txtuserusername = FindViewById<EditText>(Resource.Id.txtSignUpUserName);
                string username = txtuserusername.Text;
                EditText txtuserpassword = FindViewById<EditText>(Resource.Id.txtSignUpPassword);
                string password = txtuserpassword.Text;
                WifiManager wifiManager = (WifiManager)GetSystemService(Context.WifiService);
                WifiInfo wInfo = wifiManager.ConnectionInfo;
                String MACAdress = Android.Provider.Settings.Secure.GetString(ContentResolver, Android.Provider.Settings.Secure.AndroidId);
                objime = new WebService();
                objime.EmpLoginAsync(username, password, MACAdress,"Android");
                objime.EmpLoginCompleted += delegate(object psender, EmpLoginCompletedEventArgs pe)
                {
                    DataSet ds = new DataSet();
                    string innerxml = pe.Result.InnerXml.ToString();
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
                    ds.Tables.Add(dataTable);
                    System.IO.StringReader xmlSR = new System.IO.StringReader(innerxml);

                    ds.ReadXml(xmlSR, XmlReadMode.IgnoreSchema);

                    dataTable = ds.Tables[0];

                    int empid = Convert.ToInt16(dataTable.Rows[0]["EmpID"]);
                    if (empid == 0)
                    {
                        if (rdbBusinessUse.Checked)
                        {


                            EditText txtuserfirstname = FindViewById<EditText>(Resource.Id.txtSignUpFirstName);
                            string firstname = txtuserfirstname.Text;

                            EditText txtuserlastname = FindViewById<EditText>(Resource.Id.txtSignUpLastName);
                            string lastname = txtuserlastname.Text;



                            EditText txtuseremail = FindViewById<EditText>(Resource.Id.txtSignUpEmail);
                            string email = txtuseremail.Text;



                            EditText txtuserrepassword = FindViewById<EditText>(Resource.Id.txtSignUpRePassword);
                            string repassword = txtuserrepassword.Text;




                            if (isAllInputOk(firstname, lastname, username, email, password, repassword))
                            {
                                rdbFreeUse.Checked = false;
                                objime.SignupPlanAsync("2");
                                objime.SignupPlanCompleted += getSubscriptionxml;
                            }
                            else
                            {
                                RadioButton rdbFreeUser = FindViewById<RadioButton>(Resource.Id.rdbFreeUse);
                                RadioButton rdbBusinessUser = FindViewById<RadioButton>(Resource.Id.rdbBusinessUse);
                                rdbFreeUser.Checked = false;
                                rdbBusinessUser.Checked = false;
                            }


                        }
                        rdbFreeUse.Enabled = true;
                        rdbBusinessUse.Enabled = true;

                    }
                    else
                    {
                        rdbFreeUse.Enabled = false;
                        rdbBusinessUse.Enabled = false;
                        Toast.MakeText(this, "You are already subscribed", ToastLength.Short).Show();

                    }
                };

            };

            ImageView button = FindViewById<ImageView>(Resource.Id.btnsignup);
            button.Click += btnSignup_Click;
            ImageView btnback = FindViewById<ImageView>(Resource.Id.imgsignupback);
            btnback.Click += delegate
            {
                StartActivity(typeof(Login));
            };
            TextView lblterm = FindViewById<TextView>(Resource.Id.lblterm);
            lblterm.Click += delegate
            {
                StartActivity(typeof(Term));
            };
        }

        private void getSubscriptionxml(object sender, SignupPlanCompletedEventArgs e)
        {
            //if (dialog != null) {
            //    dialog.Dismiss();
            //}
            //System.IO.FileStream fsReadXml = new System.IO.FileStream 
            //	(e.Result.ToString(), System.IO.FileMode.Open);
            //ds.ReadXml (fsReadXml);
            //XmlDocument doc = new XmlDocument();
            //doc.Load(e.Result.InnerXml);
            try
            {
                DataSet ds = new DataSet();
                string innerxml = e.Result.InnerXml.ToString();
                innerxml = "<ds><table1>" + innerxml + "</table1></ds>";
                DataTable dataTable = new DataTable("table1");
                dataTable.Columns.Add("PlanID", typeof(string));
                dataTable.Columns.Add("PlanName", typeof(string));
                dataTable.Columns.Add("Desc", typeof(string));
                dataTable.Columns.Add("amount", typeof(string));

                ds.Tables.Add(dataTable);
                System.IO.StringReader xmlSR = new System.IO.StringReader(innerxml);

                ds.ReadXml(xmlSR, XmlReadMode.IgnoreSchema);

                dataTable = ds.Tables[0];
                Dialog dialog = new Dialog(this);

                dialog.RequestWindowFeature((int)WindowFeatures.NoTitle);
                dialog.SetContentView(Resource.Layout.PopUpWindow);

                TextView lblsignupsubhead = (TextView)dialog.FindViewById(Resource.Id.lblsignupsubhead);
                TextView lblsignupsubtext = (TextView)dialog.FindViewById(Resource.Id.lblsignupsubtext);

                // Define the font face, color & size
                Typeface tf = Typeface.CreateFromAsset(Assets, "Fonts/ROBOTO-LIGHT.TTF");

                lblsignupsubhead.Typeface = tf;
                lblsignupsubhead.Invalidate();
                lblsignupsubhead.SetTextColor(Android.Graphics.Color.Black);
                lblsignupsubhead.SetTextSize(Android.Util.ComplexUnitType.Sp, 20);


                lblsignupsubtext.Typeface = tf;
                lblsignupsubtext.Invalidate();
                lblsignupsubtext.SetTextColor(Android.Graphics.Color.Black);
                lblsignupsubtext.SetTextSize(Android.Util.ComplexUnitType.Sp, 14);
                lblsignupsubtext.Gravity = GravityFlags.Center;
                lblsignupsubtext.TextAlignment = Android.Views.TextAlignment.Gravity;
                // Define the font face, color & size


                lblsignupsubhead.Visibility = ViewStates.Visible;
                string planname = Html.FromHtml(dataTable.Rows[0]["PlanName"].ToString()).ToString();
                string amount = Html.FromHtml(dataTable.Rows[0]["amount"].ToString()).ToString();
                if (dataTable.Rows.Count > 0)
                {

                    string desc = dataTable.Rows[0]["Desc"].ToString();
                    desc = Html.FromHtml(dataTable.Rows[0]["Desc"].ToString(), null, null).ToString();
                    //lblsignupsubhead.Text = dataTable.Rows[0]["PlanName"].ToString();
                    lblsignupsubhead.Text = Html.FromHtml(dataTable.Rows[0]["PlanName"].ToString()).ToString();
                    lblsignupsubtext.SetText(Html.FromHtml(dataTable.Rows[0]["Desc"].ToString()).ToString(), TextView.BufferType.Editable);
                    PaymentAmount = Convert.ToDecimal(amount);
                }
                RadioButton rdbFreeUse = FindViewById<RadioButton>(Resource.Id.rdbFreeUse);

                RadioButton rdbBusinessUse = FindViewById<RadioButton>(Resource.Id.rdbBusinessUse);

                ImageView dialogButton = (ImageView)dialog.FindViewById(Resource.Id.btnclosebusinessuse);
                dialogButton.Click += delegate
                {
                    dialog.Dismiss();

                    if (!planname.ToLower().Contains("free"))
                    {
                        if (rdbBusinessUse.Checked)
                        {
                            planfetchdialog = ProgressDialog.Show(this, "Fectching Plan", "Please wait.....", true);
                            CreateSignUp();
                            //OpenPlanPopUp();
                            new Thread(new ThreadStart(delegate
                                {
                                    //LOAD METHOD TO GET ACCOUNT INFO
                                    RunOnUiThread(() => OpenPlanPopUp());
                                    //HIDE PROGRESS DIALOG
                                    RunOnUiThread(() => planfetchdialog.Dismiss());
                                })).Start();
                            //CreateToken();
                            //planfetchdialog.Dismiss();
                        }
                    }
                };
                dialog.Show();
            }

            catch (Exception ex)
            {
                Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
            }
        }
        private void getsignupxml(object sender, GetEmpDetailsByEmpIDCompletedEventArgs e)
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
                string AccessFindITExport = Convert.ToString(dataTable.Rows[0]["AccessFindITExport"]);
                string AccessBarCodeScan = Convert.ToString(dataTable.Rows[0]["AccessBarCodeScan"]);
                string AccessExport = Convert.ToString(dataTable.Rows[0]["AccessExport"]);
                string UploadedSize = Convert.ToString(dataTable.Rows[0]["UploadedSize"]);
                if (empid > 0)
                {
                    //Toast.MakeText (this, "Your successfully log in", ToastLength.Short).Show ();
                    //dialog.Hide();
                    ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                    ISharedPreferencesEditor editor = prefs.Edit();
                    //editor.Clear();
                    editor.PutLong("EmpID", empid);
                    editor.PutLong("IsInternal", isinternal);
                    editor.PutString("UserPlan", UserPlans);
                    editor.PutString("AccessFindITExport", AccessFindITExport);
                    editor.PutString("AccessBarCodeScan", AccessBarCodeScan);
                    editor.PutString("AccessExport", AccessExport);
                    editor.PutString("UploadedSize", UploadedSize);
                    editor.PutLong("UserType", UserType);

                    editor.PutString("StripeSubscriptionID", StripeSubscriptionID);
                    editor.Commit();
                    // applies changes synchronously on older APIs
                    editor.Apply();



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
        void btnSignup_Click(object sender, EventArgs e)
        {
            WifiManager wifiManager = (WifiManager)GetSystemService(Context.WifiService);
            WifiInfo wInfo = wifiManager.ConnectionInfo;
            String MACAdress = Android.Provider.Settings.Secure.GetString(ContentResolver, Android.Provider.Settings.Secure.AndroidId);
            dialog = ProgressDialog.Show(this, "Creating Account", "Please wait.....");
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetCancelable(false);
            if (IsBusinessUser)
            {
                WebService objWebService = new WebService();

                WebService objime = new WebService();
                objime.GetEmpDetailsByEmpIDAsync(EmployeeID.ToString(), MACAdress);
                objime.GetEmpDetailsByEmpIDCompleted += getsignupxml;
                dialog.Dismiss();
                AlertDialog.Builder AlertDialogbuilder = new AlertDialog.Builder(this);

                AlertDialogbuilder.SetMessage("You are successfully register as a business user for " + plan+". Please login again to continue.");

                AlertDialogbuilder.SetCancelable(false);

                AlertDialogbuilder.SetNeutralButton("Ok", (object buildersender, DialogClickEventArgs ev) =>
                {
						StartActivity(typeof(Login));
                });
                AlertDialog alertdialog = AlertDialogbuilder.Create();
                alertdialog.Show();

            }
            else
            {

                EditText txtfirstname = FindViewById<EditText>(Resource.Id.txtSignUpFirstName);
                string firstname = txtfirstname.Text;

                EditText txtlastname = FindViewById<EditText>(Resource.Id.txtSignUpLastName);
                string lastname = txtlastname.Text;

                EditText txtusername = FindViewById<EditText>(Resource.Id.txtSignUpUserName);
                string username = txtusername.Text;

                EditText txtemail = FindViewById<EditText>(Resource.Id.txtSignUpEmail);
                string email = txtemail.Text;

                EditText txtpassword = FindViewById<EditText>(Resource.Id.txtSignUpPassword);
                string password = txtpassword.Text;

                EditText txtrepassword = FindViewById<EditText>(Resource.Id.txtSignUpRePassword);
                string repassword = txtrepassword.Text;
                WebService obj = new WebService();
                if (isNetworkConnected() == false)
                {
                    // If not connected to internet
                    Toast.MakeText(this, "You are not conncted to internet", ToastLength.Long).Show();
                }
                else
                {
                    if (isAllInputOk(firstname, lastname, username, email, password, repassword))
                    {
                        obj.ExternalSignUpAsync(firstname + "" + lastname, "", email, "", "", password, username, "1", MACAdress, "", "","Android");
                        obj.ExternalSignUpCompleted -= getempxml;
                        obj.ExternalSignUpCompleted += getempxml;

                    }
                }
            }
            // builder.SetNeutralButton("Ok", (object buildersender, DialogClickEventArgs eve) =>
            // {
            //    StartActivity(typeof(Main));
            //  });
            // AlertDialog alertdialog = builder.Create();
            // alertdialog.Show();
        }

        public void CreateSignUp()
        {

            try
            {

                // Collect Data from all Input Field
                EditText txtfirstname = FindViewById<EditText>(Resource.Id.txtSignUpFirstName);
                string firstname = txtfirstname.Text;

                EditText txtlastname = FindViewById<EditText>(Resource.Id.txtSignUpLastName);
                string lastname = txtlastname.Text;

                EditText txtusername = FindViewById<EditText>(Resource.Id.txtSignUpUserName);
                string username = txtusername.Text;

                EditText txtemail = FindViewById<EditText>(Resource.Id.txtSignUpEmail);
                string email = txtemail.Text;

                EditText txtpassword = FindViewById<EditText>(Resource.Id.txtSignUpPassword);
                string password = txtpassword.Text;

                EditText txtrepassword = FindViewById<EditText>(Resource.Id.txtSignUpRePassword);
                string repassword = txtrepassword.Text;
                // Collect Data from all Input Field

                //Get MAC Adress

                WifiManager wifiManager = (WifiManager)GetSystemService(Context.WifiService);
                WifiInfo wInfo = wifiManager.ConnectionInfo;
                String MACAdress = Android.Provider.Settings.Secure.GetString(ContentResolver, Android.Provider.Settings.Secure.AndroidId);

                // Check Internet Connectivity
                if (isNetworkConnected() == false)
                {
                    // If not connected to internet
                    Toast.MakeText(this, "You are not conncted to internet", ToastLength.Long).Show();
                }
                else
                {
                    if (isAllInputOk(firstname, lastname, username, email, password, repassword))
                    {

                        RadioButton rdbFreeUse = FindViewById<RadioButton>(Resource.Id.rdbFreeUse);
                        RadioButton rdbBusinessUse = FindViewById<RadioButton>(Resource.Id.rdbBusinessUse);
                        if (!rdbBusinessUse.Checked && !rdbFreeUse.Checked)
                        {
                            Toast.MakeText(this, "Please tick either business or free user button", ToastLength.Long).Show();
                        }
                        else
                        {

                            if (rdbBusinessUse.Checked)
                            {
                                WebService obj = new WebService();
                                obj.ExternalSignUpAsync(firstname + "" + lastname, "", email, "", "", password, username, "2", MACAdress, "", "","Android");

                                obj.ExternalSignUpCompleted -= getempxml;
                                obj.ExternalSignUpCompleted += getempxml;
                                IsBusinessUser = true;
                            }
                            else
                            {

                            }
                        }
                    }
                    else
                    {

                        RadioButton rdbFreeUse = FindViewById<RadioButton>(Resource.Id.rdbFreeUse);
                        RadioButton rdbBusinessUse = FindViewById<RadioButton>(Resource.Id.rdbBusinessUse);
                        rdbBusinessUse.Checked = false;
                        rdbFreeUse.Checked = false;
                    }
                }
            }
            catch
            {
                Toast.MakeText(this, "Invalid username or password", ToastLength.Short).Show();
            }


        }
        private void getempxml(object sender, ExternalSignUpCompletedEventArgs e)
        {
            DataSet ds = new DataSet();
            string innerxml = e.Result.InnerXml.ToString();
            try
            {
                innerxml = "<ds><table1>" + innerxml + "</table1></ds>";

            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
            }
            DataTable dataTable = new DataTable("table1");
            dataTable.Columns.Add("EMPID", typeof(string));
            ds.Tables.Add(dataTable);

            System.IO.StringReader xmlSR = new System.IO.StringReader(innerxml);
            ds.ReadXml(xmlSR, XmlReadMode.IgnoreSchema);
            dataTable = ds.Tables[0];
            WifiManager wifiManager = (WifiManager)GetSystemService(Context.WifiService);
            WifiInfo wInfo = wifiManager.ConnectionInfo;
            String MACAdress = Android.Provider.Settings.Secure.GetString(ContentResolver, Android.Provider.Settings.Secure.AndroidId);
            if (!innerxml.ToLower().Contains("taken"))
            {
                if (dataTable.Rows.Count > 0)
                {
                    int empid = Convert.ToInt16(dataTable.Rows[0]["EMPID"]);
                    EmployeeID = Convert.ToInt16(dataTable.Rows[0]["EMPID"]);
                    RadioButton rdbBusinessUse = FindViewById<RadioButton>(Resource.Id.rdbBusinessUse); WebService objime = new WebService();
                    objime.GetEmpDetailsByEmpIDAsync(empid.ToString(), MACAdress);
                    objime.GetEmpDetailsByEmpIDCompleted += getsignupxml;
                    if (empid > 0)
                    {
                        ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                        ISharedPreferencesEditor editor = prefs.Edit();
                        userid = empid.ToString();
                        editor.PutLong("EmpID", empid);
                        editor.Commit();
                        // applies changes synchronously on older APIs
                        editor.Apply();

                        AlertDialog.Builder builder = new AlertDialog.Builder(this);
                        if (!IsBusinessUser)
                        {
                            builder.SetMessage("You are successfully register as free user");
                            builder.SetCancelable(false);

                            builder.SetNeutralButton("Ok", (object buildersender, DialogClickEventArgs ev) =>
                            {
                                StartActivity(typeof(Main));
                            });
                            AlertDialog alertdialog = builder.Create();
                            alertdialog.Show();
                        }


                    }
                    else
                    {
                        Payment objPayment = new Payment();
                        objPayment.CancelSubscription(subscriptionid, customerid);
                        Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                        dialog.Dismiss();
                        RadioButton rdbFreeUse = FindViewById<RadioButton>(Resource.Id.rdbFreeUse);
                        rdbBusinessUse = FindViewById<RadioButton>(Resource.Id.rdbBusinessUse);

                        rdbFreeUse.Checked = false;
                        rdbBusinessUse.Checked = false;
                    }
                }
                else
                {
                    Payment objPayment = new Payment();
                    AlertDialog.Builder builder = new AlertDialog.Builder(this);
                    objPayment.CancelSubscription(subscriptionid, customerid);
                    builder.SetMessage("Username already taken, please enter another username");
                    builder.SetCancelable(false);
                    RadioButton rdbFreeUse = FindViewById<RadioButton>(Resource.Id.rdbFreeUse);
                    RadioButton rdbBusinessUse = FindViewById<RadioButton>(Resource.Id.rdbBusinessUse);
                    rdbFreeUse.Checked = false;
                    rdbBusinessUse.Checked = false;
                    builder.SetNeutralButton("Ok", (object buildersender, DialogClickEventArgs ev) =>
                        {
                        });
                    AlertDialog alertdialog = builder.Create();
                    alertdialog.Show();
                    //Toast.MakeText (this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show ();
                    dialog.Dismiss();
                }
            }
            else
            {
                Payment objPayment = new Payment();
                objPayment.CancelSubscription(subscriptionid, customerid);
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetMessage("Username already taken, please enter another username");
                builder.SetCancelable(false);
                RadioButton rdbFreeUse = FindViewById<RadioButton>(Resource.Id.rdbFreeUse);
                RadioButton rdbBusinessUse = FindViewById<RadioButton>(Resource.Id.rdbBusinessUse);

                rdbFreeUse.Checked = false;
                rdbBusinessUse.Checked = false;
                builder.SetNeutralButton("Ok", (object buildersender, DialogClickEventArgs ev) =>
                    {
                    });
                AlertDialog alertdialog = builder.Create();
                alertdialog.Show();
                //Toast.MakeText (this, "Duplicate Email or username", ToastLength.Short).Show ();
                dialog.Dismiss();

            }
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

        private bool isValidEmail(String email)
        {
            String EMAIL_PATTERN = "^[_A-Za-z0-9-\\+]+(\\.[_A-Za-z0-9-]+)*@"
                + "[A-Za-z0-9-]+(\\.[A-Za-z0-9]+)*(\\.[A-Za-z]{2,})$";

            Java.Util.Regex.Pattern pattern = Java.Util.Regex.Pattern.Compile(EMAIL_PATTERN);
            Matcher matcher = pattern.Matcher(email);
            return matcher.Matches();
        }

        public string[] arrplans = null;
        private void OpenPlanPopUp()
        {
            try
            {
                GeneralValues objGeneralValues = new GeneralValues();
                //planfetchdialog = ProgressDialog.Show(this, "Fectching Plan", "Please wait.....");
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
                string email = txtemail.Text;
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
                string encrypteduserid = URLEncoder.Encode(Encrypt(userid), "UTF-8");
                string encryptedmacadress = URLEncoder.Encode(Encrypt(MACAdress), "UTF-8");
                string encryptedemailaddress = URLEncoder.Encode(Encrypt(email), "UTF-8");
				string url = objGeneralValues.PaymentURL+"?planname=" + encryptedplanname + "&amount=" + encryptedamount + "&planid=" + encryptedplanid + "&user=" + encrypteduserid + "&macaddress=" + encryptedmacadress + "&email=" + encryptedemailaddress;
				Intent browserintent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(objGeneralValues.PaymentURL+"?planname=" + encryptedplanname + "&amount=" + encryptedamount + "&planid=" + encryptedplanid + "&user=" + encrypteduserid + "&macaddress=" + encryptedmacadress + "&email=" + encryptedemailaddress));
                selecteditempositin = 0;
                if (plan.Trim().ToLower().Contains("platinum"))
                {
                    objWebService.ExternalSignUpUserSubscriptionAsync(EmployeeID.ToString(), MACAdress, "1", "1", "1");
                }
                else
                {
                    objWebService.ExternalSignUpUserSubscriptionAsync(EmployeeID.ToString(), MACAdress, "0", "0", "0");
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


        private Boolean isAllInputOk(string firstname, string lastname, string username, string email, string password, string repassword)
        {
            bool _isallinputok = true;

            // If first name is not supplied
            if (firstname.Trim() == string.Empty)
            {
                Toast.MakeText(this, "Please provide first name", ToastLength.Long).Show();
                if (dialog != null)
                {
                    dialog.Dismiss();
                }
                return false;
            }

            // If last name is not supplied
            if (lastname.Trim() == string.Empty)
            {
                Toast.MakeText(this, "Please provide last name", ToastLength.Long).Show();
                if (dialog != null)
                {
                    dialog.Dismiss();
                }
                return false;
            }

            // If username is not supplied
            if (username.Trim() == string.Empty)
            {
                Toast.MakeText(this, "Please provide username", ToastLength.Long).Show();
                if (dialog != null)
                {
                    dialog.Dismiss();
                }
                return false;
            }

            // If email is not supplied
            if (email.Trim() == string.Empty)
            {
                Toast.MakeText(this, "Please provide email", ToastLength.Long).Show();
                if (dialog != null)
                {
                    dialog.Dismiss();
                }
                return false;
            }
            else
            {
                // If email supplied is not valid
                if (!isValidEmail(email.Trim()))
                {
                    Toast.MakeText(this, "Please give proper email", ToastLength.Long).Show();
                    if (dialog != null)
                    {
                        dialog.Dismiss();
                    }
                    return false;
                }
            }

            // If password is not supplied
            if (password.Trim() == string.Empty)
            {
                Toast.MakeText(this, "Please provide password", ToastLength.Long).Show();
                if (dialog != null)
                {
                    dialog.Dismiss();
                }
                return false;
            }

            // If confirm password is not supplied
            if (repassword.Trim() == string.Empty)
            {
                Toast.MakeText(this, "Please confirm your password", ToastLength.Long).Show();
                if (dialog != null)
                {
                    dialog.Dismiss();
                }
                return false;
            }
            else
            {
                // If Password & Confirm Password does not match
                if (password.Trim() != repassword.Trim())
                {
                    Toast.MakeText(this, "Password not match", ToastLength.Long).Show();
                    if (dialog != null)
                    {
                        dialog.Dismiss();
                    }
                    return false;
                }
            }

            return _isallinputok;
        }
    }


}

