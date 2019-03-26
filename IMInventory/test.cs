


using Android.Support.V4.View;
using Android.Support.V4.App;

using Android.Widget;
using Android.App;
using Android.OS;
using Android.Content;
using Android.Graphics;
using Android.Database.Sqlite;
using Android.Database;
using System;
using Android.Views;
using System.IO;
using System.Net;
using System.Collections.Specialized;
using System.Security.Principal;
using CommonFunctions;


using System.Data;
using Android.Util;
using Android.Support.V4.Widget;





namespace IMInventory
{
	[Activity (Label = "test",MainLauncher=false)]			
	public class test :Activity
	{
		private ImageView Cam1;
		private ImageView Cam2;
		private ImageView Cam3;
		private ImageView Cam4;
		private string capturedimagename;
		SQLiteDatabase db=null;
		String Data="";
		private DrawerLayout mDrawerLayout;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.test);

			//TabHost tabHost=(TabHost)FindViewById(Resource.Id.tabHost);
			//tabHost.Setup ();

			//Android.Widget.TabHost.TabSpec spec1=tabHost.NewTabSpec("Manual");
			//spec1.SetContent (Resource.Id.tab1);
			//spec1.SetIndicator ("Manual");


			//Android.Widget.TabHost.TabSpec spec2=tabHost.NewTabSpec("Dictation");
			//spec2.SetContent (Resource.Id.tab2);
			//spec2.SetIndicator ("Dictation");

			//Android.Widget.TabHost.TabSpec spec3=tabHost.NewTabSpec("Barcode");
			//spec3.SetContent (Resource.Id.tab3);
			//spec3.SetIndicator ("Barcode");


			//tabHost.AddTab (spec1);
			//tabHost.AddTab (spec2);
			//tabHost.AddTab (spec3);
			Cam1 = FindViewById<ImageView> (Resource.Id.cam1);
			Cam1.Click += delegate {
				Intent intent = new Intent (Android.Provider.MediaStore.ActionImageCapture);
				string path=CreateDirectoryForPictures();
				string picname=String.Format("iminventory_{0}.jpg", Guid.NewGuid());
				Java.IO.File objFile=new Java.IO.File(path,picname);
				intent.PutExtra(Android.Provider.MediaStore.ExtraOutput, Android.Net.Uri.FromFile(objFile));
				capturedimagename=picname;
				StartActivityForResult (intent,1);

			};
			Cam2 = FindViewById<ImageView> (Resource.Id.cam2);
			Cam2.Click += delegate {
				Intent intent = new Intent (Android.Provider.MediaStore.ActionImageCapture);
				string path=CreateDirectoryForPictures();
				string picname=String.Format("iminventory_{0}.jpg", Guid.NewGuid());
				Java.IO.File objFile=new Java.IO.File(path,picname);
				intent.PutExtra(Android.Provider.MediaStore.ExtraOutput, Android.Net.Uri.FromFile(objFile));
				capturedimagename=picname;
				StartActivityForResult (intent, 2);

			};

			Cam3 = FindViewById<ImageView> (Resource.Id.cam3);
			Cam3.Click += delegate {
				Intent intent = new Intent (Android.Provider.MediaStore.ActionImageCapture);
				string path=CreateDirectoryForPictures();
				string picname=String.Format("iminventory_{0}.jpg", Guid.NewGuid());
				Java.IO.File objFile=new Java.IO.File(path,picname);
				intent.PutExtra(Android.Provider.MediaStore.ExtraOutput, Android.Net.Uri.FromFile(objFile));
				capturedimagename=picname;
				StartActivityForResult (intent, 3);

			};

			Cam4 = FindViewById<ImageView> (Resource.Id.cam4);
			Cam4.Click += delegate {
				Intent intent = new Intent (Android.Provider.MediaStore.ActionImageCapture);
				string path=CreateDirectoryForPictures();
				string picname=String.Format("iminventory_{0}.jpg", Guid.NewGuid());
				Java.IO.File objFile=new Java.IO.File(path,picname);
				intent.PutExtra(Android.Provider.MediaStore.ExtraOutput, Android.Net.Uri.FromFile(objFile));
				capturedimagename=picname;
				StartActivityForResult (intent, 4);

			};



			
			ImageView btnSave = FindViewById<ImageView> (Resource.Id.btnSave);

