
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

namespace IMInventory
{
	[Activity (Label = "PaymentActivity")]			
	public class PaymentActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			this.RequestWindowFeature(WindowFeatures.NoTitle);
			SetContentView(Resource.Layout.Payment);
			WebView paymentwebview = FindViewById<WebView>(Resource.Id.paymentwebView);
			paymentwebview.Settings.JavaScriptEnabled=true;
			paymentwebview.Settings.DomStorageEnabled=true;
			paymentwebview.LoadUrl("http://59.162.181.91/dtswork/FaceRecognition/stripe.html");
			paymentwebview.SetWebViewClient (new WebViewClient ());
			//paymentdialog.Show();
			// Create your application here
		}
	}
}

