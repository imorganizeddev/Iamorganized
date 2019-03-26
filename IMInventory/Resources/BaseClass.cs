using System;
using Android.Content;
using Android.Preferences;

namespace IMInventory
{
	public class BaseClass:CommonFunction
	{
		
		public BaseClass ()
		{
			
		}
		/// <summary>
		/// Sets the ISharedPreferences manager.
		/// </summary>
		/// <param name="Key">String Key.</param>
		/// <param name="Value">Long Value.</param>
		public void SetPreferenceManager<T> (string Key, T Value,Context contex)
		{
			try{
				ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences (contex);
			ISharedPreferencesEditor editor = prefs.Edit ();
			if (Value.GetType () == typeof(Int32))
				editor.PutInt (Key, Convert.ToInt32 (Value));
			else if (Value.GetType () == typeof(Int64))
				editor.PutLong (Key, Convert.ToInt64 (Value));
			else
				editor.PutString (Key, Convert.ToString (Value));
			editor.Commit ();
			// applies changes synchronously on older APIs
				editor.Apply ();
			}
			catch{
			}
		}
	}
}

