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
using IMInventory.iminventory;
using System.Data;
using Android.Preferences;
using Android.Database;
using Android.Graphics;
using DK.Ostebaronen.FloatingActionButton;
using Android.Database.Sqlite;
using System.Globalization;
using Xamarin.InAppBilling.Utilities;
using Xamarin.InAppBilling;
using System.Threading.Tasks;
using Android.Net.Wifi;

namespace IMInventory
{
	[Activity (Label = "Main_Search_A", MainLauncher = false, ScreenOrientation=Android.Content.PM.ScreenOrientation.SensorPortrait)]			
	public class Main_Search_A : Activity
	{
		ListView listView;
		List<TableItem> tableItems;
		List<TableItem> filterdtableItems;
		ISharedPreferences prefs;
		ISharedPreferencesEditor editor;
		long InventoryType;
		private Fab _fab;
		long IsInternal;
		Dialog dialog;
		Dialog locationdialog;
		string projectname;
		string clientname;
		int inventorytype;
		long EmpID;
		ProgressDialog projectloadingdialog;
		SQLiteDatabase db = null;
		long projectid;
		long UserType;
		string UserPlan;
		InAppBillingServiceConnection _serviceConnection;
		IList<Product> _products;

		long freeproject = 0;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			Typeface tf = Typeface.CreateFromAsset (Assets, "Fonts/ROBOTO-LIGHT.TTF");
			this.RequestWindowFeature (WindowFeatures.NoTitle);
			SetContentView (Resource.Layout.Main_Search_A);
			StartSetup();
			_fab = FindViewById<Fab> (Resource.Id.btnOpenCreateProjectPopUp);
			_fab.FabColor = Color.ParseColor ("#3597d4");
			_fab.FabDrawable = Resources.GetDrawable (Resource.Drawable.CreateProject);
			_fab.Show ();
			prefs = PreferenceManager.GetDefaultSharedPreferences (this); 
			editor = prefs.Edit ();
			projectid = prefs.GetLong("ProjectID", 0);
			long empid = prefs.GetLong ("EmpID", 5);
			EmpID = prefs.GetLong ("EmpID", 0);
			UserType = prefs.GetLong("UserType", 1);
			UserPlan = prefs.GetString("UserPlan", "");
			IsInternal = prefs.GetLong ("IsInternal", 0);
			if (IsInternal == 1) {
				_fab.Visibility = ViewStates.Gone;
			} else {
				_fab.Visibility = ViewStates.Visible;
			}
			_fab.Click += delegate {
				if(UserType==1){
					OpenInAppPurchasePopUp();

				}
				else{
					showcreateprojectdialog ();
				
				}

			};
			WebService objime = new WebService ();
			projectloadingdialog = ProgressDialog.Show (this, "Loading", "Please wait.....");
			objime.ProjectListAsync ("", empid.ToString ());
			objime.ProjectListCompleted += getempxml; 
			//	listView=listView = FindViewById<ListView>(Resource.Id.List);
			if (IMApplication.player != null) {
				IMApplication.player.Stop ();
				IMApplication.player = null;
			}
			ImageView Cam1 = FindViewById<ImageView> (Resource.Id.imgSearchBack);
			ImageView imgDeleteVoiceSearch = FindViewById<ImageView> (Resource.Id.imgDeleteVoiceSearch);
			ImageView imgVoiceSearch = FindViewById<ImageView> (Resource.Id.imgVoiceSearch);
			EditText txtsearchproject = FindViewById<EditText> (Resource.Id.txtsearchproject);
			listView = FindViewById<ListView> (Resource.Id.List);
			listView.ItemClick += OnListItemClick;
			listView.ItemLongClick += DeleteProject;
			db = this.OpenOrCreateDatabase("ImInventory", FileCreationMode.Private, null);
			txtsearchproject.TextChanged += delegate {
				
				int projectidcolumn;
				int ProjectID;

				string text = txtsearchproject.Text;
				ICursor c1 = db.RawQuery("SELECT * FROM " + "tbl_Inventory WHERE EmpID = " + empid + " AND (ItemDescription like '%"+text+"%' OR BarCodeNumber like '%"+text+"%') GROUP BY ProjectName ORDER BY  ID  ", null);
				var projectitemsitems = filterdtableItems;
				List<TableItem> serceitemsbyprojectidlist=new List<TableItem>();
				if (c1.Count > 0)
				{
					projectidcolumn = c1.GetColumnIndex("ProjectID");
					c1.MoveToFirst();
					if (c1 != null)
					{
						// Loop through all Results
						do
						{

							ProjectID = c1.GetInt(projectidcolumn);
							List<TableItem> serceitemsbyprojectid=projectitemsitems.Where (x => x.ProjectID==Convert.ToString(ProjectID)).ToList();
							serceitemsbyprojectidlist=serceitemsbyprojectidlist.Concat(serceitemsbyprojectid).ToList();
						}
						while (c1.MoveToNext());
					}
				}

				if (text.Trim () != "") {
					imgVoiceSearch.Visibility = ViewStates.Gone;
					imgDeleteVoiceSearch.Visibility = ViewStates.Visible;
					List<TableItem> serceitemsbyprojectname= projectitemsitems.Where (x => x.Projectname.ToLower ().Contains (text.Trim().ToLower())).ToList();
					List<TableItem> serceitemsbyclientname=projectitemsitems.Where (x => x.ClientName.ToLower ().Contains (text.Trim().ToLower())).ToList();
					projectitemsitems=serceitemsbyprojectname.Concat(serceitemsbyclientname).ToList();
					projectitemsitems=projectitemsitems.Concat(serceitemsbyprojectidlist).ToList();
					projectitemsitems=projectitemsitems.OrderBy(x=>x.ProjectID).ToList();
					var projectitemsitem=projectitemsitems.GroupBy(x=> new {x.ProjectID}).OrderBy(g=>g.Key.ProjectID).ThenBy(g=>g.Key.ProjectID).ToList();
					projectitemsitems=new List<TableItem>();
					foreach(var group in projectitemsitem)
					{
						List<TableItem> cps = group.ToList();
						projectitemsitems=projectitemsitems.Concat(group).ToList();
					}
					tableItems = projectitemsitems;

				
				} else {
					imgVoiceSearch.Visibility = ViewStates.Visible;
					imgDeleteVoiceSearch.Visibility = ViewStates.Gone;
					projectitemsitems = filterdtableItems;

				}

				listView.Adapter = new ProjectScreenAdapter (this, projectitemsitems);

			};

