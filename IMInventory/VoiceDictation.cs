
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
using Android.Media;
using System.IO;
using Android.Graphics;
using Java.IO;
using Android.Content.Res;
using System.Xml;
using Android.Database;
using Android.Database.Sqlite;
using IMInventory.iminventory;
using Android.Preferences;
using DK.Ostebaronen.FloatingActionButton;
using System.Text.RegularExpressions;
using Android.Provider;
using Android.Widget;
using Java.Net;
using System.Security.Cryptography;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Data;
using Android.Net.Wifi;



namespace IMInventory
{
    [Activity(Label = "VoiceDictation", MainLauncher = false, ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorPortrait)]
    public class VoiceDictation : BaseClass
    {
        int currentplayingindex = 0;
        string[] ArrRecordAudioPath;
        ImageView imgStartRecord;
        ImageView imgPauseRecord;
        ImageView imgDeleteRecord;
        ImageView imgSaveRecord;
        ImageView imgPausePlayer;
        ImageView imgPlayPlayer;
        ImageView imgrewind;
        ImageView fastforward;
        MediaPlayer player;
        SQLiteDatabase db = null;
        System.Timers.Timer tmr;
        System.Timers.Timer playingtmr;
        MediaRecorder recorder;
        string path;

        private ImageView Addvoicecam_;
        LinearLayout llplayer;
        private string capturedimagename;
        long second = 0;
        SeekBar playerseek;
        long empid;
        long projectid;
        private string recordfilename;
        int multipleimagebuttonclickcount = 0;
        string projectname;
        string clientname;
        string locationname;
        private Fab _fab;
        ISharedPreferencesEditor editor;
        ISharedPreferences prefs;
        string previousfilepath;
        int currentPreviewcount = 0;
        string images = "";
        int AlertVibrate;
        int AlertTone;
        System.String RecordAudioPath;
        System.String RecordAudioPathList;
        System.String RecordAudioNameList;
        AudioRecord audiorecord;
        private bool isRecording = false;

        private int bufferSize = 0;
        int FromVoiceEdit = 0;
        LinearLayout llVoiceMultipleImageView;
        int audiocount = 0;

        private AudioManager mAudioManager;
        private ComponentName mRemoteControlResponder;
        public static bool StartRecord = false;
        long UserType;
        string UserPlan;
        string plan = "";
        string amount = "";
        string planid = "";
        string userid = "";
        string subscriptionid = "";
        protected override void OnCreate(Bundle bundle)
        {

            prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            empid = prefs.GetLong("EmpID", 0);
            projectid = prefs.GetLong("ProjectID", 0);
            projectname = prefs.GetString("ProjectName", "");
            clientname = prefs.GetString("ClientName", "");
            locationname = prefs.GetString("LocationName", "");
            AlertVibrate = prefs.GetInt("AlertVibrate", 0);
            AlertTone = prefs.GetInt("AlertTone", 0);
            FromVoiceEdit = prefs.GetInt("FromVoiceEdit", 0);
            UserType = prefs.GetLong("UserType", 1);
            UserPlan = prefs.GetString("UserPlan", "");
            subscriptionid = prefs.GetString("StripeSubscriptionID", "");
            bufferSize = AudioRecord.GetMinBufferSize(8000,
                ChannelIn.Stereo,
                Android.Media.Encoding.Pcm16bit);

            recorder = new MediaRecorder();
            base.OnCreate(bundle);
            this.RequestWindowFeature(WindowFeatures.NoTitle);

            SetContentView(Resource.Layout.VoiceDictation);
            //RegisterMediaButton ();
            db = this.OpenOrCreateDatabase("ImInventory", FileCreationMode.Private, null);
            //db.ExecSQL("DROP TABLE IF EXISTS " + "tbl_VoiceImages");
            db.ExecSQL("CREATE TABLE IF NOT EXISTS " + "tbl_VoiceImages"
                + " (ID INTEGER PRIMARY KEY AUTOINCREMENT,  ImageID INTEGER, ImageName VARCHAR);");

            //db.ExecSQL("DROP TABLE IF EXISTS " + "tbl_VoiceAudios");
            db.ExecSQL("CREATE TABLE IF NOT EXISTS " + "tbl_VoiceAudios"
                + " (ID INTEGER PRIMARY KEY AUTOINCREMENT,  AudioID INTEGER, AudioName VARCHAR);");
            llplayer = FindViewById<LinearLayout>(Resource.Id.llplayer);
            playerseek = FindViewById<SeekBar>(Resource.Id.playerseekbar);
            if (FromVoiceEdit == 1)
            {
                llVoiceMultipleImageView = FindViewById<LinearLayout>(Resource.Id.llVoiceImageView);
                FromVoiceEdit = 0;
                db = this.OpenOrCreateDatabase("ImInventory", FileCreationMode.Private, null);
                ICursor c1 = db.RawQuery("SELECT * FROM " + "tbl_Inventory Where InventoryType=2 AND Location='" + locationname + "'", null);
                PopulatePreviousFiles(c1);

            }
            if (IMApplication.player != null)
            {
                IMApplication.player.Stop();
                IMApplication.player = null;
            }
            _fab = FindViewById<Fab>(Resource.Id.btnSaveVoiceEntryInventory);
            _fab.FabColor = Color.Blue;
            _fab.FabDrawable = Resources.GetDrawable(Resource.Drawable.icon_save2x);
            _fab.Show();

            Typeface tf = Typeface.CreateFromAsset(Assets, "Fonts/ROBOTO-LIGHT.TTF");

            EditText txtproname = FindViewById<EditText>(Resource.Id.txtvoiceprojectname);
            txtproname.Typeface = tf;
            txtproname.Invalidate();

            EditText txtclientname = FindViewById<EditText>(Resource.Id.txtvoiceclientname);
            txtclientname.Typeface = tf;
            txtclientname.Invalidate();

            EditText txtlocation = FindViewById<EditText>(Resource.Id.txtvoicelocation);
            txtlocation.Typeface = tf;
            txtlocation.Invalidate();

            // Create your application here
            txtproname.SetText(projectname, TextView.BufferType.Editable);
            txtclientname.SetText(clientname, TextView.BufferType.Editable);
            txtlocation.SetText(locationname, TextView.BufferType.Editable);


            editor = prefs.Edit();
            txtlocation.TextChanged += delegate
            {
                editor.PutString("LocationName", txtlocation.Text);
                editor.Commit();
                editor = prefs.Edit();
                locationname = prefs.GetString("LocationName", "");
            };
            imgStartRecord = FindViewById<ImageView>(Resource.Id.btnstartrecord);
            imgPauseRecord = FindViewById<ImageView>(Resource.Id.btnpauserecord);
            imgDeleteRecord = FindViewById<ImageView>(Resource.Id.imgdeletedrecord);
            imgSaveRecord = FindViewById<ImageView>(Resource.Id.imgsaverecord);
            imgPausePlayer = FindViewById<ImageView>(Resource.Id.imgPausePlayer);
            imgPlayPlayer = FindViewById<ImageView>(Resource.Id.imgPlayPlayer);
            imgrewind = FindViewById<ImageView>(Resource.Id.imgrewind);
            fastforward = FindViewById<ImageView>(Resource.Id.fastforward);

            imgPausePlayer.Click += delegate
            {
                player.Pause();
                imgPausePlayer.Visibility = ViewStates.Gone;
                imgPlayPlayer.Visibility = ViewStates.Visible;
            };
            imgPlayPlayer.Click += delegate
            {
                ArrRecordAudioPath = RecordAudioPathList.Split('|');

                currentplayingindex = 0;
                llplayer.Visibility = ViewStates.Visible;
                playerseek.Visibility = ViewStates.Visible;
                if (tmr != null)
                {
                    tmr.Stop();

                }
                imgDeleteRecord.Visibility = ViewStates.Visible;
                imgSaveRecord.Visibility = ViewStates.Visible;
                imgStartRecord = FindViewById<ImageView>(Resource.Id.btnstartrecord);
                imgStartRecord.Visibility = ViewStates.Visible;
                imgPauseRecord = FindViewById<ImageView>(Resource.Id.btnpauserecord);
                imgPauseRecord.Visibility = ViewStates.Gone;

                imgPausePlayer = FindViewById<ImageView>(Resource.Id.imgPausePlayer);
                imgPlayPlayer = FindViewById<ImageView>(Resource.Id.imgPlayPlayer);
                imgPausePlayer.Visibility = ViewStates.Visible;
                imgPlayPlayer.Visibility = ViewStates.Gone;

                imgPausePlayer.Visibility = ViewStates.Visible;
                imgPlayPlayer.Visibility = ViewStates.Gone;
                PlayRecordedAudio();
                IMApplication.player = player;

            };
            fastforward.Click += delegate
            {
                int currentpostion = player.CurrentPosition;
                player.SeekTo(currentpostion + 2000);
            };
            imgrewind.Click += delegate
            {
                int currentpostion = player.CurrentPosition;
                player.SeekTo(currentpostion - 2000);
            };


            path = CreateDirectoryForAudio() + "/" + Guid.NewGuid() + ".3gpp";
            previousfilepath = "";
            imgStartRecord.Click += delegate
            {
                if (UserPlan.ToLower().Contains("platinum"))
                {
                    txtlocation = FindViewById<EditText>(Resource.Id.txtvoicelocation);
                    if (txtlocation.Text != "")
                    {

                        AlertVibrate = prefs.GetInt("AlertVibrate", 0);
                        AlertTone = prefs.GetInt("AlertTone", 0);
                        // Vibrate Device
                        if (AlertVibrate == 1)
                            VibrateDevice();
                        // Play tone
                        if (AlertTone == 1)
                            PlayNitificationTone();
                        RecordAudio();
                    }
                    else
                    {

                        Toast.MakeText(this, "Please enter a location name", ToastLength.Long).Show();
                    }
                    //RecordAudioInWavFormat();
                }
                else
                {

                    OpenSubscriptionAlert();

                    //Toast.MakeText (this, "you are not a platinum member", ToastLength.Short).Show ();
                }

            };
            imgPauseRecord.Click += delegate
            {
                AlertVibrate = prefs.GetInt("AlertVibrate", 0);
                AlertTone = prefs.GetInt("AlertTone", 0);

                // Vibrate Device
                if (AlertVibrate == 1)
                {
                    VibrateDevice();
                }
                // Play tone
                if (AlertTone == 1)
                {
                    PlayNitificationTone();
                }
                StopRecordAudio();
            };
            imgDeleteRecord.Click += delegate
            {
                DeleteRecord(path);
            };
            imgSaveRecord.Click += delegate
            {
                TextView lblPlayingCount = FindViewById<TextView>(Resource.Id.lblPlayingCount);
                lblPlayingCount.Visibility = ViewStates.Gone;
                SaveRecord(path);
            };
            playerseek = FindViewById<SeekBar>(Resource.Id.playerseekbar);
            playerseek.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) =>
            {

            };

