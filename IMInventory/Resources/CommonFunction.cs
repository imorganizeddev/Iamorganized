using System;
using Android.App;
using Android.OS;
using Android.Content;
using Android.Media;

namespace IMInventory
{
	public class CommonFunction : Activity
	{
		public CommonFunction ()
		{
		}
		/// <summary>
		/// Vibrates the device.
		/// </summary>
		public  void VibrateDevice()
		{
			Vibrator v = (Vibrator)this.GetSystemService (Context.VibratorService); // Make phone vibrate
			v.Vibrate (1000);
		}

		public void PlayNitificationTone ()
		{
			Android.Net.Uri notification = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
			Ringtone r = RingtoneManager.GetRingtone(this, notification);
			r.Play ();
		}
	}
}