			imgDeleteVoiceSearch.Click += delegate {
				listView = FindViewById<ListView> (Resource.Id.List);
				imgVoiceSearch.Visibility = ViewStates.Visible;
				imgDeleteVoiceSearch.Visibility = ViewStates.Gone;
				txtsearchproject.SetText ("", TextView.BufferType.Editable);
				//listView = FindViewById<ListView>(Resource.Id.List);
				listView.Adapter = new ProjectScreenAdapter (this, filterdtableItems);
				tableItems = filterdtableItems;
				//listView.ItemClick += OnListItemClick;
			};
			Cam1.Click += delegate {
				this.Finish ();
				//StartActivity (typeof(Main));
			};
		}




		private void getempxml (object sender, ProjectListCompletedEventArgs e)
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
			tableItems = new List<TableItem> ();
			filterdtableItems = new List<TableItem> ();

			if (dataTable.Rows.Count > 0) {
					if(UserType==1){

						if(dataTable.Rows.Count==1){
							foreach (DataRow dr in dataTable.Rows) {
								if(dr ["ProjectID"].ToString () != "0"){
									//_fab.Visibility=ViewStates.Gone;

								}
								else{
									_fab.Visibility=ViewStates.Visible;}
							
							}

						}
						else{
							_fab.Visibility=ViewStates.Gone;

						}

					}
						
				foreach (DataRow dr in dataTable.Rows) {
					if (dr ["ProjectID"].ToString () != "0") {
							string date=dr ["addeddate"].ToString ();
							DateTime dt=DateTime.Parse(date,CultureInfo.GetCultureInfo("en-gb"));
							string converteddate=Convert.ToDateTime(dt).ToString("MM-dd-yyyy HH:mm:ss");
						tableItems.Add (new TableItem () {
							Projectname = dr ["Projectname"].ToString (),
							addeddate =  converteddate,
							ClientName = dr ["clientname"].ToString (),
							ProjectID = dr ["ProjectID"].ToString ()
						});
					} else {
							freeproject = 1;
						tableItems.Add (new TableItem () {
							Projectname = "No project found",
							addeddate = "",
							ClientName = "",
							ProjectID = ""
						});
					}
						}
					
					
			} else {
					
					freeproject = 1;

				tableItems.Add (new TableItem () {
						Projectname = "No project found",
						addeddate = "",
						ClientName = "",
						ProjectID = ""
					});
			}
			filterdtableItems=tableItems;
			//listView = FindViewById<ListView>(Resource.Id.List); // get reference to the ListView in the layout
			// populate the listview with data
			listView.Adapter = new ProjectScreenAdapter(this, tableItems);
		     
