
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
using Android.Webkit;
using System.Data;
using IMInventory.iminventory;
using Android.Graphics;

namespace IMInventory
{
	[Activity (Label = "Term", ScreenOrientation=Android.Content.PM.ScreenOrientation.SensorPortrait, MainLauncher=false)]			
	public class Term : Activity
	{
		WebService objime;
		WebView mWebView ;
		String customHtml = "<html><body> Loading .. </body></html>";

		protected override void OnCreate (Bundle bundle)
		{
            base.OnCreate(bundle);
			this.RequestWindowFeature(WindowFeatures.NoTitle);            
            SetContentView(Resource.Layout.Terms);
			if (IMApplication.player != null) {
				IMApplication.player.Stop ();
				IMApplication.player = null;
			}
			// Define Font Face
			Typeface tf=Typeface.CreateFromAsset(Assets,"Fonts/ROBOTO-LIGHT.TTF");
			TextView txtTermsScreenheading = FindViewById<TextView> (Resource.Id.txtTermsScreenheading);
			txtTermsScreenheading.Typeface=tf;
			txtTermsScreenheading.Invalidate();

			// Define Web View
            mWebView = FindViewById<WebView>(Resource.Id.webView);

			// Get Web View HTML content and assign
			objime = new WebService();
			objime.CMSAsync("2");
			objime.CMSCompleted += getCMSxml;
            mWebView.LoadData(customHtml, "text/html", "UTF-8");

			// Back Button Event
			ImageView imgtermsBack = FindViewById<ImageView>(Resource.Id.imgtermsBack);
			imgtermsBack.Click += delegate {
				StartActivity(typeof(Signup));	
			};
		}

		private void getCMSxml (object sender, CMSCompletedEventArgs e)
		{
			try {

				DataSet ds = new DataSet ();
				string innerxml = e.Result.InnerXml.ToString ();
				innerxml = "<ds><table1>" + innerxml + "</table1></ds>";
				DataTable dataTable = new DataTable ("table1");			
				dataTable.Columns.Add ("CMSID", typeof(string));
				dataTable.Columns.Add ("Heading", typeof(string));
				dataTable.Columns.Add ("Desc", typeof(string));
				ds.Tables.Add (dataTable);


				System.IO.StringReader xmlSR = new System.IO.StringReader (innerxml);
				ds.ReadXml (xmlSR, XmlReadMode.IgnoreSchema);
				dataTable = ds.Tables [0];
				if (dataTable.Rows.Count > 0) {
					customHtml = "<html><body><h1 style='font-family:ROBOTO-LIGHT;'>" + dataTable.Rows [0] ["Heading"].ToString () + "</h1>";
					customHtml += dataTable.Rows [0] ["Desc"].ToString () + "</body></html>";
				}
				
			} catch {

				customHtml = "<html><body style='font-family:ROBOTO-LIGHT;'><h1 style='font-family:ROBOTO-LIGHT;'>Oops !!</h1><p>It's taking too long time ! " +
					"Please check your network connection and try again ! </p></body></html>";
					
			}

			//Load Web View Data						
			mWebView.LoadData(customHtml, "text/html", "UTF-8");
		}
	}
}