			btnSave.Click += delegate {
				
				TextView	Cam1Path = FindViewById<TextView> (Resource.Id.cam1path);
				string path=CreateDirectoryForPictures();
				string cam1imagepath=path+"/"+ Cam1Path.Text;

				//SaveDataToLocalDataBase();



			

			};
			// Create your application here
		}
		public override void OnBackPressed ()
		{
			createDialog();
		}

		public void createDialog(){

			AlertDialog.Builder builder  = new AlertDialog.Builder (this);
			builder .SetMessage ("Are you sure want to exit from Im Inventory?");
			builder .SetCancelable (false);
			//builder .SetIcon (Resource.Drawable.Icon);
			builder .SetPositiveButton ("Yes", (object sender, DialogClickEventArgs e) => {
				this.Finish();
			});
			builder .SetNegativeButton("No", (object sender, DialogClickEventArgs e) => {

			});



			AlertDialog alertdialog = builder.Create ();
			alertdialog.Show ();
		}
		private void SaveDataToLocalDataBase(){
			//EditText txtprojectname = FindViewById<EditText> (Resource.Id.txtProjectname);

			//EditText txtclientname = FindViewById<EditText> (Resource.Id.txtClientName);
			//txtclientname.Text="DVE";
			//txtprojectname.Enabled = false;
			//EditText txtlocation = FindViewById<EditText> (Resource.Id.txtLocation);
			ImageView img1 = FindViewById<ImageView> (Resource.Id.cam1);
			ImageView img2 = FindViewById<ImageView> (Resource.Id.cam2);
			ImageView img3 = FindViewById<ImageView> (Resource.Id.cam3);
			ImageView img4 = FindViewById<ImageView> (Resource.Id.cam4);
			EditText txtItemDescription = FindViewById<EditText> (Resource.Id.txtItemDescription);
			EditText txtbrand = FindViewById<EditText> (Resource.Id.txtBrand);
			EditText txtquantity = FindViewById<EditText> (Resource.Id.txtQuantity);
			EditText txtModelNumber = FindViewById<EditText> (Resource.Id.txtModelNumber);
			EditText txtUnitCost = FindViewById<EditText> (Resource.Id.txtUnitCost);
			EditText txtNotes = FindViewById<EditText> (Resource.Id.txtNotes);

			//string projectname = txtprojectname.Text;
			//string clientname = txtclientname.Text;
			//string location = txtlocation.Text;
			//string image1 = "";
			//string image2 = "";
			//string image3 = "";
			//string image4 = "";
			//string description = txtItemDescription.Text;
			string brand = txtbrand.Text;
			string quantity = txtquantity.Text;
			string model = txtModelNumber.Text;
			string unitcost = txtUnitCost.Text;
			string notes = txtNotes.Text;
			//string img = img1.Background;
			//this.DeleteDatabase ("ImInventory");
			db = this.OpenOrCreateDatabase ("ImInventory", FileCreationMode.Private, null);

			db.ExecSQL("CREATE TABLE IF NOT EXISTS "
				+ "ManualEntry"
				+ " (ProjectName VARCHAR, ClientName VARCHAR,Location VARCHAR, Image1 VARCHAR , Image2 VARCHAR, Image3 VARCHAR, Image4 VARCHAR," +
				"ItemDescription VARCHAR, Brand VARCHAR, Quantity VARCHAR, ModelNumber VARCHAR, UnitCost VARCHAR, Notes VARCHAR" +
					"" +
					"" +
					");");

			//db.ExecSQL("INSERT INTO "
			//	+ "ManualEntry"
			///+ " (ProjectName, ClientName,Location,Image1,Image2,Image3,Image4,ItemDescription,Brand,Quantity,ModelNumber,UnitCost,Notes)"
				//+ " VALUES ('"+txtprojectname.Text+"', '"+txtclientname.Text+"', '"+txtlocation.Text+"', '"+img1.+"');			// ICursor c = db.RawQuery("SELECT * FROM " + "ManualEntry" , null);
			//int Column1 = c.GetColumnIndex("Field1");
			//int Column2 = c.GetColumnIndex("Field2");

			//c.MoveToFirst();
			//if (c != null) {
				// Loop through all Results
				//do {
					//String Name = c.GetString (Column1);
					//int Age = c.GetInt (Column2);
					//Data = Data + Name + "/" + Age + "\n";
				//} while(c.MoveToNext());
			//}
		}

		private string CreateDirectoryForPictures()
		{
			String path = Android.OS.Environment.GetExternalStoragePublicDirectory (Android.OS.Environment.DirectoryPictures).ToString()+"/ImInventory";
			if (! Directory.Exists (path)) {

				Directory.CreateDirectory (path);

			}
			return path;
		}
		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			//base.OnActivityResult (requestCode, resultCode, data);
			//Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
			//Uri contentUri = Android.Net.Uri.FromFile(data.Data);
			//Bundle extras;
			Bitmap photo;
			string path=CreateDirectoryForPictures()+"/"+capturedimagename;
			if (requestCode == 1 && resultCode==Result.Ok){
				//extras= data.Extras;
				//BitmapFactory.Options options = new BitmapFactory.Options();
				//options.InPreferredConfig = Bitmap.Config.Argb8888;
				BitmapFactory.Options options = new BitmapFactory.Options();
				options.InJustDecodeBounds = false;
				options.InPreferredConfig = Android.Graphics.Bitmap.Config.Rgb565;
				options.InDither = true;
				photo=null;
				photo = BitmapFactory.DecodeFile(path);
				Matrix m = new Matrix();
				m.SetRectToRect(new RectF(0, 0, photo.Width, photo.Height), new RectF(0, 0, 100, 115), Matrix.ScaleToFit.Center);
				Bitmap resizedBitmap= Bitmap.CreateBitmap(photo, 0, 0, photo.Width, photo.Height, m, true);

				Cam1.SetImageBitmap (resizedBitmap);
				TextView	Cam1Path = FindViewById<TextView> (Resource.Id.cam1path);
				Cam1Path.Text = capturedimagename;

			}

			if (requestCode == 2 && resultCode==Result.Ok){
				BitmapFactory.Options options = new BitmapFactory.Options();
				options.InJustDecodeBounds = false;
				options.InPreferredConfig = Android.Graphics.Bitmap.Config.Rgb565;
				options.InDither = true;
				photo=null;
				photo = BitmapFactory.DecodeFile(path);
				Matrix m = new Matrix();
				m.SetRectToRect(new RectF(0, 0, photo.Width, photo.Height), new RectF(0, 0, 100, 115), Matrix.ScaleToFit.Center);
				Bitmap resizedBitmap= Bitmap.CreateBitmap(photo, 0, 0, photo.Width, photo.Height, m, true);

				//Cam2.SetTag ("imagename", capturedimagename);
				Cam2.SetImageBitmap (resizedBitmap);
			}
			if (requestCode == 3 && resultCode==Result.Ok){
				BitmapFactory.Options options = new BitmapFactory.Options();
				options.InJustDecodeBounds = false;
				options.InPreferredConfig = Android.Graphics.Bitmap.Config.Rgb565;
				options.InDither = true;
				photo=null;
			
				photo = BitmapFactory.DecodeFile(path);
			Matrix m = new Matrix();
				m.SetRectToRect(new RectF(0, 0, photo.Width, photo.Height), new RectF(0, 0, 100, 115), Matrix.ScaleToFit.Center);
				Bitmap resizedBitmap= Bitmap.CreateBitmap(photo, 0, 0, photo.Width, photo.Height, m, true);
				//Cam3.SetTag ("imagename", capturedimagename);
				Cam3.SetImageBitmap (resizedBitmap);
			}
			if (requestCode == 4 && resultCode==Result.Ok){
				BitmapFactory.Options options = new BitmapFactory.Options();
				options.InJustDecodeBounds = false;
				options.InPreferredConfig = Android.Graphics.Bitmap.Config.Rgb565;
				options.InDither = true;
				photo=null;

				photo = BitmapFactory.DecodeFile(path,options);
			Matrix m = new Matrix();
				m.SetRectToRect(new RectF(0, 0, photo.Width, photo.Height), new RectF(0, 0, 100, 115), Matrix.ScaleToFit.Center);
				Bitmap resizedBitmap= Bitmap.CreateBitmap(photo, 0, 0, photo.Width, photo.Height, m, true);
				//Cam4.SetTag ("imagename", capturedimagename);
				Cam4.SetImageBitmap (resizedBitmap);
			}


		}

		private string GetPathToImage(Android.Net.Uri uri)
		{
			string path = null;
			// The projection contains the columns we want to return in our query.
			string[] projection = new[] { Android.Provider.MediaStore.Audio.Media.InterfaceConsts.Data };
			using (ICursor cursor = ManagedQuery(uri, projection, null, null, null))
			{
				if (cursor != null)
				{
					int columnIndex = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Audio.Media.InterfaceConsts.Data);
					cursor.MoveToFirst();
					path = cursor.GetString(columnIndex);
				}
			}
			return path;
		}


	}
}