			projectloadingdialog.Dismiss ();
			}
			catch (Exception ex){
				projectloadingdialog.Dismiss ();
				AlertDialog.Builder builder  = new AlertDialog.Builder (this);
				builder .SetCancelable (false);
				builder .SetMessage ("Please check your device internet. And try again");
				builder .SetNeutralButton("Ok", (object buldersender, DialogClickEventArgs clickevent) => {
					projectloadingdialog.Dismiss ();
					StartActivity (typeof(Main));

				});
				AlertDialog alertdialog = builder.Create ();
				alertdialog.Show ();
			}
			//listView.ItemClick += OnListItemClick;
			//listView.SetFooterDividersEnabled(false);
		}


		void DeleteProject (object sender, AdapterView.ItemLongClickEventArgs e)
		{
			var listView = sender as ListView;
			var t = tableItems [e.Position];
			if (t.ClientName.Trim () != "") {
				if (IsInternal == 0) {
					string ProjectID = t.ProjectID;
					AlertDialog.Builder builder = new AlertDialog.Builder (this);
					builder.SetMessage ("Are you sure want to delete " + t.Projectname.ToString () + " ?");
					builder.SetCancelable (false);
					builder.SetPositiveButton ("Yes", (object buildersender, DialogClickEventArgs eve) => {
						WebService objservice = new WebService ();
						objservice.DeleteProjectAsync (ProjectID);
						objservice.DeleteProjectCompleted += getdeleteprojectsuccessxml;
						db = this.OpenOrCreateDatabase ("ImInventory", FileCreationMode.Private, null);
						db.ExecSQL ("delete from " + "tbl_Inventory Where ProjectID = " + ProjectID);
						ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences (this);
						ISharedPreferencesEditor editor = prefs.Edit ();
						long	projectid = prefs.GetLong ("ProjectID", 0);
						if (projectid == Convert.ToInt64 (ProjectID)) {
							editor.Remove ("ProjectID");
							editor.Commit ();
						}
					});

					builder.SetNegativeButton ("No", (object buildersender, DialogClickEventArgs eve) => {
					
					});
					AlertDialog alertdialog = builder.Create ();
					alertdialog.Show ();
				}
			}
		}
		private void  getdeleteprojectsuccessxml(object sender,DeleteProjectCompletedEventArgs eve){
			try{
				if(eve.Result.InnerXml.Contains("success")){

					Toast.MakeText(this, "Project deleted successfully", ToastLength.Short).Show();
					this.Finish();
					StartActivity(typeof(Main_Search_A));
				}
				else{
					Toast.MakeText(this, "Something error happend. Pls try again later", ToastLength.Short).Show();

				}

			}
			catch{
				Toast.MakeText(this, "Something error happend. Pls try again later", ToastLength.Short).Show();
			}

		}
		void OnListItemClick (object sender, AdapterView.ItemClickEventArgs e)
		{
			var listView = sender as ListView;
			var t = tableItems [e.Position];

			if (t.ClientName.Trim()!="") {
				if (projectid != Convert.ToInt64 (t.ProjectID)) {
					editor.PutString("LocationName", "");
				}
				editor.PutLong ("ProjectID", Convert.ToInt64 (t.ProjectID));
				editor.PutString ("ProjectName", t.Projectname);
				editor.PutString ("ClientName", t.ClientName);
				editor.PutString("EditFromItemList", "");
				editor.PutString("EditFromItemListForBarcode", "");
				editor.PutInt("FromVoiceEdit", 0);
				editor.Commit ();    
				// applies changes synchronously on older APIs
				editor.Apply ();  
				InventoryType = prefs.GetLong ("InventoryType", 0);
				if (InventoryType > 0) {
					ICursor c1 = db.RawQuery("SELECT * FROM " + "tbl_Inventory WHERE ProjectID = " + t.ProjectID, null);
					try{
						if(c1.Count>0){
							AlertDialog.Builder builder  = new AlertDialog.Builder (this);
							builder .SetMessage ("Please select from the following options");
							builder .SetCancelable (false);
							builder .SetPositiveButton ("Location List", (object buildersender, DialogClickEventArgs ev) => {
								StartActivity (typeof(LocationList));
							});
							builder .SetNegativeButton ("Add Item", (object buildersender, DialogClickEventArgs ev) => {
								this.Finish ();
								StartActivity (typeof(EntryTab));
							});
							AlertDialog alertdialog = builder.Create ();
							alertdialog.Show ();
						}
						else{
							this.Finish ();
							StartActivity (typeof(EntryTab));
						}
					
					}
					catch{

						Toast.MakeText(this, "Something error happend. Pls try again later", ToastLength.Short).Show();
					}
				} else {

					ICursor c1 = db.RawQuery("SELECT * FROM " + "tbl_Inventory WHERE ProjectID = " + t.ProjectID, null);
					try{
					editor = prefs.Edit ();
					editor.PutLong ("InventoryType", 1);
					editor.Commit ();
						if(c1.Count>0){
						AlertDialog.Builder builder  = new AlertDialog.Builder (this);
						builder .SetMessage ("Please select from the following options");
						builder .SetCancelable (false);
						builder .SetPositiveButton ("Location List", (object buildersender, DialogClickEventArgs ev) => {
							StartActivity (typeof(LocationList));
						});
							builder .SetNegativeButton ("Add Item", (object buildersender, DialogClickEventArgs ev) => {
								this.Finish ();
							StartActivity (typeof(EntryTab));
							});
							AlertDialog alertdialog = builder.Create ();
							alertdialog.Show ();
						}
						else{
							this.Finish ();
							StartActivity (typeof(EntryTab));
						}
					
					}
					catch{

						Toast.MakeText(this, "Something error happend. Pls try again later", ToastLength.Short).Show();
					}
				}
			}
			//dialog.Dismiss();
			//Android.Widget.Toast.MakeText(this, t.Projectname, Android.Widget.ToastLength.Short).Show();
        }
		private void showcreateprojectdialog ()
		{

			dialog=new Dialog(this);
		    dialog.RequestWindowFeature((int)WindowFeatures.NoTitle);
		    dialog.SetContentView(Resource.Layout.CreateProject);
			ImageView btnCreateProject = (ImageView) dialog.FindViewById(Resource.Id.btnCreateProject);
			ImageView btnCancelCreateProject = (ImageView) dialog.FindViewById(Resource.Id.btnCancelCreateProject);

			Typeface tf=Typeface.CreateFromAsset(Assets,"Fonts/ROBOTO-LIGHT.TTF");
			EditText txtCreateProjectName = (EditText)dialog.FindViewById (Resource.Id.txtCreateProjectName);
			txtCreateProjectName.Typeface=tf;
			txtCreateProjectName.Invalidate();
			EditText txtCreateProjectClientName = (EditText)dialog.FindViewById (Resource.Id.txtCreateProjectClientName);
			txtCreateProjectClientName.Typeface=tf;
			txtCreateProjectClientName.Invalidate();
			btnCreateProject.Click+=delegate {
			CreateProject();	
			};
			btnCancelCreateProject.Click+=delegate {
				dialog.Dismiss();
			};
			dialog.Show();



				
					
		}
		private void CreateProject ()
		{

			EditText txtCreateProjectName = (EditText)dialog.FindViewById (Resource.Id.txtCreateProjectName);
			EditText txtCreateProjectClientName = (EditText)dialog.FindViewById (Resource.Id.txtCreateProjectClientName);
			WebService objime = new WebService ();
			projectname = txtCreateProjectName.Text;
			clientname = txtCreateProjectClientName.Text;

			if (projectname.Trim () == "") {
				Toast.MakeText (this, "Please give project name", ToastLength.Long).Show ();
			} else if (clientname.Trim () == "") {
				Toast.MakeText (this, "Please give client name", ToastLength.Long).Show ();
			} else {
			string dtnow=DateTime.Now.ToString("yyyy-MM-dd");
				objime.ExternalCreateProjectAsync(EmpID.ToString(),projectname,clientname,dtnow);
				objime.ExternalCreateProjectCompleted+=getexternalcreateprojectxml;
			}

		}
		private void getexternalcreateprojectxml (object sender, ExternalCreateProjectCompletedEventArgs e)
		{
			try {
				DataSet ds = new DataSet ();
				string innerxml = e.Result.InnerXml.ToString ();
				innerxml = "<ds><table1>" + innerxml + "</table1></ds>";
				DataTable dataTable = new DataTable ("table1");
				dataTable.Columns.Add ("ProjectID", typeof(string));
				ds.Tables.Add (dataTable);

				System.IO.StringReader xmlSR = new System.IO.StringReader (innerxml);
				ds.ReadXml (xmlSR, XmlReadMode.IgnoreSchema);
				dataTable = ds.Tables [0];
				if (dataTable.Rows.Count > 0) {
					int projectid = Convert.ToInt16 (dataTable.Rows [0] ["ProjectID"]);
					ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences (this);
					ISharedPreferencesEditor editor = prefs.Edit ();
					editor.PutLong ("ProjectID", projectid);
					editor.PutString ("ProjectName", projectname);
					editor.PutString ("ClientName", clientname);
					//editor.PutLong ("InventoryType", inventorytype);
					editor.Commit ();    
					// applies changes synchronously on older APIs
					editor.Apply ();
					dialog.Dismiss ();
					//StartActivity (typeof(EntryTab));
					//ISharedPreferencesEditor editor = prefs.Edit ();
					locationdialog = new Dialog (this);
					locationdialog.RequestWindowFeature ((int)WindowFeatures.NoTitle);
					locationdialog.SetContentView (Resource.Layout.CreateLocation);
					ImageView btnCreateLocation = (ImageView)locationdialog.FindViewById (Resource.Id.btnCreateLocation);
					ImageView btnCancelCreateLocation = (ImageView)locationdialog.FindViewById (Resource.Id.btnCancelCreateLocation);
					Typeface tf=Typeface.CreateFromAsset(Assets,"Fonts/ROBOTO-LIGHT.TTF");
					EditText txtPopUpLocationName = (EditText)locationdialog.FindViewById (Resource.Id.txtPopUpLocationName);
					txtPopUpLocationName.Typeface=tf;
					txtPopUpLocationName.Invalidate();
					btnCreateLocation.Click += delegate {
						editor.PutString ("LocationName", txtPopUpLocationName.Text);
						editor.Commit();
						editor.Apply ();
						this.Finish ();
						StartActivity(typeof(EntryTab));
					};
					btnCancelCreateLocation.Click+=delegate {
						this.Finish ();
						StartActivity(typeof(EntryTab));
					};
					//dialog.Dismiss ();
					locationdialog.SetCanceledOnTouchOutside(false);
					locationdialog.Show ();


				}
			} catch {


			}
		}

		#region
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

				_serviceConnection.BillingHandler.OnGetProductsError += (int responseCode, Bundle ownedItems) => {
					//Console.WriteLine("Error getting products");
					Toast.MakeText(this, "Error getting products", ToastLength.Long).Show();
				};

				_serviceConnection.BillingHandler.OnInvalidOwnedItemsBundleReturned += (Bundle ownedItems) => {
					//Console.WriteLine("Invalid owned items bundle returned");
					Toast.MakeText(this, "Invalid owned items bundle returned", ToastLength.Long).Show();
				};

				_serviceConnection.BillingHandler.OnProductPurchasedError += (int responseCode, string sku) => {
					//Console.WriteLine("Error purchasing item {0}",sku);
					Toast.MakeText(this, "Error purchasing item {0}"+sku, ToastLength.Long).Show();
				};

				_serviceConnection.BillingHandler.OnPurchaseConsumedError += (int responseCode, string token) => {
					//Console.WriteLine("Error consuming previous purchase");
					Toast.MakeText(this,"Error consuming previous purchase", ToastLength.Long).Show();
				};

				_serviceConnection.BillingHandler.InAppBillingProcesingError += (message) => {
					//Console.WriteLine("In app billing processing error {0}",message);

					Toast.MakeText(this,"In app billing processing error {0}"+message, ToastLength.Long).Show();

				};

				GetInventory();
				LoadPurchasedItems();
			};
			// Attempt to connect to the service
			_serviceConnection.Connect();
		}
		private async Task GetInventory()
		{

			_products = await _serviceConnection.BillingHandler.QueryInventoryAsync (new List<string> {
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

				_serviceConnection.BillingHandler.ConsumePurchase (purchases [0]);

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
					showcreateprojectdialog ();

				}
				else {
					
				}
			}
			catch { }
		}
		private void OpenInAppPurchasePopUp (int forfreespace = 1)
		{
			if (freeproject == 0) {

				WifiManager wifiManager = (WifiManager)GetSystemService (Context.WifiService);
				WifiInfo wInfo = wifiManager.ConnectionInfo;
				String MACAdress = Android.Provider.Settings.Secure.GetString (ContentResolver, Android.Provider.Settings.Secure.AndroidId);

				AlertDialog.Builder builder = new AlertDialog.Builder (this);
				builder.SetMessage ("To create a new project , you will need to pay $1.99 . Would you like to continue and purchase?");

				builder.SetCancelable (false);
				builder.SetPositiveButton ("Yes", (object sender, DialogClickEventArgs e) => {
					try {
						if (_products != null) {

							_serviceConnection.BillingHandler.BuyProduct (_products [3]);

						} else {

							Toast.MakeText (this, "Something error happend when retrieving product. Please try again later", ToastLength.Long).Show ();
							//StartSetup();
						}
					} catch (Exception ex) {
						Toast.MakeText (this, ex.ToString (), ToastLength.Long).Show ();

					}
				});
				builder.SetNegativeButton ("No", (object sender, DialogClickEventArgs e) => {

				});
				AlertDialog alertdialog = builder.Create ();
				alertdialog.Show ();

			}
			else {
				showcreateprojectdialog ();

			}
		}


		#endregion
	}

	public class TableItem
	{
		public string Projectname{ get; set; }
		public string addeddate{ get; set; }
		public string ClientName{ get; set; }
		public string ProjectID{ get; set; }
	}
}

