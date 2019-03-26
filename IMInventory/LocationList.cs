

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
using Android.Database.Sqlite;
using Android.Database;
using System.Xml;
using IMInventory.iminventory;
using System.IO;
using Java.IO;
using Android.Preferences;
using DK.Ostebaronen.FloatingActionButton;
using Android.Graphics;
using System.Threading;
using Android.Net;
using Android.Net.Wifi;
using Java.Net;
using Xamarin.InAppBilling;
using Xamarin.InAppBilling.Utilities;
using System.Threading.Tasks;
using System.Data;

namespace IMInventory
{
    [Activity(Label = "LocationList", ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorPortrait)]
    public class LocationList : Activity
    {
        ListView listView;
        List<LocationTableItem> tableItems;
        SQLiteDatabase db = null;
        long empid;
        long projectid;
        int WifiUpload;
        string clientname;
        int i = 0;
        ProgressDialog savelocationprogressdialog;
        //private Fab _fab;
        int dbdatacount = 0;
        int rowcount = 0;
        int voicemultiimagetotalcount = 0;
        int voicemultiimagecount = 0;
        int voicefiletotalcount = 0;
        int voicefilecount = 0;
        long FileLength = 0;

        string xml = "";
        string newfilename = "";
        bool uploadfrommanualandbarcode = false;
        bool uploadmultiimagefromvoice = false;
        bool uploadmultiaudiofromvoice = false;
        bool mergeaudiofromvoice = false;
        int counter = 0;
        string UserPlan = "";
        string UploadedSize = "";
        string AccessFindITExport = "";
        string AccessExport = "";
        string AccessBarCodeScan = "";

        long IsInternal = 0;
        //string UserPlan = "";
        private Fab _fab;
        private Fab _qrfab;
        string AudioFiles = "";
        private ProgressDialog qrcodedialog;
        ISharedPreferences prefs;
        IList<Product> _products;
        InAppBillingServiceConnection _serviceConnection;
        string producttopurchase = "";
        protected override void OnCreate(Bundle bundle)
        {

            prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            empid = prefs.GetLong("EmpID", 0);
            projectid = prefs.GetLong("ProjectID", 0);
            db = this.OpenOrCreateDatabase("ImInventory", FileCreationMode.Private, null);
            IMApplication.empid = empid;
            IMApplication.projectid = projectid;
            IMApplication.imdb = db;
            IMApplication.pref = prefs;
            //StartActivity (typeof(GoogleDriveWebview));
            StartSetup();
            prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            empid = prefs.GetLong("EmpID", 0);
            projectid = prefs.GetLong("ProjectID", 0);

            UserPlan = prefs.GetString("UserPlan", "");
            clientname = prefs.GetString("ClientName", "");
            IMApplication.clientname = clientname;
            UploadedSize = "max";//prefs.GetString("UploadedSize", "");//For new requirement
            IsInternal = prefs.GetLong("IsInternal", 0);
            WifiUpload = prefs.GetInt("WifiUpload", 0);
            AccessFindITExport = prefs.GetString("AccessFindITExport", "");
            AccessExport = prefs.GetString("AccessExport", "");//For new requirement
            AccessBarCodeScan = prefs.GetString("AccessBarCodeScan", "");
            base.OnCreate(bundle);
            this.RequestWindowFeature(WindowFeatures.NoTitle);
            newfilename = Guid.NewGuid() + ".mp3";
            // Create your application here
            SetContentView(Resource.Layout.MainInventoryList);
            if (IMApplication.player != null)
            {
                IMApplication.player.Stop();
                IMApplication.player = null;
            }
            ImageView imgLocationBack = FindViewById<ImageView>(Resource.Id.imgLocationBack);
            imgLocationBack.Click += delegate
            {
                try
                {

                    prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                    empid = prefs.GetLong("EmpID", 0);
                    projectid = prefs.GetLong("ProjectID", 0);

                    UserPlan = prefs.GetString("UserPlan", "");
                    UploadedSize = "max";//prefs.GetString("UploadedSize", "");//For new requirement
                    IsInternal = prefs.GetLong("IsInternal", 0);
                    WifiUpload = prefs.GetInt("WifiUpload", 0);
                    AccessFindITExport = prefs.GetString("AccessFindITExport", "");
                    AccessExport = prefs.GetString("AccessExport", "");//For new requirement
                    AccessBarCodeScan = prefs.GetString("AccessBarCodeScan", "");
                    StartActivity(typeof(EntryTab));
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                    StartActivity(typeof(LocationList));
                }
            };
            //ImageView _fab = FindViewById<ImageView>(Resource.Id.btn1);

            _fab = FindViewById<Fab>(Resource.Id.btn1);
            _fab.FabColor = Color.ParseColor("#5ad091");

            _fab.FabDrawable = Resources.GetDrawable(Resource.Drawable.uploadlocation);
            _fab.Show();

            _qrfab = FindViewById<Fab>(Resource.Id.btnqrgenerate);
            _qrfab.FabColor = Color.ParseColor("#5ad091");
            _qrfab.FabDrawable = Resources.GetDrawable(Resource.Drawable.qrcodegenarator);
            _qrfab.Show();
            _fab.Click += delegate
            {

                try
                {
                    if (AccessExport.Trim() == "1")
                    {
                        if (UploadedSize != "max" && UploadedSize != "")
                        {
                            if (Convert.ToInt64(UploadedSize) < 102400)
                            {
                                if (WifiUpload == 1)
                                {
                                    if (NetworkConnectedViaWiFi())
                                    {
                                        savelocationprogressdialog = new ProgressDialog(this);
                                        savelocationprogressdialog.SetCancelable(false);
                                        savelocationprogressdialog.SetProgressNumberFormat(null);
                                        savelocationprogressdialog.SetCanceledOnTouchOutside(false);
                                        savelocationprogressdialog.SetMessage("Uploading");
                                        savelocationprogressdialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
                                        savelocationprogressdialog.Progress = 0;
                                        #region For server upload
                                        savelocationprogressdialog.Show();
                                        uploadmanuallocationtoserver();

                                        #endregion

                                        //                              string xml=uploadmanuallocationtoserver();
                                        //IMApplication.xml=xml;
                                        //StartActivity (typeof(GoogleDriveWebview));
                                        // 
                                        prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                                        ISharedPreferencesEditor editor = prefs.Edit();
                                        // editor.Remove("EmpID");
                                        editor.Remove("ProjectID");
                                        editor.Remove("InventoryType");
                                        editor.Remove("ProjectName");
                                        editor.Remove("ClientName");
                                        editor.Remove("LocationName");
                                        editor.Commit();
                                    }
                                    else
                                    {
                                        AlertDialog.Builder builder = new AlertDialog.Builder(this);
                                        builder.SetMessage("Wifi is not available. would you like to continue this upload using the mobile data option?");
                                        builder.SetCancelable(false);
                                        builder.SetPositiveButton("Yes", (object buildersender, DialogClickEventArgs eve) =>
                                        {
                                            if (IsNetworkConnected())
                                            {
                                                savelocationprogressdialog = new ProgressDialog(this);
                                                savelocationprogressdialog.SetCancelable(false);
                                                savelocationprogressdialog.SetCanceledOnTouchOutside(false);
                                                savelocationprogressdialog.SetProgressNumberFormat(null);
                                                savelocationprogressdialog.SetMessage("Uploading");
                                                savelocationprogressdialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
                                                savelocationprogressdialog.Progress = 0;
                                                #region For server upload
                                                savelocationprogressdialog.Show();
                                                uploadmanuallocationtoserver();

                                                #endregion

                                                //                                       string xml=uploadmanuallocationtoserver();
                                                //IMApplication.xml=xml;
                                                //StartActivity (typeof(GoogleDriveWebview));


                                                //uploadmanuallocationtoserver();
                                                prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                                                ISharedPreferencesEditor editor = prefs.Edit();
                                                // editor.Remove("EmpID");
                                                editor.Remove("ProjectID");
                                                editor.Remove("InventoryType");
                                                editor.Remove("ProjectName");
                                                editor.Remove("ClientName");
                                                editor.Remove("LocationName");
                                                editor.Commit();
                                            }
                                            else
                                            {
                                                Toast.MakeText(this, "You are not connected to a network", ToastLength.Short).Show();
                                            }

                                        });
                                        builder.SetNegativeButton("No", (object buildersender, DialogClickEventArgs eve) =>
                                        {

                                        });
                                        AlertDialog alertdialog = builder.Create();
                                        alertdialog.Show();
                                        //Toast.MakeText(this, "You are not connected to wifi", ToastLength.Short).Show();
                                    }
                                }
                                else
                                {
                                    if (IsNetworkConnected())
                                    {
                                        savelocationprogressdialog = new ProgressDialog(this);
                                        savelocationprogressdialog.SetCancelable(false);
                                        savelocationprogressdialog.SetCanceledOnTouchOutside(false);
                                        savelocationprogressdialog.SetProgressNumberFormat(null);
                                        savelocationprogressdialog.SetMessage("Uploading");
                                        savelocationprogressdialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
                                        savelocationprogressdialog.Progress = 0;
                                        #region For server upload
                                        savelocationprogressdialog.Show();
                                        uploadmanuallocationtoserver();

                                        #endregion



                                        //                              string xml=uploadmanuallocationtoserver();
                                        //IMApplication.xml=xml;
                                        //StartActivity (typeof(GoogleDriveWebview));


                                        // uploadmanuallocationtoserver();
                                        prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                                        ISharedPreferencesEditor editor = prefs.Edit();
                                        // editor.Remove("EmpID");
                                        editor.Remove("ProjectID");
                                        editor.Remove("InventoryType");
                                        editor.Remove("ProjectName");
                                        editor.Remove("ClientName");
                                        editor.Remove("LocationName");
                                        editor.Commit();
                                    }
                                    else
                                    {
                                        Toast.MakeText(this, "You are not connected to a network", ToastLength.Short).Show();
                                    }
                                }
                            }
                            else
                            {
                                OpenInAppPurchasePopUp(2);
                                producttopurchase = "export";
                            }
                        }
                        else
                        {

                            if (WifiUpload == 1)
                            {
                                if (NetworkConnectedViaWiFi())
                                {
                                    savelocationprogressdialog = new ProgressDialog(this);
                                    savelocationprogressdialog.SetCancelable(false);
                                    savelocationprogressdialog.SetProgressNumberFormat(null);
                                    savelocationprogressdialog.SetCanceledOnTouchOutside(false);
                                    savelocationprogressdialog.SetMessage("Uploading");
                                    savelocationprogressdialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
                                    savelocationprogressdialog.Progress = 0;
                                    #region For server upload
                                    savelocationprogressdialog.Show();
                                    uploadmanuallocationtoserver();

                                    #endregion

                                    //                           string xml=uploadmanuallocationtoserver();
                                    //IMApplication.xml=xml;
                                    //StartActivity (typeof(GoogleDriveWebview));



                                    // uploadmanuallocationtoserver();
                                    prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                                    ISharedPreferencesEditor editor = prefs.Edit();
                                    // editor.Remove("EmpID");
                                    editor.Remove("ProjectID");
                                    editor.Remove("InventoryType");
                                    editor.Remove("ProjectName");
                                    editor.Remove("ClientName");
                                    editor.Remove("LocationName");
                                    editor.Commit();
                                }
                                else
                                {
                                    AlertDialog.Builder builder = new AlertDialog.Builder(this);
                                    builder.SetMessage("Wifi is not available. would you like to continue this upload using the mobile data option?");
                                    builder.SetCancelable(false);
                                    builder.SetPositiveButton("Yes", (object buildersender, DialogClickEventArgs eve) =>
                                    {
                                        if (IsNetworkConnected())
                                        {
                                            savelocationprogressdialog = new ProgressDialog(this);
                                            savelocationprogressdialog.SetCancelable(false);
                                            savelocationprogressdialog.SetCanceledOnTouchOutside(false);
                                            savelocationprogressdialog.SetProgressNumberFormat(null);
                                            savelocationprogressdialog.SetMessage("Uploading");
                                            savelocationprogressdialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
                                            savelocationprogressdialog.Progress = 0;
                                            #region For server upload
                                            savelocationprogressdialog.Show();
                                            uploadmanuallocationtoserver();

                                            #endregion

                                            //                                    string xml=uploadmanuallocationtoserver();
                                            //IMApplication.xml=xml;
                                            //StartActivity (typeof(GoogleDriveWebview));


                                            //uploadmanuallocationtoserver();
                                            prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                                            ISharedPreferencesEditor editor = prefs.Edit();
                                            // editor.Remove("EmpID");
                                            editor.Remove("ProjectID");
                                            editor.Remove("InventoryType");
                                            editor.Remove("ProjectName");
                                            editor.Remove("ClientName");
                                            editor.Remove("LocationName");
                                            editor.Commit();
                                        }
                                        else
                                        {
                                            Toast.MakeText(this, "You are not connected to a network", ToastLength.Short).Show();
                                        }

                                    });
                                    builder.SetNegativeButton("No", (object buildersender, DialogClickEventArgs eve) =>
                                    {

                                    });
                                    AlertDialog alertdialog = builder.Create();
                                    alertdialog.Show();
                                    //Toast.MakeText(this, "You are not connected to wifi", ToastLength.Short).Show();
                                }
                            }
                            else
                            {
                                if (IsNetworkConnected())
                                {
                                    savelocationprogressdialog = new ProgressDialog(this);
                                    savelocationprogressdialog.SetCancelable(false);
                                    savelocationprogressdialog.SetCanceledOnTouchOutside(false);
                                    savelocationprogressdialog.SetProgressNumberFormat(null);
                                    savelocationprogressdialog.SetMessage("Uploading");
                                    savelocationprogressdialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
                                    savelocationprogressdialog.Progress = 0;
                                    #region For server upload
                                    savelocationprogressdialog.Show();
                                    uploadmanuallocationtoserver();

                                    #endregion

                                    #region For google drive upload

                                    //                           string xml=uploadmanuallocationtoserver();
                                    //IMApplication.xml=xml;
                                    //StartActivity (typeof(GoogleDriveWebview));
                                    #endregion

                                    //uploadmanuallocationtoserver();
                                    prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                                    ISharedPreferencesEditor editor = prefs.Edit();
                                    // editor.Remove("EmpID");
                                    editor.Remove("ProjectID");
                                    editor.Remove("InventoryType");
                                    editor.Remove("ProjectName");
                                    editor.Remove("ClientName");
                                    editor.Remove("LocationName");
                                    editor.Commit();
                                }
                                else
                                {
                                    Toast.MakeText(this, "You are not connected to a network", ToastLength.Short).Show();
                                }
                            }

                        }
                    }
                    else
                    {
                        OpenInAppPurchasePopUp(2);
                        producttopurchase = "export";

                    }
                }
                catch (Exception ex)
                {
                    savelocationprogressdialog.Dismiss();
                    Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                    StartActivity(typeof(LocationList));
                }
            };

            _qrfab.Click += delegate
            {

                try
                {
                    if (AccessFindITExport.Trim() == "1")
                    {
                        if (IsNetworkConnected())
                        {
                            CreateLocationQROptionPopup();




                        }
                        else
                        {
                            Toast.MakeText(this, "You are not connected to a network", ToastLength.Short).Show();
                        }
                    }
                    else
                    {
                        OpenInAppPurchasePopUp();
                        producttopurchase = "findit";

                    }

                }
                catch
                {
                    savelocationprogressdialog.Dismiss();
                    Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                    StartActivity(typeof(LocationList));
                }

            };
            inventorylist();
            var sd = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).ToString() + "/ImInventory";
            var data = Android.OS.Environment.DataDirectory;
            String currentDBPath = "/data/" + "com.organizedtest.iminventory" + "/databases/" + "ImInventory";
            String backupDBPath = "ImInventory";
            var currentDB = new Java.IO.File(data, currentDBPath);
            var backupDB = new Java.IO.File(sd, backupDBPath);


