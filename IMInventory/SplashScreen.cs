
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
using System.Threading;
using Android.Content.PM;

namespace IMInventory
{
	[Activity(Theme = "@style/Theme.Splash", MainLauncher = true , ScreenOrientation=Android.Content.PM.ScreenOrientation.SensorPortrait)]
	public class SplashScreen : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			//Java.Lang.IRunnable
			Thread.Sleep(2000);

			StartActivity(typeof(Login));
			this.Finish ();
		}
	}
}

