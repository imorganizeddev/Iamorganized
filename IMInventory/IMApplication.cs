using System;
using Android.App;
using Android.Hardware;
using Android.Media;
using Android.Database.Sqlite;
using Android.Preferences;
using Android.Content;

namespace IMInventory
{
	public class IMApplication: Application
	{
		public static Camera camera=null;
		public static MediaPlayer player=null;
		public static SQLiteDatabase imdb = null;
		public static long empid = 0;
		public static long projectid = 0;
		public static string projectname = "";
		public static string clientname = "";
		public static string xml = "";
		public static string folderurl="";
		public static string folderid="";
		public static string accesstoken="";
		public static string username = "";
		public static ISharedPreferences pref=null;

		public IMApplication ()
		{
			
		}
	}
}