            try
            {
                var source = new FileInputStream(currentDB).Channel;
                var destination = new FileOutputStream(backupDB).Channel;
                destination.TransferFrom(source, 0, source.Size());
                source.Close();
                destination.Close();
            }
            catch
            {

            }
        }
        private void inventorylist()
        {
            string path = CreateDirectoryForPictures();
            int locationnamecolumn;
            int addeddate;
            int inventorytype = 0;
            int inventoryidcolumn = 0;
            int Inventoryid = 0;
            tableItems = new List<LocationTableItem>();
            db = this.OpenOrCreateDatabase("ImInventory", FileCreationMode.Private, null);
            //For Manual Entry Location List

            ICursor c1 = db.RawQuery("SELECT * FROM " + "tbl_Inventory WHERE EmpID = " + empid + " AND ProjectID=" + projectid + " GROUP BY  Location, InventoryType ORDER BY  ID  ", null);
            if (c1.Count > 0)
            {
                locationnamecolumn = c1.GetColumnIndex("Location");
                addeddate = c1.GetColumnIndex("Addeddate");
                inventorytype = c1.GetColumnIndex("InventoryType");
                inventoryidcolumn = c1.GetColumnIndex("ID");
                c1.MoveToFirst();
                long Empid = empid;
                long Projectid = projectid;

                if (c1 != null)
                {
                    // Loop through all Results
                    do
                    {

                        String Locationname = c1.GetString(locationnamecolumn);
                        string Addeddate = c1.GetString(addeddate);
                        int InventoryType = c1.GetInt(inventorytype);
                        Inventoryid = c1.GetInt(inventoryidcolumn);
                        if (InventoryType == 1)
                        {
                            tableItems.Add(new LocationTableItem()
                            {
                                ID = Inventoryid.ToString(),
                                Locationname = Locationname,
                                Addeddate = Addeddate,
                                InventoryType = "Manual"
                            });
                        }
                        if (InventoryType == 3)
                        {
                            tableItems.Add(new LocationTableItem()
                            {
                                ID = Inventoryid.ToString(),
                                Locationname = Locationname,
                                Addeddate = Addeddate,
                                InventoryType = "Barcode"
                            });
                        }
                        if (InventoryType == 2)
                        {
                            tableItems.Add(new LocationTableItem()
                            {
                                ID = Inventoryid.ToString(),
                                Locationname = Locationname,
                                Addeddate = Addeddate,
                                InventoryType = "Voice"
                            });
                        }
                        listView = FindViewById<ListView>(Resource.Id.ListLocation); // get reference to the ListView in the layout
                        // populate the listview with data
                        listView.Adapter = new LocationScreenAdapter(this, tableItems);
                    } while (c1.MoveToNext());
                }
            }
            _fab = FindViewById<Fab>(Resource.Id.btn1);
            if (c1.Count == 0)
            {
                tableItems.Add(new LocationTableItem()
                {
                    ID = "",
                    Locationname = "No location found",
                    Addeddate = "",
                    InventoryType = ""
                });
                listView = FindViewById<ListView>(Resource.Id.ListLocation); // get reference to the ListView in the layout
                // populate the listview with data
                listView.Adapter = new LocationScreenAdapter(this, tableItems);
                View view = this.LayoutInflater.Inflate(Resource.Layout.SearchLocation, null);
                view.FindViewById<LinearLayout>(Resource.Id.imgEditLocation).Visibility = ViewStates.Gone;
                _fab.Visibility = ViewStates.Gone;
                _qrfab.Visibility = ViewStates.Gone;
            }
            else
            {
                _fab.Visibility = ViewStates.Visible;
                _qrfab.Visibility = ViewStates.Visible;
            }
            // ImageView imgeditloaction = listView.FindViewById<ImageView>(Resource.Id.imgEditLocation);
            //  imgeditloaction.Click += imgeditloaction_Click;
        }