            Random rnd = new Random();
            int number = 0;
            Addvoicecam_ = FindViewById<ImageView>(Resource.Id.Addvoicecam_);


            rnd = new Random();
            number = 0;
            Addvoicecam_.Click += delegate
            {
                if (UserPlan.ToLower().Contains("platinum"))
                {
                    if (IMApplication.camera != null)
                    {
                        IMApplication.camera.Release();
                        IMApplication.camera = null;
                    }
                    multipleimagebuttonclickcount = multipleimagebuttonclickcount + 1;
                    number = rnd.Next(10000, 99999);
                    Intent intent = new Intent(Android.Provider.MediaStore.ActionImageCapture);
                    string picpath = CreateDirectoryForPictures();
                    string picname = System.String.Format("img_{0}.jpg", number);
                    Java.IO.File objFile = new Java.IO.File(picpath, picname);
                    intent.PutExtra(Android.Provider.MediaStore.ExtraOutput, Android.Net.Uri.FromFile(objFile));
                    intent.PutExtra("height", 100);

                    capturedimagename = picname;
                    StartActivityForResult(intent, 0);

                }
                else
                {

                    OpenSubscriptionAlert();

                    //Toast.MakeText (this, "you are not a platinum member", ToastLength.Short).Show ();
                }

            };

            _fab.Click += delegate
            {
                if (UserPlan.ToLower().Contains("platinum"))
                {
                    SaveDataToLocalDataBase(false);
                }
                else
                {

                    OpenSubscriptionAlert();

                    //Toast.MakeText (this, "you are not a platinum member", ToastLength.Short).Show ();
                }
            };
            ImageView bactolist = FindViewById<ImageView>(Resource.Id.btnVoiceBacktolist);
            bactolist.Click += delegate
            {

				// SaveDataToLocalDataBase (false);



                LinearLayout llVoiceImageView = FindViewById<LinearLayout>(Resource.Id.llVoiceImageView);
                //llVoiceMultipleImageView=FindViewById<LinearLayout> (Resource.Id.llVoiceImageView);

                llVoiceImageView.RemoveAllViews();
                try
                {
                    StartActivity(typeof(LocationList));
                }
                catch {
                    Toast.MakeText(this, "Oops…something happened, Please try again", ToastLength.Short).Show();
                
                }
            };

            Button btnVoiceSavePreview = FindViewById<Button>(Resource.Id.btnVoiceSavePreview);
            btnVoiceSavePreview.Click += delegate
            {
                LinearLayout LayoutVoiceEntry = FindViewById<LinearLayout>(Resource.Id.LayoutVoiceEntry);
                LinearLayout LayoutVoicePreview = FindViewById<LinearLayout>(Resource.Id.LayoutVoicePreview);
                LayoutVoiceEntry.Visibility = ViewStates.Visible;
                LayoutVoicePreview.Visibility = ViewStates.Gone;
            };

