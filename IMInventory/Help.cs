
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
using Android.Graphics;
using IMInventory.iminventory;
using Android.Webkit;
using System.Data;

namespace IMInventory
{
	[Activity (Label = "Help")]			
	public class Help : Activity
	{
		WebService objime;
		WebView mWebView ;
		String customHtml = "<html><body> Loading .. </body></html>";
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			this.RequestWindowFeature(WindowFeatures.NoTitle); 
			SetContentView (Resource.Layout.Help);
			// Create your application here

			Typeface tf=Typeface.CreateFromAsset(Assets,"Fonts/ROBOTO-LIGHT.TTF");
			TextView txtHelpScreenheading = FindViewById<TextView> (Resource.Id.txtHelpScreenheading);
			txtHelpScreenheading.Typeface=tf;
			txtHelpScreenheading.Invalidate();

			// Define Web View
			mWebView = FindViewById<WebView>(Resource.Id.helpwebView);

			// Get Web View HTML content and assign
			objime = new WebService();
			objime.CMSAsync("1");
			objime.CMSCompleted += getCMSxml;
			mWebView.LoadData(customHtml, "text/html", "UTF-8");

			// Back Button Event
			ImageView imghelpBack = FindViewById<ImageView>(Resource.Id.imghelpBack);
			imghelpBack.Click += delegate {
				this.Finish();
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