        private void imgeditloaction_Click(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void uploadmanuallocationtoserver()
        {
            string currentlocationname = "";
            String Locationname = "";
            string path = CreateDirectoryForPictures();
            int i = 0;
            int projectcount = 0;
            db = this.OpenOrCreateDatabase("ImInventory", FileCreationMode.Private, null);
            ICursor c = db.RawQuery("SELECT * FROM " + "tbl_Inventory WHERE EmpID = " + empid + " AND ProjectID=" + projectid + " ORDER BY  ID  ", null);
            int projectidcolumn = c.GetColumnIndex("ProjectID");
            int projectnamecolumn = c.GetColumnIndex("ProjectName");
            int locationnamecolumn = c.GetColumnIndex("Location");
            int image1namecolumn = c.GetColumnIndex("Image1");
            int image2namecolumn = c.GetColumnIndex("Image2");
            int image3namecolumn = c.GetColumnIndex("Image3");
            int image4namecolumn = c.GetColumnIndex("Image4");
            int itemdescriptionnamecolumn = c.GetColumnIndex("ItemDescription");
            int brandnamecolumn = c.GetColumnIndex("Brand");
            int quantitynamecolumn = c.GetColumnIndex("Quantity");
            int modelnumbernamecolumn = c.GetColumnIndex("ModelNumber");
            int unitcostnamecolumn = c.GetColumnIndex("UnitCost");
            int notesnamecolumn = c.GetColumnIndex("Notes");
            int barcodenamecolumn = c.GetColumnIndex("BarCodeNumber");
            int audionamecolumn = c.GetColumnIndex("AudioFileName");
            int optionnamecolumn = c.GetColumnIndex("InventoryType");
            c.MoveToFirst();
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            empid = prefs.GetLong("EmpID", 0);
            long Empid = empid;
            String ProjectidFromSession = "";
            String ProjectIDFromDB = "";
            ;
            using (var sw = new System.IO.StringWriter())
            {
                //XmlWriter writer = XmlWriter.Create (sw);
                using (var writer = new XmlTextWriter(sw))
                {
                    writer.WriteStartDocument(true);

                    writer.WriteStartElement("details");
                    writer.WriteStartElement("empid");
                    writer.WriteString(Empid.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("projects");






                    if (c.Count > 0)
                    {
                        dbdatacount = c.Count;
                        #region CreateXML


                        if (c != null)
                        {
                            // Loop through all Results



                            do
                            {



                                ProjectIDFromDB = c.GetString(projectidcolumn);
                                Locationname = c.GetString(locationnamecolumn);
                                string projectname = c.GetString(projectnamecolumn);

                                String Image1name = c.GetString(image1namecolumn);
                                String Image2name = c.GetString(image2namecolumn);
                                String Image3name = c.GetString(image3namecolumn);
                                String Image4name = c.GetString(image4namecolumn);
                                String Itemdescription = c.GetString(itemdescriptionnamecolumn);
                                String Brand = c.GetString(brandnamecolumn);
                                String Quantity = c.GetString(quantitynamecolumn);
                                String ModelNumber = c.GetString(modelnumbernamecolumn);
                                String UnitCost = c.GetString(unitcostnamecolumn);
                                String Notes = c.GetString(notesnamecolumn);
                                String BarCodeNumber = c.GetString(barcodenamecolumn);
                                String AudioFileName = c.GetString(audionamecolumn);
                                String Option = c.GetString(optionnamecolumn);
                                FileInfo fileinfo = null;




                                if (currentlocationname == Locationname)
                                {


                                }
                                else
                                {
                                    if (i > 0)
                                    {
                                        writer.WriteEndElement();
                                    }
                                    i = 0;

                                }

                                if (ProjectIDFromDB.Trim() == ProjectidFromSession.Trim())
                                {


                                }
                                else
                                {
                                    if (projectcount > 0)
                                    {
                                        if (currentlocationname == Locationname)
                                        {
                                            writer.WriteEndElement();
                                            i = 0;
                                        }

                                        writer.WriteEndElement();
                                    }
                                    projectcount = 0;

                                }
                                if (projectcount == 0)
                                {
                                    writer.WriteStartElement("project");

                                    writer.WriteStartElement("projectid");
                                    writer.WriteString(ProjectIDFromDB.ToString());

                                    writer.WriteEndElement();
                                    ProjectidFromSession = ProjectIDFromDB;
                                }
                                //location

                                if (i == 0)
                                {
                                    writer.WriteStartElement("location");

                                    writer.WriteStartElement("locationname");
                                    newfilename = Locationname + "_" + projectname + ".mp3";
                                    writer.WriteString(Locationname);
                                    currentlocationname = Locationname;
                                    writer.WriteEndElement();
                                }

                                //locationitem
                                writer.WriteStartElement("locationitem");
                                //image
                                writer.WriteStartElement("images");
                                string images = Image1name;
                                string[] imagearr = images.Split('|');
                                //  writer.WriteStartElement("image");
                                //  writer.WriteString("");
                                //writer.WriteEndElement();
                                if (Option != "2")
                                {
                                    if (Image1name != "")
                                    {
                                        writer.WriteStartElement("image");
                                        writer.WriteString(Image1name);
                                        writer.WriteEndElement();
                                    }
                                    if (Image2name != "")
                                    {
                                        writer.WriteStartElement("image");
                                        writer.WriteString(Image2name);
                                        writer.WriteEndElement();
                                    }
                                    if (Image3name != "")
                                    {
                                        writer.WriteStartElement("image");
                                        writer.WriteString(Image3name);
                                        writer.WriteEndElement();
                                    }
                                    if (Image4name != "")
                                    {
                                        writer.WriteStartElement("image");
                                        writer.WriteString(Image4name);
                                        writer.WriteEndElement();
                                    }
                                }
                                else
                                {

                                    foreach (var item in imagearr)
                                    {
                                        if (item.Trim() != "")
                                        {
                                            writer.WriteStartElement("image");
                                            writer.WriteString(item.ToString());
                                            writer.WriteEndElement();
                                        }

                                    }

                                }
                                writer.WriteEndElement();
                                //image
                                //barcodenumber
                                writer.WriteStartElement("barcodenumber");
                                writer.WriteString(BarCodeNumber);
                                writer.WriteEndElement();
                                //barcodenumber
                                //description
                                writer.WriteStartElement("itemdescription");
                                writer.WriteString(Itemdescription);
                                writer.WriteEndElement();

                                //description

                                //Brand
                                writer.WriteStartElement("brand");
                                writer.WriteString(Brand);
                                writer.WriteEndElement();

                                //Brand

                                //quantity
                                writer.WriteStartElement("quantity");
                                writer.WriteString(Quantity);
                                writer.WriteEndElement();
                                //quantity

                                //modelnumber
                                writer.WriteStartElement("modelnumber");
                                writer.WriteString(ModelNumber);
                                writer.WriteEndElement();
                                //modelnumber

                                //unitcost
                                writer.WriteStartElement("unitcost");
                                writer.WriteString(UnitCost);
                                writer.WriteEndElement();
                                //unitcost

                                //notes
                                writer.WriteStartElement("notes");
                                writer.WriteString(Notes);
                                writer.WriteEndElement();
                                //notes
                                //audio

                                writer.WriteStartElement("audio");
                                if (AudioFileName != "")
                                {
                                    writer.WriteString(newfilename);

                                }
                                else
                                {
                                    writer.WriteString("");

                                }
                                writer.WriteEndElement();
                                //audio
                                //option
                                writer.WriteStartElement("option");
                                writer.WriteString(Option);
                                writer.WriteEndElement();
                                //option

                                writer.WriteEndElement();

                                //locationitem

                                //location
                                //getByteArrayFromImage (path+"/"+Image2name);
                                byte[] image1bytearray = null;
                                byte[] image2bytearray = null;
                                byte[] image3bytearray = null;
                                byte[] image4bytearray = null;


                                if (Option != "2")
                                {
                                    if (Image1name != "")
                                    {
                                        //FileInfo fi = new FileInfo(szFile);
                                        image1bytearray = getByteArrayFromImage(path + "/" + Image1name);
                                        fileinfo = new FileInfo(path + "/" + Image1name);
                                        //Get_Size_in_KB_MB_GB((ulong)fi.Length, 0);
                                        // FileLength = FileLength + fileinfo.Length;
                                    }
                                    if (Image2name != "")
                                    {

                                        image2bytearray = getByteArrayFromImage(path + "/" + Image2name);
                                        fileinfo = new FileInfo(path + "/" + Image2name);
                                        //Get_Size_in_KB_MB_GB((ulong)fi.Length, 0);
                                        // FileLength = FileLength + fileinfo.Length;
                                    }

                                    if (Image3name != "")
                                    {

                                        image3bytearray = getByteArrayFromImage(path + "/" + Image3name);
                                        fileinfo = new FileInfo(path + "/" + Image3name);
                                        //Get_Size_in_KB_MB_GB((ulong)fi.Length, 0);
                                        // FileLength = FileLength + fileinfo.Length;
                                    }

                                    if (Image4name != "")
                                    {

                                        image4bytearray = getByteArrayFromImage(path + "/" + Image4name);
                                        fileinfo = new FileInfo(path + "/" + Image4name);
                                        //Get_Size_in_KB_MB_GB((ulong)fi.Length, 0);
                                        //  FileLength = FileLength + fileinfo.Length;

                                    }
                                    WebService objime = new WebService();
                                    objime.SaveImageAsync(image1bytearray, Image1name, image2bytearray, Image2name, image3bytearray, Image3name, image4bytearray, Image4name);
                                    objime.SaveImageCompleted += getsaveimagesuccessxml;
                                    objime.UploadFileMultipleAsync(null, "", "");
                                    objime.UploadFileMultipleCompleted += getuploadfilesuccessxml;
                                    objime.SaveSingleImageAsync(null, "");
                                    objime.SaveSingleImageCompleted += getsavesingleimagesuccessxml;
                                    objime.MargeAudioAsync("");
                                    objime.MargeAudioCompleted += getmergeaudiosuccessxml;

                                }
                                else
                                {


                                    byte[] audiofilebytearray = null;
                                    string audiopath = CreateDirectoryForAudio();

                                    if (AudioFileName != "")
                                    {
                                        WebService objime = new WebService();
                                        int countaudiofile = 0;
                                        if (AudioFileName.Contains("|"))
                                        {
                                            string[] ArrAudioFileName = AudioFileName.Split('|');
                                            voicefiletotalcount = ArrAudioFileName.Length;

                                            foreach (var item in ArrAudioFileName)
                                            {
                                                audiofilebytearray = getByteArrayFromImage(audiopath + "/" + item.ToString());
                                                objime.UploadFileMultipleAsync(audiofilebytearray, countaudiofile + "_" + item.ToString(), newfilename);
                                                countaudiofile = countaudiofile + 1;

                                            }
                                            objime.UploadFileMultipleCompleted += getuploadfilesuccessxml;
                                            AudioFiles = AudioFiles + "|" + newfilename;
                                            //objime.MargeAudioAsync(newfilename);
                                            //objime.MargeAudioCompleted+=getmergeaudiosuccessxml;
                                        }
                                        else
                                        {
                                            voicefiletotalcount = 1;
                                            audiofilebytearray = getByteArrayFromImage(audiopath + "/" + AudioFileName);
                                            AudioFiles = AudioFiles + "|" + newfilename;
                                            objime.UploadFileMultipleAsync(audiofilebytearray, countaudiofile + "_" + AudioFileName, newfilename);
                                            objime.UploadFileMultipleCompleted += getuploadfilesuccessxml;

                                        }
                                    }
                                    else
                                    {
                                        WebService objservice1 = new WebService();
                                        objservice1.MargeAudioAsync("");
                                        objservice1.MargeAudioCompleted += getmergeaudiosuccessxml;

                                        objservice1.UploadFileMultipleAsync(null, "", "");
                                        objservice1.UploadFileMultipleCompleted += getuploadfilesuccessxml;


                                    }

                                    WebService objservice = new WebService();
                                    images = Image1name.TrimStart('|').TrimEnd('|');
                                    imagearr = images.Split('|');
                                    voicemultiimagetotalcount = imagearr.Length;
                                    foreach (var imageitem in imagearr)
                                    {
                                        byte[] imagebytearray = getByteArrayFromImage(path + "/" + imageitem);
                                        string imagename = imageitem.ToString();
                                        objservice.SaveSingleImageAsync(imagebytearray, imagename);
                                        if (imagename != "")
                                        {

                                            // image4bytearray = getByteArrayFromImage(path + "/" + imagename);
                                            fileinfo = new FileInfo(path + "/" + imagename);
                                            //Get_Size_in_KB_MB_GB((ulong)fi.Length, 0);
                                            // FileLength = FileLength + fileinfo.Length;

                                        }

                                    }
                                    objservice.SaveSingleImageCompleted += getsavesingleimagesuccessxml;
                                    objservice.SaveImageAsync(null, "", null, "", null, "", null, "");
                                    objservice.SaveImageCompleted += getsaveimagesuccessxml;
                                    if (voicefiletotalcount == 0)
                                    {
                                        objservice.MargeAudioAsync("");
                                        objservice.MargeAudioCompleted += getmergeaudiosuccessxml;

                                    }


                                }
                                i++;
                                projectcount++;
                            } while (c.MoveToNext());
                        }
                        #endregion
                    }







                    if (currentlocationname == Locationname)
                    {
                        if (c.Count > 0)
                        {
                            writer.WriteEndElement();
                        }
                    }
                    if (ProjectIDFromDB == ProjectidFromSession)
                    {
                        if (c.Count > 0)
                        {
                            writer.WriteEndElement();
                        }
                    }

                    writer.WriteEndElement();
                    writer.WriteEndElement();

                    xml = sw.ToString();


                    writer.Flush();

                }
            }
        }




        //private string uploadmanuallocationtoserver()
        //{
        //	string currentlocationname = "";
        //	String Locationname = "";
        //	string path = CreateDirectoryForPictures ();
        //	int i = 0;
        //	int projectcount = 0;
        //	db = this.OpenOrCreateDatabase ("ImInventory", FileCreationMode.Private, null);
        //	ICursor c = db.RawQuery ("SELECT * FROM " + "tbl_Inventory WHERE EmpID = " + empid + " AND ProjectID=" + projectid + " ORDER BY  ID  ", null);
        //	int projectidcolumn = c.GetColumnIndex ("ProjectID");
        //	int projectnamecolumn = c.GetColumnIndex ("ProjectName");
        //	int locationnamecolumn = c.GetColumnIndex ("Location");
        //	int image1namecolumn = c.GetColumnIndex ("Image1");
        //	int image2namecolumn = c.GetColumnIndex ("Image2");
        //	int image3namecolumn = c.GetColumnIndex ("Image3");
        //	int image4namecolumn = c.GetColumnIndex ("Image4");
        //	int itemdescriptionnamecolumn = c.GetColumnIndex ("ItemDescription");
        //	int brandnamecolumn = c.GetColumnIndex ("Brand");
        //	int quantitynamecolumn = c.GetColumnIndex ("Quantity");
        //	int modelnumbernamecolumn = c.GetColumnIndex ("ModelNumber");
        //	int unitcostnamecolumn = c.GetColumnIndex ("UnitCost");
        //	int notesnamecolumn = c.GetColumnIndex ("Notes");
        //	int barcodenamecolumn = c.GetColumnIndex ("BarCodeNumber");
        //	int audionamecolumn = c.GetColumnIndex ("AudioFileName");
        //	int optionnamecolumn = c.GetColumnIndex ("InventoryType");
        //	c.MoveToFirst ();
        //	ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences (this);
        //	empid = prefs.GetLong ("EmpID", 0);
        //	long Empid = empid;
        //	String ProjectidFromSession = "";
        //	String ProjectIDFromDB = "";
        //	;
        //	using (var sw = new System.IO.StringWriter ()) {
        //		//XmlWriter writer = XmlWriter.Create (sw);
        //		using (var writer = new XmlTextWriter(sw) ) {
        //		writer.WriteStartDocument (true);

        //		writer.WriteStartElement ("details");
        //		writer.WriteStartElement ("empid");
        //		writer.WriteString (Empid.ToString ());
        //		writer.WriteEndElement ();

        //		writer.WriteStartElement ("projects");






        //		if (c.Count > 0) {
        //			dbdatacount = c.Count;
        //			#region CreateXML


        //			if (c != null) {
        //				// Loop through all Results



        //				do {



        //					ProjectIDFromDB = c.GetString (projectidcolumn);
        //					Locationname = c.GetString (locationnamecolumn);
        //					string projectname = c.GetString (projectnamecolumn);

        //					String Image1name = c.GetString (image1namecolumn);
        //					String Image2name = c.GetString (image2namecolumn);
        //					String Image3name = c.GetString (image3namecolumn);
        //					String Image4name = c.GetString (image4namecolumn);
        //					String Itemdescription = c.GetString (itemdescriptionnamecolumn);
        //					String Brand = c.GetString (brandnamecolumn);
        //					String Quantity = c.GetString (quantitynamecolumn);
        //					String ModelNumber = c.GetString (modelnumbernamecolumn);
        //					String UnitCost = c.GetString (unitcostnamecolumn);
        //					String Notes = c.GetString (notesnamecolumn);
        //					String BarCodeNumber = c.GetString (barcodenamecolumn);
        //					String AudioFileName = c.GetString (audionamecolumn);
        //					String Option = c.GetString (optionnamecolumn);
        //					FileInfo fileinfo = null;




        //					if (currentlocationname == Locationname) {


        //					} else {
        //						if (i > 0) {
        //							writer.WriteEndElement ();
        //						}
        //						i = 0;

        //					}

        //					if (ProjectIDFromDB.Trim () == ProjectidFromSession.Trim ()) {


        //					} else {
        //						if (projectcount > 0) {
        //							if (currentlocationname == Locationname) {
        //								writer.WriteEndElement ();
        //								i = 0;
        //							}

        //							writer.WriteEndElement ();
        //						}
        //						projectcount = 0;

        //					}
        //					if (projectcount == 0) {
        //						writer.WriteStartElement ("project");

        //						writer.WriteStartElement ("projectid");
        //						writer.WriteString (ProjectIDFromDB.ToString ());

        //						writer.WriteEndElement ();
        //						ProjectidFromSession = ProjectIDFromDB;
        //					}
        //					//location

        //					if (i == 0) {
        //						writer.WriteStartElement ("location");

        //						writer.WriteStartElement ("locationname");
        //						newfilename = Locationname + "_" + projectname + ".mp3";
        //						writer.WriteString (Locationname);
        //						currentlocationname = Locationname;
        //						writer.WriteEndElement ();
        //					}

        //					//locationitem
        //					writer.WriteStartElement ("locationitem");
        //					//image
        //					writer.WriteStartElement ("images");
        //					string images = Image1name;
        //					string[] imagearr = images.Split ('|');
        //					//  writer.WriteStartElement("image");
        //					//  writer.WriteString("");
        //					//writer.WriteEndElement();
        //					if (Option != "2") {
        //						if (Image1name != "") {
        //							writer.WriteStartElement ("image");
        //							writer.WriteString (Image1name);
        //							writer.WriteEndElement ();
        //						}
        //						if (Image2name != "") {
        //							writer.WriteStartElement ("image");
        //							writer.WriteString (Image2name);
        //							writer.WriteEndElement ();
        //						}
        //						if (Image3name != "") {
        //							writer.WriteStartElement ("image");
        //							writer.WriteString (Image3name);
        //							writer.WriteEndElement ();
        //						}
        //						if (Image4name != "") {
        //							writer.WriteStartElement ("image");
        //							writer.WriteString (Image4name);
        //							writer.WriteEndElement ();
        //						}
        //					} else {

        //						foreach (var item in imagearr) {
        //							if (item.Trim () != "") {
        //								writer.WriteStartElement ("image");
        //								writer.WriteString (item.ToString ());
        //								writer.WriteEndElement ();
        //							}

        //						}

        //					}
        //					writer.WriteEndElement ();
        //					//image
        //					//barcodenumber
        //					writer.WriteStartElement ("barcodenumber");
        //					writer.WriteString (BarCodeNumber);
        //					writer.WriteEndElement ();
        //					//barcodenumber
        //					//description
        //					writer.WriteStartElement ("itemdescription");
        //					writer.WriteString (Itemdescription);
        //					writer.WriteEndElement ();

        //					//description

        //					//Brand
        //					writer.WriteStartElement ("brand");
        //					writer.WriteString (Brand);
        //					writer.WriteEndElement ();

        //					//Brand

        //					//quantity
        //					writer.WriteStartElement ("quantity");
        //					writer.WriteString (Quantity);
        //					writer.WriteEndElement ();
        //					//quantity

        //					//modelnumber
        //					writer.WriteStartElement ("modelnumber");
        //					writer.WriteString (ModelNumber);
        //					writer.WriteEndElement ();
        //					//modelnumber

        //					//unitcost
        //					writer.WriteStartElement ("unitcost");
        //					writer.WriteString (UnitCost);
        //					writer.WriteEndElement ();
        //					//unitcost

        //					//notes
        //					writer.WriteStartElement ("notes");
        //					writer.WriteString (Notes);
        //					writer.WriteEndElement ();
        //					//notes
        //					//audio

        //					writer.WriteStartElement ("audio");
        //					if (AudioFileName != "") {
        //						writer.WriteString (newfilename);

        //					} else {
        //						writer.WriteString ("");

        //					}
        //					writer.WriteEndElement ();
        //					//audio
        //					//option
        //					writer.WriteStartElement ("option");
        //					writer.WriteString (Option);
        //					writer.WriteEndElement ();
        //					//option

        //					writer.WriteEndElement ();

        //					//locationitem

        //					//location
        //					//getByteArrayFromImage (path+"/"+Image2name);
        //					byte[] image1bytearray = null;
        //					byte[] image2bytearray = null;
        //					byte[] image3bytearray = null;
        //					byte[] image4bytearray = null;


        //					if (Option != "2") {
        //						if (Image1name != "") {
        //							//FileInfo fi = new FileInfo(szFile);
        //							image1bytearray = getByteArrayFromImage (path + "/" + Image1name);
        //							fileinfo = new FileInfo (path + "/" + Image1name);
        //							//Get_Size_in_KB_MB_GB((ulong)fi.Length, 0);
        //							// FileLength = FileLength + fileinfo.Length;
        //						}
        //						if (Image2name != "") {

        //							image2bytearray = getByteArrayFromImage (path + "/" + Image2name);
        //							fileinfo = new FileInfo (path + "/" + Image2name);
        //							//Get_Size_in_KB_MB_GB((ulong)fi.Length, 0);
        //							// FileLength = FileLength + fileinfo.Length;
        //						}

        //						if (Image3name != "") {

        //							image3bytearray = getByteArrayFromImage (path + "/" + Image3name);
        //							fileinfo = new FileInfo (path + "/" + Image3name);
        //							//Get_Size_in_KB_MB_GB((ulong)fi.Length, 0);
        //							// FileLength = FileLength + fileinfo.Length;
        //						}

        //						if (Image4name != "") {

        //							image4bytearray = getByteArrayFromImage (path + "/" + Image4name);
        //							fileinfo = new FileInfo (path + "/" + Image4name);
        //							//Get_Size_in_KB_MB_GB((ulong)fi.Length, 0);
        //							//  FileLength = FileLength + fileinfo.Length;

        //						}
        //						//WebService objime = new WebService ();
        //						//objime.SaveImageAsync (image1bytearray, Image1name, image2bytearray, Image2name, image3bytearray, Image3name, image4bytearray, Image4name);
        //						//objime.SaveImageCompleted += getsaveimagesuccessxml;
        //						//objime.UploadFileMultipleAsync (null, "", "");
        //						//objime.UploadFileMultipleCompleted += getuploadfilesuccessxml;
        //						//objime.SaveSingleImageAsync (null, "");
        //						//objime.SaveSingleImageCompleted += getsavesingleimagesuccessxml;
        //						//objime.MargeAudioAsync ("");
        //						//objime.MargeAudioCompleted += getmergeaudiosuccessxml;

        //					} else {


        //						byte[] audiofilebytearray = null;
        //						string audiopath = CreateDirectoryForAudio ();

        //						if (AudioFileName != "") {
        //							WebService objime = new WebService ();
        //							int countaudiofile = 0;
        //							if (AudioFileName.Contains ("|")) {
        //								string[] ArrAudioFileName = AudioFileName.Split ('|');
        //								voicefiletotalcount = ArrAudioFileName.Length;

        //								foreach (var item in ArrAudioFileName) {
        //									audiofilebytearray = getByteArrayFromImage (audiopath + "/" + item.ToString ());
        //									//objime.UploadFileMultipleAsync (audiofilebytearray, countaudiofile + "_" + item.ToString (), newfilename);
        //									countaudiofile = countaudiofile + 1;

        //								}
        //								//objime.UploadFileMultipleCompleted += getuploadfilesuccessxml;
        //								AudioFiles = AudioFiles + "|" + newfilename;
        //								//objime.MargeAudioAsync(newfilename);
        //								//objime.MargeAudioCompleted+=getmergeaudiosuccessxml;
        //							} else {
        //								voicefiletotalcount = 1;
        //								audiofilebytearray = getByteArrayFromImage (audiopath + "/" + AudioFileName);
        //								AudioFiles = AudioFiles + "|" + newfilename;
        //								//objime.UploadFileMultipleAsync (audiofilebytearray, countaudiofile + "_" + AudioFileName, newfilename);
        //								//objime.UploadFileMultipleCompleted += getuploadfilesuccessxml;

        //							}
        //						} else {
        //							WebService objservice1 = new WebService ();
        //							//objservice1.MargeAudioAsync ("");
        //							//objservice1.MargeAudioCompleted += getmergeaudiosuccessxml;

        //							//objservice1.UploadFileMultipleAsync (null, "", "");
        //							//objservice1.UploadFileMultipleCompleted += getuploadfilesuccessxml;


        //						}

        //						WebService objservice = new WebService ();
        //						images = Image1name.TrimStart ('|').TrimEnd ('|');
        //						imagearr = images.Split ('|');
        //						voicemultiimagetotalcount = imagearr.Length;
        //						foreach (var imageitem in imagearr) {
        //							byte[] imagebytearray = getByteArrayFromImage (path + "/" + imageitem);
        //							string imagename = imageitem.ToString ();
        //							//objservice.SaveSingleImageAsync (imagebytearray, imagename);
        //							if (imagename != "") {

        //								// image4bytearray = getByteArrayFromImage(path + "/" + imagename);
        //								fileinfo = new FileInfo (path + "/" + imagename);
        //								//Get_Size_in_KB_MB_GB((ulong)fi.Length, 0);
        //								// FileLength = FileLength + fileinfo.Length;

        //							}

        //						}
        //						//objservice.SaveSingleImageCompleted += getsavesingleimagesuccessxml;
        //						//objservice.SaveImageAsync (null, "", null, "", null, "", null, "");
        //						//objservice.SaveImageCompleted += getsaveimagesuccessxml;
        //						if (voicefiletotalcount == 0) {
        //							//objservice.MargeAudioAsync ("");
        //							//objservice.MargeAudioCompleted += getmergeaudiosuccessxml;

        //						}


        //					}
        //					i++;
        //					projectcount++;
        //				} while (c.MoveToNext ());
        //			}
        //			#endregion
        //		}







        //		if (currentlocationname == Locationname) {
        //			if (c.Count > 0) {
        //				writer.WriteEndElement ();
        //			}
        //		}
        //		if (ProjectIDFromDB == ProjectidFromSession) {
        //			if (c.Count > 0) {
        //				writer.WriteEndElement ();
        //			}
        //		}

        //		writer.WriteEndElement ();
        //		writer.WriteEndElement ();

        //		xml = sw.ToString ();


        //		writer.Flush ();

        //		}
        //	}
        //	return xml;
        //      }


        private void UploadLocationQrCodeXMLToServer()
        {
            try
            {
                string path = CreateDirectoryForPictures();
                db = this.OpenOrCreateDatabase("ImInventory", FileCreationMode.Private, null);
                ICursor c = db.RawQuery("SELECT * FROM " + "tbl_Inventory WHERE EmpID = " + empid + " AND ProjectID=" + projectid + " GROUP BY InventoryType,Location ORDER BY  ID", null);
                int projectidcolumn = c.GetColumnIndex("ProjectID");
                int locationnamecolumn = c.GetColumnIndex("Location");
                int optionnamecolumn = c.GetColumnIndex("InventoryType");
                c.MoveToFirst();
                ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                empid = prefs.GetLong("EmpID", 0);
                long Empid = empid;
                // System.IO.StringWriter sw = new System.IO.StringWriter();
                // XmlWriter writer = XmlWriter.Create(sw);
                using (var sw = new System.IO.StringWriter())
                {
                    //XmlWriter writer = XmlWriter.Create (sw);
                    using (var writer = new XmlTextWriter(sw))
                    {

                        writer.WriteStartDocument(true);

                        writer.WriteStartElement("details");
                        writer.WriteStartElement("Emp");
                        writer.WriteString(Empid.ToString());
                        writer.WriteEndElement();
                        writer.WriteStartElement("ProjectID");
                        writer.WriteString(projectid.ToString());
                        writer.WriteEndElement();
                        writer.WriteStartElement("location");
                        if (c.Count > 0)
                        {

                            if (c != null)
                            {
                                // Loop through all Results


                                do
                                {
                                    string Locationname = c.GetString(locationnamecolumn);
                                    string InventoryType = c.GetString(optionnamecolumn);
                                    if (InventoryType != "2")
                                    {
                                        writer.WriteStartElement("data");
                                        writer.WriteString(Locationname + "|||" + InventoryType);
                                        writer.WriteEndElement();
                                    }
                                }
                                while (c.MoveToNext());
                            }

                        }
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                        xml = sw.ToString();
                        if (savelocationprogressdialog != null)
                        {
                            savelocationprogressdialog.Dismiss();
                        }
                        qrcodedialog = ProgressDialog.Show(this, "Generating QR Code", "Please wait.....");
                        WebService objService = new WebService();
                        //xml = xml.Replace(@"\","");
                        objService.SendQRCodeMailAsync(xml);
                        objService.SendQRCodeMailCompleted += getqrcodesuccessmessage;

                        writer.Flush();

                    }
                }

            }
            catch (Exception ex)
            {
            }

        }

        private void UploadLocationItemQrCodeXMLToServer()
        {
            try
            {
                string path = CreateDirectoryForPictures();
                db = this.OpenOrCreateDatabase("ImInventory", FileCreationMode.Private, null);
                ICursor c = db.RawQuery("SELECT * FROM " + "tbl_Inventory WHERE EmpID = " + empid + " AND ProjectID=" + projectid + " ORDER BY  ID  ", null);
                int projectidcolumn = c.GetColumnIndex("ProjectID");
                int locationnamecolumn = c.GetColumnIndex("Location");
                int optionnamecolumn = c.GetColumnIndex("InventoryType");
                int descriptioncolumn = c.GetColumnIndex("ItemDescription");
                int idcolumn = c.GetColumnIndex("ID");
                c.MoveToFirst();
                ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                empid = prefs.GetLong("EmpID", 0);
                long Empid = empid;
                // System.IO.StringWriter sw = new System.IO.StringWriter();
                // XmlWriter writer = XmlWriter.Create(sw);
                using (var sw = new System.IO.StringWriter())
                {
                    //XmlWriter writer = XmlWriter.Create (sw);
                    using (var writer = new XmlTextWriter(sw))
                    {

                        writer.WriteStartDocument(true);

                        writer.WriteStartElement("details");
                        writer.WriteStartElement("Emp");
                        writer.WriteString(Empid.ToString());
                        writer.WriteEndElement();
                        writer.WriteStartElement("ProjectID");
                        writer.WriteString(projectid.ToString());
                        writer.WriteEndElement();
                        writer.WriteStartElement("items");
                        if (c.Count > 0)
                        {

                            if (c != null)
                            {
                                // Loop through all Results


                                do
                                {
                                    string Locationname = c.GetString(locationnamecolumn);
                                    string InventoryType = c.GetString(optionnamecolumn);
                                    string description = c.GetString(descriptioncolumn);
                                    string id = c.GetString(idcolumn);
                                    if (InventoryType == "1")
                                    {
                                        writer.WriteStartElement("item");
                                        writer.WriteString(description + "|$|" + id);
                                        writer.WriteEndElement();
                                    }
                                }
                                while (c.MoveToNext());
                            }

                        }
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                        xml = sw.ToString();
                        if (savelocationprogressdialog != null)
                        {
                            savelocationprogressdialog.Dismiss();
                        }
                        qrcodedialog = ProgressDialog.Show(this, "Generating QR Code", "Please wait.....");
                        WebService objService = new WebService();
                        //xml = xml.Replace(@"\","");
                        objService.SendLocationQRCodeMailAsync(xml);
                        objService.SendLocationQRCodeMailCompleted += getqrcodelocationsuccessmessage;

                        writer.Flush();

                    }
                }

            }
            catch (Exception ex)
            {
            }

        }

        private void getqrcodesuccessmessage(object sender, SendQRCodeMailCompletedEventArgs e)
        {
            qrcodedialog.Dismiss();
            Toast.MakeText(this, "QR label forwarded to your registered email address", ToastLength.Long).Show();

        }

        private void getqrcodelocationsuccessmessage(object sender, SendLocationQRCodeMailCompletedEventArgs e)
        {
            qrcodedialog.Dismiss();
            Toast.MakeText(this, "QR label forwarded to your registered email address", ToastLength.Long).Show();

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

        private void getsavemanualsuccessxml(object sender, SaveManualCompletedEventArgs e)
        {

            try
            {
                if (e.Result.InnerXml.ToLower().Contains("success"))
                {

                    i++;
                    Dialog dialog = new Dialog(this);

                    dialog.RequestWindowFeature((int)WindowFeatures.NoTitle);
                    dialog.SetContentView(Resource.Layout.PopUpWindow);
                    TextView lblsignupsubhead = (TextView)dialog.FindViewById(Resource.Id.lblsignupsubhead);
                    TextView lblsignupsubtext = (TextView)dialog.FindViewById(Resource.Id.lblsignupsubtext);
                    ImageView dialogButton = (ImageView)dialog.FindViewById(Resource.Id.btnclosebusinessuse);
                    dialogButton.Click += delegate
                    {
                        try
                        {
                            WifiManager wifiManager = (WifiManager)GetSystemService(Context.WifiService);
                            WifiInfo wInfo = wifiManager.ConnectionInfo;
                            String MACAdress = Android.Provider.Settings.Secure.GetString(ContentResolver, Android.Provider.Settings.Secure.AndroidId);
                            String FileSizeInKB = "";
                            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                            ISharedPreferencesEditor editor = prefs.Edit();
                            if (UploadedSize != "")
                            {
                                try
                                {
                                    FileSizeInKB = Convert.ToString(Convert.ToInt64(UploadedSize) + FileLength);
                                }
                                catch
                                {
                                    FileSizeInKB = "";
                                }
                                editor.PutString("UploadedSize", FileSizeInKB);
                                editor.Commit();
                                // applies changes synchronously on older APIs
                                editor.Apply();
                            }
                            else
                            {

                                editor.PutString("UploadedSize", FileLength.ToString());
                                editor.Commit();
                                // applies changes synchronously on older APIs
                                editor.Apply();
                            }
                            //if((FileLength / 1024)+Convert.ToInt64(UploadedSize)>102400)
                            WebService objWebService = new WebService();
                            objWebService.FreeUserUploadSizeUpdateAsync(empid.ToString(), MACAdress, FileSizeInKB);
                            dialog.Dismiss();


                            StartActivity(typeof(Main));
                        }
                        catch (Exception ex)
                        {
                            //Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                        }
                    };



                    savelocationprogressdialog.Progress = 100;

                    lblsignupsubhead.Visibility = ViewStates.Gone;
                    lblsignupsubtext.Text = "Data Saved Successfully;";
                    savelocationprogressdialog.Dismiss();
                    dialog.SetCanceledOnTouchOutside(false);
                    dialog.Show();
                    //db.ExecSQL("delete from " + "tbl_Inventory Where EmpID = " + empid);
                }
                else
                {
                    Dialog dialog = new Dialog(this);

                    dialog.RequestWindowFeature((int)WindowFeatures.NoTitle);
                    dialog.SetContentView(Resource.Layout.PopUpWindow);
                    TextView lblsignupsubhead = (TextView)dialog.FindViewById(Resource.Id.lblsignupsubhead);
                    TextView lblsignupsubtext = (TextView)dialog.FindViewById(Resource.Id.lblsignupsubtext);
                    ImageView dialogButton = (ImageView)dialog.FindViewById(Resource.Id.btnclosebusinessuse);
                    dialogButton.Click += delegate
                    {
                        try
                        {
                            dialog.Dismiss();
                            StartActivity(typeof(Main));
                        }
                        catch (Exception ex)
                        {
                            Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                            StartActivity(typeof(LocationList));
                        }
                    };


                    //lblsignupsubhead.Text="Free User";
                    lblsignupsubhead.Visibility = ViewStates.Gone;
                    lblsignupsubtext.Text = "Error on data saving!!!";
                    savelocationprogressdialog.Dismiss();
                    dialog.SetCanceledOnTouchOutside(false);
                    dialog.Show();

                }
            }
            catch
            {
                savelocationprogressdialog.Dismiss();
                Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                StartActivity(typeof(LocationList));
            }

        }
        private void getmergeaudiosuccessxml(object sender, MargeAudioCompletedEventArgs e)
        {
            try
            {
                mergeaudiofromvoice = true;
                int progress = savelocationprogressdialog.Progress;

                if (progress < 80)
                {
                    savelocationprogressdialog.Progress = progress + 20;
                }
                UploadXmlToServer();

            }
            catch
            {
                savelocationprogressdialog.Dismiss();
                Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                StartActivity(typeof(LocationList));
            }
        }
        private void getsaveimagesuccessxml(object sender, SaveImageCompletedEventArgs e)
        {
            rowcount = rowcount + 1;
            try
            {
                DataSet ds = new DataSet();
                string innerxml = e.Result.InnerXml.ToString();
                innerxml = "<ds><table1>" + innerxml + "</table1></ds>";
                DataTable dataTable = new DataTable("table1");
                dataTable.Columns.Add("FileSize", typeof(string));
                ds.Tables.Add(dataTable);
                System.IO.StringReader xmlSR = new System.IO.StringReader(innerxml);

                ds.ReadXml(xmlSR, XmlReadMode.IgnoreSchema);

                dataTable = ds.Tables[0];
                long filesize = Convert.ToInt16(dataTable.Rows[0]["FileSize"]);
                FileLength = FileLength + filesize;
                if (dbdatacount == rowcount)
                {


                    uploadfrommanualandbarcode = true;
                    int progress = savelocationprogressdialog.Progress;
                    if (progress < 40)
                    {
                        savelocationprogressdialog.Progress = progress + 20;
                    }
                    UploadXmlToServer();
                }
            }
            catch
            {
                savelocationprogressdialog.Dismiss();
                Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                StartActivity(typeof(LocationList));
            }

        }
        private void getsavesingleimagesuccessxml(object sender, SaveSingleImageCompletedEventArgs e)
        {
            voicemultiimagecount = voicemultiimagecount + 1;
            try
            {
                DataSet ds = new DataSet();
                string innerxml = e.Result.InnerXml.ToString();
                innerxml = "<ds><table1>" + innerxml + "</table1></ds>";
                DataTable dataTable = new DataTable("table1");
                dataTable.Columns.Add("FileSize", typeof(string));
                ds.Tables.Add(dataTable);
                System.IO.StringReader xmlSR = new System.IO.StringReader(innerxml);

                ds.ReadXml(xmlSR, XmlReadMode.IgnoreSchema);

                dataTable = ds.Tables[0];
                long filesize = Convert.ToInt16(dataTable.Rows[0]["FileSize"]);
                FileLength = FileLength + filesize;
                if (voicemultiimagecount == voicemultiimagetotalcount)
                {


                    uploadmultiimagefromvoice = true;
                    int progress = savelocationprogressdialog.Progress;
                    if (progress < 40)
                    {
                        savelocationprogressdialog.Progress = progress + 20;
                    }
                    UploadXmlToServer();
                }
                if (voicemultiimagetotalcount == 0)
                {
                    int progress = savelocationprogressdialog.Progress;
                    if (progress < 60)
                    {
                        savelocationprogressdialog.Progress = progress + 20;
                    }
                    uploadmultiimagefromvoice = true;

                }
            }
            catch
            {
                savelocationprogressdialog.Dismiss();
                Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                StartActivity(typeof(LocationList));
            }

        }
        private void getuploadfilesuccessxml(object sender, UploadFileMultipleCompletedEventArgs e)
        {
            voicefilecount = voicefilecount + 1;
            try
            {
                if (voicefilecount == voicefiletotalcount)
                {
                    uploadmultiaudiofromvoice = true;
                    WebService objweb = new WebService();
                    string[] arrAudioFiles = AudioFiles.TrimStart('|').TrimEnd('|').Split('|');
                    foreach (var item in arrAudioFiles)
                    {
                        objweb.MargeAudioAsync(item.ToString());
                    }
                    //objweb.MargeAudioAsync(newfilename);
                    objweb.MargeAudioCompleted += getmergeaudiosuccessxml;
                    int progress = savelocationprogressdialog.Progress;
                    if (progress < 60)
                    {
                        savelocationprogressdialog.Progress = progress + 20;
                    }
                    UploadXmlToServer();
                }
                if (voicefiletotalcount == 0)
                {
                    uploadmultiaudiofromvoice = true;
                    int progress = savelocationprogressdialog.Progress;
                    if (progress < 60)
                    {
                        savelocationprogressdialog.Progress = progress + 20;
                    }

                }
            }

            catch
            {
                savelocationprogressdialog.Dismiss();
                Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                StartActivity(typeof(LocationList));

            }

        }
        private void UploadXmlToServer()
        {
            try
            {
                if (uploadfrommanualandbarcode && uploadmultiimagefromvoice && uploadmultiaudiofromvoice && mergeaudiofromvoice)
                {
                    if (counter == 0)
                    {
                        String FileSizeInKB = (FileLength).ToString();
                        UploadedSize = "max";
                        if (UploadedSize != "")
                        {
                            if (UploadedSize != "max")
                            {
                                if ((FileLength) + Convert.ToInt64(UploadedSize) > 102400)
                                {
                                    savelocationprogressdialog.Dismiss();
                                    OpenInAppPurchasePopUp(2);
                                    producttopurchase = "export";
                                }
                                else
                                {
                                    WebService objweb = new WebService();
                                    savelocationprogressdialog.Progress = 80;
                                    objweb.SaveManualAsync(xml);
                                    objweb.SaveManualCompleted += getsavemanualsuccessxml;
                                    //progressbar.Visibility = ViewStates.Gone;
                                    counter = counter + 1;
                                }
                            }
                            else
                            {
                                WebService objweb = new WebService();
                                savelocationprogressdialog.Progress = 80;
                                objweb.SaveManualAsync(xml);
                                objweb.SaveManualCompleted += getsavemanualsuccessxml;
                                //progressbar.Visibility = ViewStates.Gone;
                                counter = counter + 1;
                            }
                        }
                        else if ((FileLength) > 102400)
                        {
                            savelocationprogressdialog.Dismiss();
                            OpenInAppPurchasePopUp(2);
                            producttopurchase = "export";
                        }
                        else
                        {
                            WebService objweb = new WebService();
                            savelocationprogressdialog.Progress = 80;
                            objweb.SaveManualAsync(xml);
                            objweb.SaveManualCompleted += getsavemanualsuccessxml;
                            //progressbar.Visibility = ViewStates.Gone;
                            counter = counter + 1;
                        }


                    }
                }
            }
            catch
            {
                savelocationprogressdialog.Dismiss();
                Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                StartActivity(typeof(LocationList));
            }

        }
        private byte[] getByteArrayFromImage(String filePath)
        {

            byte[] bytes = null;
            try
            {
                Java.IO.File file = new Java.IO.File(filePath);
                FileInputStream fis = new FileInputStream(file);
                ByteArrayOutputStream bos = new ByteArrayOutputStream();
                byte[] buf = new byte[1024];

                for (int readNum; (readNum = fis.Read(buf)) != -1;)
                {
                    bos.Write(buf, 0, readNum);
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
            String path = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).ToString() + "/ImInventory";
            if (!Directory.Exists(path))
            {

                Directory.CreateDirectory(path);

            }
            return path;
        }
        private Boolean NetworkConnectedViaWiFi()
        {
            var connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            var activeConnection = connectivityManager.ActiveNetworkInfo;

            if ((activeConnection != null) && activeConnection.IsConnected)
            {
                if (activeConnection.TypeName.ToLower() == "wifi")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private Boolean IsNetworkConnected()
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
        public override void OnBackPressed()
        {

        }

        String[] sizeArry = new String[] { "Byes", "KB", "MB", "GB" };
        String Get_Size_in_KB_MB_GB(ulong sizebytes, int index)
        {

            if (sizebytes < 1000) return sizebytes + sizeArry[index];

            else return Get_Size_in_KB_MB_GB(sizebytes / 1024, ++index);

        }

        private void OpenInAppPurchasePopUp(int forfreespace = 1)
        {

            WifiManager wifiManager = (WifiManager)GetSystemService(Context.WifiService);
            WifiInfo wInfo = wifiManager.ConnectionInfo;
            String MACAdress = Android.Provider.Settings.Secure.GetString(ContentResolver, Android.Provider.Settings.Secure.AndroidId);

            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            if (forfreespace == 2)
            {
                builder.SetMessage("To export your files from this device to Google Drive, you will need to pay a one-time in-app purchase of $1.99 USD. Would you like to continue and purchase?");
            }
            else
            {
                //builder.SetMessage("To access this feature you will need to make a purchase of $1.99 for the lifetime use on this device. Would you like to continue and purchase this add-on?");
                builder.SetMessage("To access the print labels feature you will need to make a purchase of $3.99 for the life time use on this project in this device. Would you like to continue and purchase this add-on?");
            }
            builder.SetCancelable(false);
            builder.SetPositiveButton("Yes", (object sender, DialogClickEventArgs e) =>
            {
                try
                {
                    if (_products != null)
                    {
                        if (producttopurchase == "export")
                        {
                            _serviceConnection.BillingHandler.BuyProduct(_products[2]);
                        }
                        else
                        {

                            _serviceConnection.BillingHandler.BuyProduct(_products[1]);
                        }
                    }
                    else
                    {

                        Toast.MakeText(this, "Something error happend when retrieving product. Please try again later", ToastLength.Long).Show();
                        //StartSetup();
                    }
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, ex.ToString(), ToastLength.Long).Show();

                }
            });
            builder.SetNegativeButton("No", (object sender, DialogClickEventArgs e) =>
            {

            });
            AlertDialog alertdialog = builder.Create();
            alertdialog.Show();

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
                string AccessBarCodeScan = Convert.ToString(dataTable.Rows[0]["AccessBarCodeScan"]);
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
                    StartActivity(typeof(LocationList));

                }
            }
            catch (Exception ex)
            {

                Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
                StartActivity(typeof(LocationList));
            }
        }
        #region InAppPurchase


        private void CreateInAppPurchasePopUp()
        {

            // StartSetup();

        }
        public void StartSetup()
        {



            string value = Security.Unify(
                new string[] { "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAvC1rCfTD2nVuEeLcxv7EfyjQZuX6igvBw1X/V2" ,
                    "kJ+zx4HuiBIk6V6FIpFifIFcyfkvrxzzPat8yWg8cUGU5HEfh+rhypv+zeTZsSbPDDBB/pgAv47UtPFXk2FFnrVZoxML5050Lu" ,
                    "Tn+nTJ++n5OBoubvM8wtkyNjKuuXyWTRW2yyslKPpIPAS7Q9AHUmSPZFHf0NybR5EGADdoqxnh4qr7sU5kS94J7y5K3HdtYHOidU59" ,
                    "m7bmvMyjUN7uwXgjBWRi1HywJR/yZeyZDekDASGuJCSKlbjjItDfR0K3BcoH4ti4SHsD+T2v" ,
                    "26cEoqGHGiICllySrqwhMUfgdm58b57QIDAQAB" },
                new int[] { 0, 1, 2, 3, 4 });

            _serviceConnection = new InAppBillingServiceConnection(this, value);
            _serviceConnection.OnConnected += () =>
            {

                _serviceConnection.BillingHandler.OnGetProductsError += (int responseCode, Bundle ownedItems) =>
                {
                    //Console.WriteLine("Error getting products");
                    Toast.MakeText(this, "Error getting products", ToastLength.Long).Show();
                };

                _serviceConnection.BillingHandler.OnInvalidOwnedItemsBundleReturned += (Bundle ownedItems) =>
                {
                    //Console.WriteLine("Invalid owned items bundle returned");
                    Toast.MakeText(this, "Invalid owned items bundle returned", ToastLength.Long).Show();
                };

                _serviceConnection.BillingHandler.OnProductPurchasedError += (int responseCode, string sku) =>
                {
                    //Console.WriteLine("Error purchasing item {0}",sku);
                    Toast.MakeText(this, "Error purchasing item {0}" + sku, ToastLength.Long).Show();
                };

                _serviceConnection.BillingHandler.OnPurchaseConsumedError += (int responseCode, string token) =>
                {
                    //Console.WriteLine("Error consuming previous purchase");
                    Toast.MakeText(this, "Error consuming previous purchase", ToastLength.Long).Show();
                };

                _serviceConnection.BillingHandler.InAppBillingProcesingError += (message) =>
                {
                    //Console.WriteLine("In app billing processing error {0}",message);

                    Toast.MakeText(this, "In app billing processing error {0}" + message, ToastLength.Long).Show();

                };

                GetInventory();
                LoadPurchasedItems();
            };
            // Attempt to connect to the service
            _serviceConnection.Connect();
        }
        private async Task GetInventory()
        {

            _products = await _serviceConnection.BillingHandler.QueryInventoryAsync(new List<string> {
                "unlock_export",
                "find_it",
                "googledrive_upload",
                "newproject_add",
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
        {
            // Ask the open connection's billing handler to get any purchases
            var purchases = _serviceConnection.BillingHandler.GetPurchases(ItemType.Product);
            if (producttopurchase == "export")
            {
                _serviceConnection.BillingHandler.ConsumePurchase(purchases[0]);
            }
            else
            {
                _serviceConnection.BillingHandler.ConsumePurchase(purchases[1]);

            }
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
                    if (producttopurchase == "export")
                    {
                        WebService objWebService = new WebService();
                        objWebService.ExternalSignUpUserSubscriptionAsync(empid.ToString(), MACAdress, AccessFindITExport, AccessBarCodeScan, "1");
                        // objWebService.GetEmpDetailsByEmpIDAsync(empid.ToString(), MACAdress);
                        //objWebService.GetEmpDetailsByEmpIDCompleted += getempxml;
                        objWebService.ExternalSignUpUserSubscriptionCompleted += getExternalSignUpUserSubscriptionxml;
                        FileLength = 0;
                        #region For server upload
                        savelocationprogressdialog.Show();
                        uploadmanuallocationtoserver();

                        #endregion

                        //                  string xml=uploadmanuallocationtoserver();
                        //IMApplication.xml=xml;
                        //StartActivity (typeof(GoogleDriveWebview));


                        // long sizetoupload=-102400+Convert.ToInt64(UploadedSize);
                        // objWebService.FreeUserUploadSizeUpdateAsync(empid.ToString(), MACAdress, sizetoupload.ToString());
                        // UploadedSize = sizetoupload.ToString();
                    }
                    if (producttopurchase == "findit")
                    {
                        WebService objWebService = new WebService();
                        objWebService.ExternalSignUpUserSubscriptionAsync(empid.ToString(), MACAdress, "1", AccessBarCodeScan, AccessExport);
                        objWebService.ExternalSignUpUserSubscriptionCompleted += getExternalSignUpUserSubscriptionxml;

                    }

                }
                else
                {
                    if (producttopurchase == "export")
                    {
                        WebService objWebService = new WebService();
                        objWebService.ExternalSignUpUserSubscriptionAsync(empid.ToString(), MACAdress, AccessFindITExport, AccessBarCodeScan, "0");
                        // objWebService.GetEmpDetailsByEmpIDAsync(empid.ToString(), MACAdress);
                        //objWebService.GetEmpDetailsByEmpIDCompleted += getempxml;
                        objWebService.ExternalSignUpUserSubscriptionCompleted += getExternalSignUpUserSubscriptionxml;
                    }
                    if (producttopurchase == "findit")
                    {
                        WebService objWebService = new WebService();
                        objWebService.ExternalSignUpUserSubscriptionAsync(empid.ToString(), MACAdress, "0", AccessBarCodeScan, AccessExport);
                        objWebService.ExternalSignUpUserSubscriptionCompleted += getExternalSignUpUserSubscriptionxml;

                    }
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
        #endregion


        #region
        int selectedoption = 0;
        private void CreateLocationQROptionPopup()
        {

            string[] arrqroption = new string[2]; ;



            arrqroption[0] = "Print Location Labels";
            arrqroption[1] = "Print Inventory Items (Stock)";

            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle("Choose option");
            builder.SetSingleChoiceItems(arrqroption, 0, ListClicked);
            builder.SetNeutralButton("Ok", (object sender, DialogClickEventArgs e) =>
            {
                savelocationprogressdialog = new ProgressDialog(this);
                savelocationprogressdialog.SetCancelable(false);
                savelocationprogressdialog.SetCanceledOnTouchOutside(false);
                savelocationprogressdialog.SetProgressNumberFormat(null);
                savelocationprogressdialog.SetMessage("Sending");

                savelocationprogressdialog.Show();
                if (selectedoption == 0)
                {
                    UploadLocationQrCodeXMLToServer();


                }
                else
                {

                    UploadLocationItemQrCodeXMLToServer();


                }
                selectedoption = 0;

            });

            builder.SetCancelable(false);
            builder.Show();


        }
        private void ListClicked(object sender, DialogClickEventArgs args)
        {
            try
            {


                selectedoption = args.Which;

            }
            catch (Exception ex)
            {
            }
        }
        #endregion
    }

    public class LocationTableItem
    {
        public string ID { get; set; }
        public string Locationname { get; set; }
        public string InventoryType { get; set; }
        public string Addeddate { get; set; }

    }
}