            Button btnVoiceDiscardPreview = FindViewById<Button>(Resource.Id.btnVoiceDiscardPreview);
            btnVoiceDiscardPreview.Click += delegate
            {

                if (currentPreviewcount != 0)
                {
                    rnd = new Random();
                    number = rnd.Next(10000, 99999);
                    Intent intent = new Intent(Android.Provider.MediaStore.ActionImageCapture);
                    string picturepath = CreateDirectoryForPictures();
                    string picname = System.String.Format("img_{0}.jpg", number);
                    Java.IO.File objFile = new Java.IO.File(picturepath, picname);
                    intent.PutExtra(Android.Provider.MediaStore.ExtraOutput, Android.Net.Uri.FromFile(objFile));
                    capturedimagename = picname;
                    StartActivityForResult(intent, currentPreviewcount);
                    //currentPreviewcount = 0;
                }

            };

        }


        public void DeleteRecord(string path)
        {
            TextView lblPlayingCount = FindViewById<TextView>(Resource.Id.lblPlayingCount);
            lblPlayingCount.Visibility = ViewStates.Gone;
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
                imgDeleteRecord.Visibility = ViewStates.Gone;
                imgSaveRecord.Visibility = ViewStates.Gone;
            }
            TextView tmrtime = (TextView)FindViewById(Resource.Id.txtrecordtime);
            tmrtime.SetText("00:00:00", TextView.BufferType.Editable);
            second = 0;
            playerseek = FindViewById<SeekBar>(Resource.Id.playerseekbar);
            playerseek.Progress = 0;
            llplayer.Visibility = ViewStates.Gone;
            playerseek.Visibility = ViewStates.Gone;
            RecordAudioPath = "";
            RecordAudioPathList = "";
            ArrRecordAudioPath = null;

        }
        public void SaveRecord(string path)
        {
            imgDeleteRecord.Visibility = ViewStates.Gone;
            imgSaveRecord.Visibility = ViewStates.Gone;
            TextView tmrtime = (TextView)FindViewById(Resource.Id.txtrecordtime);
            tmrtime.SetText("00:00:00", TextView.BufferType.Editable);
            second = 0;
            playerseek = FindViewById<SeekBar>(Resource.Id.playerseekbar);
            playerseek.Progress = 0;
            playerseek.Visibility = ViewStates.Invisible;
            //recordfilename = previousfilepath.Split ('/') [previousfilepath.Split ('/').Length - 1].ToString ();
            //player.Stop ();
            llplayer.Visibility = ViewStates.Gone;
            playerseek.Visibility = ViewStates.Gone;
        }
        public void RecordAudio()
        {
            TextView lblPlayingCount = FindViewById<TextView>(Resource.Id.lblPlayingCount);
            lblPlayingCount.Visibility = ViewStates.Gone;
            if (player != null)
            {
                player.Stop();
            }
            llplayer.Visibility = ViewStates.Invisible;
            playerseek.Visibility = ViewStates.Invisible;
            //Timer Region
            tmr = new System.Timers.Timer();
            tmr.Elapsed += new System.Timers.ElapsedEventHandler(tmr_Elapsed);
            tmr.Interval = 1000;
            tmr.Enabled = true;
            tmr.Start();
            //Timer Region
            imgStartRecord.Visibility = ViewStates.Invisible;
            imgPauseRecord.Visibility = ViewStates.Visible;
            imgDeleteRecord.Visibility = ViewStates.Invisible;
            imgSaveRecord.Visibility = ViewStates.Invisible;
            string audiofilename = Guid.NewGuid() + ".3gpp";
            RecordAudioNameList = RecordAudioNameList + "|" + audiofilename;
            RecordAudioPath = CreateDirectoryForAudio() + "/" + audiofilename;
            recorder = new MediaRecorder();
			recorder.SetAudioSource(AudioSource.Mic);
			recorder.SetOutputFormat(OutputFormat.Mpeg4);
            recorder.SetAudioEncoder(AudioEncoder.Aac);
            recorder.SetAudioSamplingRate(180000);
            recorder.SetAudioEncodingBitRate(200000);
            recorder.SetAudioChannels(1);
            recorder.SetOutputFile(RecordAudioPath);
            recorder.Prepare();
            recorder.Start();
            //int buffersize = AudioRecord.GetMinBufferSize (44100, ChannelIn.Stereo, Android.Media.Encoding.Pcm16bit);
            //audiorecord = new AudioRecord(AudioSource.Mic, 44100, ChannelIn.Stereo, Android.Media.Encoding.Pcm16bit, 0);


        }

        public void StopRecordAudio()
        {
            recorder.Stop();
            audiocount = audiocount + 1;
            db.ExecSQL("CREATE TABLE IF NOT EXISTS " + "tbl_VoiceAudios"
                + " (ID INTEGER PRIMARY KEY AUTOINCREMENT,  AudioID INTEGER, AudioName VARCHAR);");

            db.ExecSQL("INSERT INTO "
                + "tbl_VoiceAudios"
                + " (AudioName, AudioID)"
                + " VALUES ('" + RecordAudioPath + "','" + audiocount + "')");

            RecordAudioPathList = RecordAudioPathList + "|" + RecordAudioPath;
            RecordAudioPathList = RecordAudioPathList.TrimStart('|');
            ArrRecordAudioPath = RecordAudioPathList.Split('|');
            currentplayingindex = 0;
            llplayer.Visibility = ViewStates.Visible;
            playerseek.Visibility = ViewStates.Visible;
            tmr.Stop();
            imgDeleteRecord.Visibility = ViewStates.Visible;
            imgSaveRecord.Visibility = ViewStates.Visible;
            imgStartRecord.Visibility = ViewStates.Visible;
            imgPauseRecord.Visibility = ViewStates.Invisible;
            imgPausePlayer.Visibility = ViewStates.Gone;
            imgPlayPlayer.Visibility = ViewStates.Visible;
            TextView lblPlayingCount = FindViewById<TextView>(Resource.Id.lblPlayingCount);
            lblPlayingCount.Text = "Playing 0 of " + ArrRecordAudioPath.Length;
            SaveDataToLocalDataBase(true);
        }

        private void PlayRecordedAudio()
        {
            if (player == null)
            {
                player = new MediaPlayer();
                player.SetAudioStreamType(Android.Media.Stream.Music);
            }
            else
            {
                player.Release();
                player = new MediaPlayer();
                player.SetAudioStreamType(Android.Media.Stream.Music);
            }
            if (!ArrRecordAudioPath[currentplayingindex].ToString().Contains("/"))
            {
                player.SetDataSource(CreateDirectoryForAudio() + "/" + ArrRecordAudioPath[currentplayingindex].ToString());
            }
            else
            {
                player.SetDataSource(ArrRecordAudioPath[currentplayingindex].ToString());

            }
            player.Prepare();
            player.Start();
            TextView lblPlayingCount;
            player.Completion += delegate
            {
                if (currentplayingindex == ArrRecordAudioPath.Length)
                {

                    imgPausePlayer.Visibility = ViewStates.Gone;
                    imgPlayPlayer.Visibility = ViewStates.Visible;
                    lblPlayingCount = FindViewById<TextView>(Resource.Id.lblPlayingCount);
                    lblPlayingCount.Text = "Playing 0 of " + ArrRecordAudioPath.Length;
                    //lblPlayingCount.Visibility=ViewStates.Gone;
                }
                else
                {
                    PlayRecordedAudio();
                }
            };
            lblPlayingCount = FindViewById<TextView>(Resource.Id.lblPlayingCount);
            lblPlayingCount.Visibility = ViewStates.Visible;
            int playingcount = currentplayingindex + 1;
            lblPlayingCount.Text = "Playing " + playingcount + " of " + ArrRecordAudioPath.Length;
            int intplayercurrentposition = player.CurrentPosition / 1000;
            playerseek = FindViewById<SeekBar>(Resource.Id.playerseekbar);
            playerseek.Progress = intplayercurrentposition;
            playerseek.Max = player.Duration / 1000;
            player.SetScreenOnWhilePlaying(true);
            playingtmr = new System.Timers.Timer(1000);
            playingtmr.Start();
            playingtmr.Elapsed += new System.Timers.ElapsedEventHandler(playingtmr_Elapsed);

            currentplayingindex = currentplayingindex + 1;



        }



        protected void tmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            second = second + 1;
            //Toast.MakeText (this, second.ToString(), ToastLength.Long).Show ();
            TextView tmrtime = (TextView)FindViewById(Resource.Id.txtrecordtime);
            RunOnUiThread(() =>
            {
                tmrtime.Text = string.Format("{0:00}:{1:00}:{2:00}", second / 3600, (second / 60) % 60, second % 60);
            });




        }

        protected void playingtmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            int playercurrentposition = player.CurrentPosition / 1000;
            playerseek = FindViewById<SeekBar>(Resource.Id.playerseekbar);
            playerseek.Progress = playercurrentposition;
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
        private string CreateDirectoryForOriginalPictures()
        {
            String path = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).ToString() + "/ImInventory/Original";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
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
        private Bitmap GetResizeBitmap(string path, int reqheight, int reqwidth, bool crop)
        {
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InJustDecodeBounds = false;
            options.InPreferredConfig = Android.Graphics.Bitmap.Config.Rgb565;
            options.InDither = true;
            options.InPurgeable = true;
            //options.InSampleSize = 3;
            Bitmap photo = null;
            photo = BitmapFactory.DecodeFile(path, options);

            Matrix m = new Matrix();
            m.SetRectToRect(new RectF(0, 0, photo.Width, photo.Height), new RectF(0, 0, reqheight, reqwidth), Matrix.ScaleToFit.Center);
            Bitmap resizedBitmap = null;
            if (crop)
            {
                resizedBitmap = Bitmap.CreateBitmap(photo, 0, 0, reqwidth, reqheight);
            }
            else
            {
                resizedBitmap = Bitmap.CreateBitmap(photo, 0, 0, photo.Width, photo.Height, m, true);
            }
            photo.Dispose();
            return resizedBitmap;
        }

        private byte[] Combine(byte[] a, byte[] b)
        {
            byte[] c = new byte[a.Length + b.Length];
            System.Array.Copy(a, 0, c, 0, a.Length);
            System.Array.Copy(b, 0, c, a.Length, b.Length);
            return c;
        }
        private byte[] getByteArrayFromImage(String filePath)
        {


            byte[] bytes = null;
            try
            {
                Java.IO.File file = new Java.IO.File(filePath);

                FileInputStream fis = new FileInputStream(file);
                //create FileInputStream which obtains input bytes from a file in a file system
                //FileInputStream is meant for reading streams of raw bytes such as image data. For reading streams of characters, consider using FileReader.

                //InputStream in = resource.openStream();
                ByteArrayOutputStream bos = new ByteArrayOutputStream();
                byte[] buf = new byte[1024];

                for (int readNum; (readNum = fis.Read(buf)) != -1; )
                {
                    bos.Write(buf, 0, readNum);
                    //no doubt here is 0
                    /*Writes len bytes from the specified byte array starting at offset 
                off to this byte array output stream.*/

                }

                bytes = bos.ToByteArray();
            }
            catch
            {
                return bytes;
            }
            return bytes;
        }
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            string path = CreateDirectoryForPictures() + "/" + capturedimagename;
            string originalimagepath = CreateDirectoryForOriginalPictures() + "/" + capturedimagename;

            LinearLayout LayoutVoiceEntry = FindViewById<LinearLayout>(Resource.Id.LayoutVoiceEntry);
            LinearLayout LayoutVoicePreview = FindViewById<LinearLayout>(Resource.Id.LayoutVoicePreview);
            ImageView VoiceCamImagePreview = FindViewById<ImageView>(Resource.Id.VoiceCamImagePreview);
            LayoutVoiceEntry.Visibility = ViewStates.Visible;
            LayoutVoicePreview.Visibility = ViewStates.Gone;


            if (resultCode == Result.Ok)
            {


               
                LinearLayout llVoiceImageView = FindViewById<LinearLayout>(Resource.Id.llVoiceImageView);
                //int child = llVoiceImageView.ChildCount;
                View v = llVoiceImageView.GetChildAt(0);
                LinearLayout LL1 = new LinearLayout(this);
                Android.Widget.LinearLayout.LayoutParams LLParams = new Android.Widget.LinearLayout.LayoutParams(Android.Widget.LinearLayout.LayoutParams.WrapContent, Android.Widget.LinearLayout.LayoutParams.WrapContent);
                LLParams.SetMargins(10, 0, 0, 0);
                LLParams.Gravity = GravityFlags.Center;

                LL1.SetPadding(10, 10, 10, 10);
                LL1.Orientation = Android.Widget.Orientation.Vertical;


                LL1.LayoutParameters = LLParams;
                LinearLayout LL2 = new LinearLayout(this);

                LLParams = new Android.Widget.LinearLayout.LayoutParams(Android.Widget.LinearLayout.LayoutParams.MatchParent, Android.Widget.LinearLayout.LayoutParams.MatchParent);
                LLParams.Gravity = GravityFlags.Center;
                LL2.Orientation = Android.Widget.Orientation.Vertical;
                LL2.SetBackgroundColor(Color.White);
                string previousimagename = "";
                ImageView imgcamera = new ImageView(this);
                try
                {
                    imgcamera = FindViewById<ImageView>(multipleimagebuttonclickcount);
                    previousimagename = imgcamera.Tag.ToString();
                    imgcamera.Tag = capturedimagename;
                    llVoiceImageView.RemoveView(imgcamera);
                    imgcamera = new ImageView(this);
                    imgcamera.Id = requestCode;
                    imgcamera.Tag = capturedimagename;
                    db = this.OpenOrCreateDatabase("ImInventory", FileCreationMode.Private, null);
                    db.ExecSQL("CREATE TABLE IF NOT EXISTS " + "tbl_VoiceImages"
                        + " (ID INTEGER PRIMARY KEY AUTOINCREMENT,  ImageID INTEGER, ImageName VARCHAR);");


                    ICursor c = db.RawQuery("SELECT * FROM " + "tbl_VoiceImages where ImageID=" + "'" + requestCode + "'", null);

                    int idcolumn = c.GetColumnIndex("ID");
                    c.MoveToFirst();
                    if (c.Count > 0)
                    {
                        if (c != null)
                        {
                            // Loop through all Results
                            do
                            {
                                String ID = c.GetString(idcolumn);
                                ContentValues values = new ContentValues();
                                values.Put("ImageName", capturedimagename);
                                db.Update("tbl_VoiceImages", values, "ID = " + ID, null);
                            } while (c.MoveToNext());
                        }
                    }
                }
                catch (Exception ex)
                {
                    imgcamera = new ImageView(this);
                    imgcamera.Id = multipleimagebuttonclickcount;
                    imgcamera.Tag = capturedimagename;
                    db.ExecSQL("INSERT INTO "
                        + "tbl_VoiceImages"
                        + " (ImageName, ImageID)"
                        + " VALUES ('" + capturedimagename + "','" + multipleimagebuttonclickcount + "')");
                }

                imgcamera.SetBackgroundResource(Resource.Drawable.icon_camera2x);
				ResizeAndSaveImage (path);
				byte[] imagebyte = getByteArrayFromImage (path);
				Bitmap b = BitmapFactory.DecodeByteArray (imagebyte, 0, imagebyte.Length);
				//Bitmap reducedqualityBitmap = GetResizeBitmap (path, 1000, 1000, false);
				imgcamera.SetImageBitmap(Bitmap.CreateScaledBitmap(b, 120, 120, false));
				//imgcamera.SetImageBitmap(GetResizeBitmap(path, 120, 120, false));
                //View vw = new View (this);
                //vw.LayoutParameters = LLParams;
                LL2.LayoutParameters = LLParams;
                LL2.AddView(imgcamera);
                //LL1.AddView (vw);
                LL1.AddView(LL2);
                LinearLayout LL3 = new LinearLayout(this);
                LLParams = new Android.Widget.LinearLayout.LayoutParams(Android.Widget.LinearLayout.LayoutParams.MatchParent, Android.Widget.LinearLayout.LayoutParams.MatchParent);
                LL3.SetBackgroundColor(Color.White);
                LLParams.Gravity = GravityFlags.Center;
                LL3.Orientation = Android.Widget.Orientation.Vertical;
                LL3.LayoutParameters = LLParams;
                TextView txtpicname = new TextView(this);

                try
                {
                    txtpicname = FindViewById<TextView>(1000 + multipleimagebuttonclickcount);

                    txtpicname.Tag = "";
                    llVoiceImageView.RemoveView(imgcamera);
                    txtpicname = new TextView(this);
                    txtpicname.Id = 1000 + requestCode;

                }
                catch
                {
                    txtpicname = new TextView(this);
                    txtpicname.Id = 1000 + multipleimagebuttonclickcount;
                }

                txtpicname.SetBackgroundColor(Color.ParseColor("#cfd9db"));
                txtpicname.SetTextColor(Color.Black);
                LLParams = new Android.Widget.LinearLayout.LayoutParams(Android.Widget.LinearLayout.LayoutParams.MatchParent, Android.Widget.LinearLayout.LayoutParams.WrapContent);
                LLParams.Gravity = GravityFlags.Center;
                txtpicname.LayoutParameters = LLParams;
                string randomnumberfromimage = Regex.Replace(capturedimagename, "[^0-9]+", string.Empty);
                txtpicname.SetText(randomnumberfromimage, TextView.BufferType.Editable);
				txtpicname.Gravity = GravityFlags.Center;
                LL3.AddView(txtpicname);
                LL2.AddView(LL3);
                
                if (requestCode == 0)
                {
                    llVoiceImageView.AddView(LL1);
                    images = images + "|" + capturedimagename;
                }
                else
                {
                    llVoiceImageView.RemoveViewAt(requestCode - 1);
                    llVoiceImageView.AddView(LL1, requestCode - 1);
                }
                imgcamera.Click += getview;
				b.Recycle (); 
			
			}
            if (requestCode == 10)
            {
                WifiManager wifiManager = (WifiManager)GetSystemService(Context.WifiService);
                WifiInfo wInfo = wifiManager.ConnectionInfo;
                String MACAdress = Android.Provider.Settings.Secure.GetString(ContentResolver, Android.Provider.Settings.Secure.AndroidId);
                WebService objime = new WebService();
                objime.GetEmpDetailsByEmpIDAsync(empid.ToString(), MACAdress);
                objime.GetEmpDetailsByEmpIDCompleted += getempxml;
            }
        }
		private void ResizeAndSaveImage(string path){

			//File dir=Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM);

			try{

				Matrix mtx = new Matrix();
				ExifInterface exif = new ExifInterface(path);
				var orientation = (Android.Media.Orientation)exif.GetAttributeInt(ExifInterface.TagOrientation, (int)Android.Media.Orientation.Undefined);
				int rotate = 0;
				switch(orientation){


				case Android.Media.Orientation.Undefined: // Nexus 7 landscape...
					break;

				case Android.Media.Orientation.Normal: // landscape
					break;

				case Android.Media.Orientation.FlipHorizontal:
					break;

				case Android.Media.Orientation.Rotate180:
					rotate=180;
					break;
				case Android.Media.Orientation.FlipVertical:
					break;

				case Android.Media.Orientation.Transpose:
					break;

				case Android.Media.Orientation.Rotate90:
					rotate=90;
					break;

				case Android.Media.Orientation.Transverse:
					break;

				case Android.Media.Orientation.Rotate270: 
					rotate=270;
					break;

				}

				Bitmap b= BitmapFactory.DecodeFile(path);		
				System.IO.File.Delete (path);
				Bitmap outbitmap = null;

				if (b.Height > b.Width) {
					outbitmap = Bitmap.CreateScaledBitmap (b, 768, 1024, false);
				} else { 

					outbitmap = Bitmap.CreateScaledBitmap (b, 1024, 768, false);
				}
				System.IO.File.Delete(path);


				try {

					mtx.PreRotate(rotate);
					outbitmap=Bitmap.CreateBitmap(outbitmap, 0, 0, outbitmap.Width, outbitmap.Height, mtx, false); 

					Java.IO.File file = new Java.IO.File(path);
					var fOut = new FileStream(path, FileMode.Create);
					outbitmap.Compress(Bitmap.CompressFormat.Jpeg, 75, fOut);
					fOut.Close();
					b.Recycle();
					outbitmap.Recycle();   
					mtx.Dispose();
					mtx = null;
				}
				catch {
				}
			} 

			catch (Exception e) {


			}
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
                ds.Tables.Add(dataTable);
                System.IO.StringReader xmlSR = new System.IO.StringReader(innerxml);
                ds.ReadXml(xmlSR, XmlReadMode.IgnoreSchema);
                dataTable = ds.Tables[0];
                int empid = Convert.ToInt16(dataTable.Rows[0]["EmpID"]);
                int isinternal = Convert.ToInt16(dataTable.Rows[0]["IsInternal"]);
                int UserType = Convert.ToInt16(dataTable.Rows[0]["UserType"]);
                string UserPlans = Convert.ToString(dataTable.Rows[0]["UserPlan"]);
                string StripeSubscriptionID = Convert.ToString(dataTable.Rows[0]["StripeSubscriptionID"]);
                if (empid > 0)
                {
                    //Toast.MakeText (this, "Your successfully log in", ToastLength.Short).Show ();
                    //dialog.Hide();
                    ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                    ISharedPreferencesEditor editor = prefs.Edit();
                    //editor.Clear();
                    editor.PutLong("EmpID", empid);
                    editor.PutLong("IsInternal", isinternal);
                    editor.PutString("UserPlan", UserPlan);
                    editor.PutLong("UserType", UserType);

                    editor.PutString("StripeSubscriptionID", StripeSubscriptionID);
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


                }
            }
            catch (Exception ex)
            {

                Toast.MakeText(this, "Something went wrong. Please try back in few minutes.", ToastLength.Short).Show();
            }
        }
        public void getview(object sender, EventArgs e)
        {
            ImageView img = (ImageView)sender;
            int i = 0;
            int id = img.Id;
            ICursor c = db.RawQuery("SELECT * FROM " + "tbl_VoiceImages where ImageID=" + "'" + id + "'", null);
            //c = db.RawQuery("SELECT * FROM " + "tbl_VoiceImages", null);
            int iamgenamecolumn = c.GetColumnIndex("ImageName");
            c.MoveToFirst();
            if (c.Count > 0)
            {
                if (c != null)
                {
                    // Loop through all Results
                    do
                    {
                        if (i == 0)
                        {
                            String Imagename = c.GetString(iamgenamecolumn);
                            OnPreviewImage(Imagename);

                            currentPreviewcount = id;
                            i = i + 1;
                        }
                    }
                    while (c.MoveToNext());
                }
            }
        }

        protected void OnPreviewImage(string ImageResource_Cam)
        {
            string imagepath = CreateDirectoryForPictures() + "/" + ImageResource_Cam;
            LinearLayout LayoutVoiceEntry = FindViewById<LinearLayout>(Resource.Id.LayoutVoiceEntry);
            LinearLayout LayoutVoicePreview = FindViewById<LinearLayout>(Resource.Id.LayoutVoicePreview);
            ImageView VoiceCamImagePreview = FindViewById<ImageView>(Resource.Id.VoiceCamImagePreview);
            LayoutVoiceEntry.Visibility = ViewStates.Gone;
            LayoutVoicePreview.Visibility = ViewStates.Visible;

            if (System.IO.File.Exists(imagepath))
            {
                Java.IO.File imageFile = new Java.IO.File(imagepath);
                Bitmap bitmap = BitmapFactory.DecodeFile(imageFile.AbsolutePath);

                VoiceCamImagePreview.SetImageBitmap(null);
                VoiceCamImagePreview.SetImageBitmap(bitmap);
                bitmap.Dispose();
            }
        }
        private void PopulatePreviousImages(String ImageName, int ImageID)
        {
            if (ImageName != "")
            {
                String ImagePath = CreateDirectoryForPictures() + "/" + ImageName;

                //int child = llVoiceImageView.ChildCount;
                //View v = llVoiceImageView.GetChildAt (0);
                LinearLayout LL1 = new LinearLayout(this);
                Android.Widget.LinearLayout.LayoutParams LLParams = new Android.Widget.LinearLayout.LayoutParams(Android.Widget.LinearLayout.LayoutParams.WrapContent, Android.Widget.LinearLayout.LayoutParams.WrapContent);
                LLParams.SetMargins(10, 0, 0, 0);
                LLParams.Gravity = GravityFlags.Center;

                LL1.SetPadding(10, 10, 10, 10);
                LL1.Orientation = Android.Widget.Orientation.Vertical;


                LL1.LayoutParameters = LLParams;
                LinearLayout LL2 = new LinearLayout(this);

                LLParams = new Android.Widget.LinearLayout.LayoutParams(Android.Widget.LinearLayout.LayoutParams.MatchParent, Android.Widget.LinearLayout.LayoutParams.MatchParent);
                LLParams.Gravity = GravityFlags.Center;
                LL2.Orientation = Android.Widget.Orientation.Vertical;
                LL2.SetBackgroundColor(Color.White);
                string previousimagename = "";
                ImageView imgcamera = new ImageView(this);
                imgcamera.Id = ImageID;
                imgcamera.Tag = ImageName;
                imgcamera.SetBackgroundResource(Resource.Drawable.icon_camera2x);
                imgcamera.SetImageBitmap(GetResizeBitmap(ImagePath, 120, 120, true));
                //View vw = new View (this);
                //vw.LayoutParameters = LLParams;
                LL2.LayoutParameters = LLParams;
                LL2.AddView(imgcamera);
                //LL1.AddView (vw);
                LL1.AddView(LL2);
                LinearLayout LL3 = new LinearLayout(this);
                LLParams = new Android.Widget.LinearLayout.LayoutParams(Android.Widget.LinearLayout.LayoutParams.MatchParent, Android.Widget.LinearLayout.LayoutParams.MatchParent);
                LL3.SetBackgroundColor(Color.White);
                LLParams.Gravity = GravityFlags.Center;
                LL3.Orientation = Android.Widget.Orientation.Vertical;
                LL3.LayoutParameters = LLParams;
                TextView txtpicname = new TextView(this);
                txtpicname.SetBackgroundColor(Color.ParseColor("#cfd9db"));
                txtpicname.SetTextColor(Color.Black);
                LLParams = new Android.Widget.LinearLayout.LayoutParams(Android.Widget.LinearLayout.LayoutParams.MatchParent, Android.Widget.LinearLayout.LayoutParams.WrapContent);
                LLParams.Gravity = GravityFlags.Center;
                txtpicname.LayoutParameters = LLParams;
                string randomnumberfromimage = Regex.Replace(ImageName, "[^0-9]+", string.Empty);
                txtpicname.SetText(randomnumberfromimage, TextView.BufferType.Editable);

                LL3.AddView(txtpicname);
                LL2.AddView(LL3);

                llVoiceMultipleImageView.AddView(LL1);
                images = images + "|" + ImageName;

                imgcamera.Click += getview;
            }

        }
        private void PopulatePreviousFiles(ICursor c)
        {
            TextView lblPlayingCount = FindViewById<TextView>(Resource.Id.lblPlayingCount);
            imgPausePlayer = FindViewById<ImageView>(Resource.Id.imgPausePlayer);
            imgPlayPlayer = FindViewById<ImageView>(Resource.Id.imgPlayPlayer);
            images = "";
            db.ExecSQL("DROP TABLE IF EXISTS " + "tbl_VoiceImages");
            //db.ExecSQL("delete from " + "tbl_VoiceImages");
            db.ExecSQL("CREATE TABLE IF NOT EXISTS " + "tbl_VoiceImages"
                + " (ID INTEGER PRIMARY KEY AUTOINCREMENT,  ImageID INTEGER, ImageName VARCHAR);");
            int imagenamecolumn = c.GetColumnIndex("Image1");
            int audioamecolumn = c.GetColumnIndex("AudioFileName");
            int ImageID = 1;
            c.MoveToFirst();
            if (c != null)
            {
                // Loop through all Results
                if (c.Count > 0)
                {
                    do
                    {
                        String Imagename = c.GetString(imagenamecolumn);
                        String AudioName = c.GetString(audioamecolumn);
                        RecordAudioNameList = "";

                        if (AudioName != "")
                        {

                            if (AudioName.Contains('|'))
                            {
                                string[] arrAudioame = AudioName.Split('|');
                                foreach (var item in arrAudioame)
                                {
                                    RecordAudioPathList = RecordAudioPathList + "|" + CreateDirectoryForAudio() + "/" + item.ToString();
                                    RecordAudioNameList = RecordAudioNameList + "|" + item.ToString();


                                }
                                RecordAudioNameList = RecordAudioNameList.TrimStart('|').TrimEnd('|');
                                RecordAudioPathList = RecordAudioPathList.TrimStart('|').TrimEnd('|');
                                llplayer.Visibility = ViewStates.Visible;
                                playerseek.Visibility = ViewStates.Visible;
                                imgPausePlayer.Visibility = ViewStates.Gone;
                                imgPlayPlayer.Visibility = ViewStates.Visible;
                            }
                            else
                            {
                                RecordAudioNameList = CreateDirectoryForAudio() + "/" + AudioName.TrimStart('|').TrimEnd('|');
                                RecordAudioPathList = AudioName.TrimStart('|').TrimEnd('|');
                                llplayer.Visibility = ViewStates.Visible;
                                playerseek.Visibility = ViewStates.Visible;
                                imgPausePlayer.Visibility = ViewStates.Gone;
                                imgPlayPlayer.Visibility = ViewStates.Visible;

                            }


                            lblPlayingCount.Text = "Playing 0 of " + RecordAudioPathList.Split('|').Length;


                        }
                        if (Imagename.Contains("|"))
                        {
                            Imagename = Imagename.TrimStart('|').TrimEnd('|');
                            string[] arrImagename = Imagename.Split('|');
                            foreach (var item in arrImagename)
                            {
                                db.ExecSQL("INSERT INTO "
                                + "tbl_VoiceImages"
                                + " (ImageName, ImageID)"
                                + " VALUES ('" + item.ToString() + "','" + ImageID + "')");

                                PopulatePreviousImages(item.ToString(), ImageID);
                                multipleimagebuttonclickcount = ImageID;
                                ImageID = ImageID + 1;

                            }


                        }
                        else
                        {
                            db.ExecSQL("INSERT INTO "
                            + "tbl_VoiceImages"
                            + " (ImageName, ImageID)"
                            + " VALUES ('" + Imagename + "','" + ImageID + "')");


                            ImageID = ImageID + 1;

                        }
                    } while (c.MoveToNext());
                }

            }


        }
        private void SaveDataToLocalDataBase(bool autosave)
        {
            if (images != "")
            {
                images = images.TrimStart('|').TrimEnd('|');
                string path = CreateDirectoryForPictures();
                string audiopath = CreateDirectoryForAudio();
                db = this.OpenOrCreateDatabase("ImInventory", FileCreationMode.Private, null);

                //EditText img1path = FindViewById<EditText> (Resource.Id.voicecam1path);
                ICursor c = db.RawQuery("SELECT * FROM " + "tbl_VoiceImages ", null);
                int iamgenamecolumn = c.GetColumnIndex("ImageName");
				//   images = "";
				String [] arrimages = null;
				string distinctimagename = "";
                c.MoveToFirst();
	                if (c.Count > 0)
	                {
	                    if (c != null)
	                    {
	                        // Loop through all Results
	                        do
	                        {
	                            String Imagename = c.GetString(iamgenamecolumn);
	                            images = images + "|" + Imagename;
							arrimages = images.Split ('|');
							foreach (var item in arrimages) {
								if (!distinctimagename.Contains (item.ToString ())) {
									distinctimagename = distinctimagename + "|" + item.ToString();

								}
							}
							images = distinctimagename;


	                        } while (c.MoveToNext());
	                    }
	                }
            }

            EditText txtproname = FindViewById<EditText>(Resource.Id.txtvoiceprojectname);
            EditText txtclientname = FindViewById<EditText>(Resource.Id.txtvoiceclientname);
            EditText txtlocation = FindViewById<EditText>(Resource.Id.txtvoicelocation);
            string recordedaudiofilenames = "";
            if (RecordAudioPathList != "" && RecordAudioPathList != null)
            {
                RecordAudioPathList = RecordAudioPathList.TrimStart('|').TrimEnd('|');
                RecordAudioNameList = RecordAudioNameList.TrimStart('|').TrimEnd('|');
                string[] ArrRecordAudioPath = RecordAudioNameList.Split('|');

                foreach (var item in ArrRecordAudioPath)
                {
                    if (item.ToString().Contains("/"))
                    {

                        string path = item.Split('/')[item.Split('/').Length - 1].ToString();
                        recordedaudiofilenames = recordedaudiofilenames + "|" + path;
                    }
                    else
                    {
                        recordedaudiofilenames = recordedaudiofilenames + "|" + item.ToString();
                    }
                }
                recordedaudiofilenames = recordedaudiofilenames.TrimStart('|');

            }
            string projectname = txtproname.Text;
            string clientname = txtclientname.Text;
            string location = txtlocation.Text;
            string dtnow = DateTime.Now.ToString("MM-dd-yyyy HH:mm");

            //db = this.OpenOrCreateDatabase ("ImInventory", FileCreationMode.Private, null);
            //db.ExecSQL ("DROP TABLE IF EXISTS " + "tbl_Inventory");
           db.ExecSQL("delete from " + "tbl_Inventory where Location='" + locationname + "' AND InventoryType='" + 2 + "'");
            db.ExecSQL("CREATE TABLE IF NOT EXISTS "
                + "tbl_Inventory"
                + " (ID INTEGER PRIMARY KEY AUTOINCREMENT,EmpID INTEGER,ProjectID VARCHAR,ProjectName VARCHAR, ClientName VARCHAR,Location VARCHAR, Image1 VARCHAR , Image2 VARCHAR, Image3 VARCHAR, Image4 VARCHAR," +
                "ItemDescription VARCHAR, Brand VARCHAR, Quantity VARCHAR, ModelNumber VARCHAR, UnitCost VARCHAR, Notes VARCHAR , Addeddate VARCHAR, AudioFileName VARCHAR,BarCodeNumber VARCHAR,InventoryType VARCHAR" +
                "" +
                "" +
                ");");

            if (location.Trim() != "")
            {
                ContentValues values = new ContentValues(); values.Put("EmpID", empid);
                values.Put("ProjectID", projectid);
                values.Put("ProjectName", projectname);
                values.Put("ClientName", clientname);
                values.Put("Location", location);
                values.Put("AudioFileName", recordedaudiofilenames);
                values.Put("BarCodeNumber", "");
                values.Put("Image1", images);
                values.Put("Image2", "");
                values.Put("Image3", "");
                values.Put("Image4", "");
                values.Put("ItemDescription", "");
                values.Put("Brand", "");
                values.Put("Quantity", "");
                values.Put("ModelNumber", "");
                values.Put("UnitCost", "");
                values.Put("Notes", "");
                values.Put("InventoryType", "2");
                values.Put("Addeddate", dtnow);
                db.Insert("tbl_Inventory", null, values);
                //db.ExecSQL ("INSERT INTO "
                //+ "tbl_Inventory"
                //+ " (EmpID,ProjectID,ProjectName, ClientName,Location,Image1,Image2,Image3,Image4,ItemDescription,Brand,Quantity,ModelNumber,UnitCost,Notes,Addeddate,AudioFileName,BarCodeNumber,InventoryType)"
                //+ " VALUES ('" + empid + "','" + projectid + "','" + projectname + "','" + clientname + "','" + location + "','" + images + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + dtnow + "','" + recordedaudiofilenames + "','" + "" + "','2')");
            }


            LinearLayout llVoiceImageView = FindViewById<LinearLayout>(Resource.Id.llVoiceImageView);
            //llVoiceMultipleImageView=FindViewById<LinearLayout> (Resource.Id.llVoiceImageView);



            multipleimagebuttonclickcount = 0;
            prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            editor = prefs.Edit();
            editor.PutInt("FromVoiceEdit", 0);
            editor.Commit();

            if (!autosave)
            {
                llVoiceImageView.RemoveAllViews();
                if (location.Trim() != "")
                {
                    Toast.MakeText(this, "Data saved successfully", ToastLength.Long).Show();
                }
                else
                {
                    Toast.MakeText(this, "Please enter location name", ToastLength.Long).Show();
                }
            }
        }

        public override void OnBackPressed()
        {

        }

        private void RegisterMediaButton()
        {
            //mAudioManager = (AudioManager)GetSystemService(Context.AudioService);
            //MyMediaButtonBroadcastReceiver obj = new MyMediaButtonBroadcastReceiver ();
            //mRemoteControlResponder = new ComponentName(this.PackageName,obj.Class.Name);

        }


        private void record()
        {

            TextView lblPlayingCount = FindViewById<TextView>(Resource.Id.lblPlayingCount);

            lblPlayingCount.Visibility = ViewStates.Gone;


        }


        protected override void OnResume()
        {
            base.OnResume();
            //mAudioManager.RegisterMediaButtonEventReceiver (mRemoteControlResponder);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            //mAudioManager.UnregisterMediaButtonEventReceiver (mRemoteControlResponder);
        }

        private void OpenSubscriptionAlert()
        {
            GetPlanDetailsByPlanID();
            WifiManager wifiManager = (WifiManager)GetSystemService(Context.WifiService);
            WifiInfo wInfo = wifiManager.ConnectionInfo;
            String MACAdress = Android.Provider.Settings.Secure.GetString(ContentResolver, Android.Provider.Settings.Secure.AndroidId);
            string encryptedplanname = URLEncoder.Encode(Encrypt(plan), "UTF-8");
            string encryptedamount = URLEncoder.Encode(Encrypt(amount), "UTF-8");
            string encryptedplanid = URLEncoder.Encode(Encrypt(planid), "UTF-8");
            string encrypteduserid = URLEncoder.Encode(Encrypt(userid), "UTF-8");
            string encryptedsubscriptionid = URLEncoder.Encode(Encrypt(subscriptionid), "UTF-8");
            string encryptedmacadress = URLEncoder.Encode(Encrypt(MACAdress), "UTF-8");
			GeneralValues objGeneralValues = new GeneralValues ();
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetMessage("To access this feature you will need to upgrade your subscription to platinum.Would you like to upgrade to the platinum plan?");
            builder.SetCancelable(false);
            builder.SetPositiveButton("Yes", (object sender, DialogClickEventArgs e) =>
            {
					Intent browserintent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(objGeneralValues.PaymentURL+"?planname=" + encryptedplanname + "&amount=" + encryptedamount + "&planid=" + encryptedplanid + "&user=" + encrypteduserid + "&subscriptionid=" + encryptedsubscriptionid + "&macaddress=" + encryptedmacadress));
                StartActivityForResult(browserintent, 10);
            });
            builder.SetNegativeButton("No", (object sender, DialogClickEventArgs e) =>
            {

            });
            AlertDialog alertdialog = builder.Create();
            alertdialog.Show();
        }

        private string Encrypt(string clearText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        private void GetPlanDetailsByPlanID()
        {
            GeneralValues objGeneralValues = new GeneralValues();
            var webrequest = (HttpWebRequest)WebRequest.Create("https://api.stripe.com/v1/plans/" + objGeneralValues.PlatinumPlanID);
            webrequest.Method = "POST";
            webrequest.Headers.Add("Authorization", "Bearer " + objGeneralValues.SecretKey);
            try
            {
                HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
                StreamReader responseStream = new StreamReader(webresponse.GetResponseStream());
                string obj = responseStream.ReadToEnd();
                var parsedjson = JObject.Parse(obj);
                plan = parsedjson["name"].ToString();
                planid = parsedjson["id"].ToString();
                amount = parsedjson["amount"].ToString();
                userid = empid.ToString();
            }
            catch
            {
            }
        }

    }
}

