using Android.App;
using Android.Webkit;
using Android.Widget;
using Android.Views;
using Android.OS;
using System.Net;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Android.Content;
using Android.Preferences;
using Java.IO;
using System;
using System.Collections.Generic;
using System.Threading;
using Android.Database.Sqlite;
using Android.Database;
using IMInventory.iminventory;
using System.Threading.Tasks;
using Android.Runtime;
using Xamarin.Auth;

namespace IMInventory
{
    [Activity(Label = "WebView")]
    public class GoogleDriveWebview : Activity
    {

        bool successmultipleimageupload = false;
        bool successsingleimageupload = false;
        bool successaudioupload = false;
        string username = "";
       
        protected override void OnCreate(Bundle savedInstanceState)
        {


            ISharedPreferences prefs;
            ISharedPreferencesEditor editor;
            Activity activity;
            string capturedimagepath = "";

            base.OnCreate(savedInstanceState);
            this.RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.GoogleDriveWebView);
            prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            capturedimagepath = prefs.GetString("capturedimagepath", "");
            username = prefs.GetString("Name", "");
            IMApplication.username = username;
            string url = "https://accounts.google.com/o/oauth2/v2/auth?scope=https://www.googleapis.com/auth/drive&https://www.googleapis.com/auth/drive.file&redirect_uri=http://localhost:61424/Default.aspx?accountid=500&response_type=code&client_id=327705258572-jrfoil2gvejn6rjj6ht49rqml34cf71s.apps.googleusercontent.com&suppress_webview_warning=true";

            //Android.Webkit.WebView wvView = FindViewById<Android.Webkit.WebView>(Resource.Id.wvGoogleSignIn);
            //wvView.Visibility = ViewStates.Visible;
            //wvView.LoadUrl(url);
            //wvView.Settings.JavaScriptEnabled = true;
            //wvView.SetWebViewClient(new MyWebViewClient(wvView, this));

            Intent browserintent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(url));
            
            StartActivityForResult(browserintent, 1);





            // Create your application here
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
        }

        public void CreatePostRequestForAuthorization(Android.Webkit.WebView view, string code, Activity act, ProgressDialog prgdialog)
        {


            var request = (HttpWebRequest)WebRequest.Create("https://www.googleapis.com/oauth2/v4/token");
            var postData = "code=" + code;
            postData += "&client_id=327705258572-jrfoil2gvejn6rjj6ht49rqml34cf71s.apps.googleusercontent.com";

            postData += "&redirect_uri=http://localhost:9002";
            postData += "&grant_type=authorization_code";
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            string path = CreateDirectoryForPictures();
            string audiopath = CreateDirectoryForAudio();
            MyWebViewClient.access dynObj = JsonConvert.DeserializeObject<MyWebViewClient.access>(responseString);

            SQLiteDatabase db = IMApplication.imdb;
            IMApplication.accesstoken = dynObj.access_token;
            ICursor c = db.RawQuery("SELECT * FROM " + "tbl_Inventory WHERE EmpID = " + IMApplication.empid + " AND ProjectID=" + IMApplication.projectid + " ORDER BY  ID  ", null);
            ISharedPreferencesEditor editor = IMApplication.pref.Edit();
            int projectnamecolumn = c.GetColumnIndex("ProjectName");
            int image1namecolumn = c.GetColumnIndex("Image1");
            int image2namecolumn = c.GetColumnIndex("Image2");
            int image3namecolumn = c.GetColumnIndex("Image3");
            int image4namecolumn = c.GetColumnIndex("Image4");
            int audionamecolumn = c.GetColumnIndex("AudioFileName");
            int locationnamecolumn = c.GetColumnIndex("Location");

            string folderid = "";
            string foldername = "";
            int success = 0;
            if (c.Count > 0)
            {

                c.MoveToFirst();

                do
                {
                    string projectname = c.GetString(projectnamecolumn);
                    IMApplication.projectname = projectname;
                    if (foldername == "")
                    {

                        foldername = projectname + "_" + DateTime.Now.ToString("MM-dd-yyyy") + "_" + IMApplication.username + "_" + DateTime.Now.ToString("hh-mm-ss");
                        //foldername=projectname+"_"+DateTime.Now.ToString("MM-dd-yyyy_hh-mm-ss");
                        do
                        {

                            string parentfolderid = CreateParentFolder(dynObj.access_token);
                            folderid = CreateFolder(dynObj.access_token, foldername, parentfolderid);
                            if (folderid != "")
                            {
                                success = 1;

                            }
                        } while (success == 0);
                        success = 0;

                    }

                    byte[] bytearr = null;
                    String Image1name = c.GetString(image1namecolumn);
                    String Image2name = c.GetString(image2namecolumn);
                    String Image3name = c.GetString(image3namecolumn);
                    String Image4name = c.GetString(image4namecolumn);
                    String AudioFileName = c.GetString(audionamecolumn);
                    string Locationname = c.GetString(locationnamecolumn);

                    if (!Image1name.Contains("|"))
                    {
                        successmultipleimageupload = true;

                    }
                    if (AudioFileName == "")
                    {

                        successaudioupload = true;
                    }

                    if (Image1name.Contains("|"))
                    {

                        string[] imagearr = Image1name.TrimEnd('|').TrimStart('|').Split('|');
                        Image1name = "";
                        foreach (var item in imagearr)
                        {
                            if (item.Trim() != "")
                            {

                                bytearr = getByteArrayFromfile(path + "/" + item.ToString());

                                do
                                {
                                    success = uploadfiletodrive(bytearr, dynObj.access_token, item.ToString(), folderid);



                                } while (success == 0);

                                success = 0;
                            }

                        }
                        successmultipleimageupload = true;
                    }


                    if (AudioFileName != "")
                    {

                        if (AudioFileName.Contains("|"))
                        {
                            string[] ArrAudioFileName = AudioFileName.Split('|');
                            string newfilename = Locationname + "_" + projectname + ".mp3";
                            var list = new List<byte>();
                            int audiofilenumber = 0;

                            int filecount = 0;
                            WebService objWebService = new WebService();
                            foreach (var item in ArrAudioFileName)
                            {

                                bytearr = getByteArrayFromfile(audiopath + "/" + item.ToString());

                                objWebService.UploadFileMultipleAsync(bytearr, audiofilenumber + "_" + item.ToString(), newfilename);



                                audiofilenumber = audiofilenumber + 1;

                            }

                            objWebService.UploadFileMultipleCompleted += delegate (object sender, UploadFileMultipleCompletedEventArgs ev)
                          {

                              filecount = filecount + 1;
                              if (filecount == ArrAudioFileName.Length)
                              {
                                  objWebService.MargeAudioForGDriveAsync(newfilename);
                                  objWebService.MargeAudioForGDriveCompleted += delegate (object sender1, MargeAudioForGDriveCompletedEventArgs ev1)
                                   {
                                      do
                                      {
                                          success = uploadfiletodrive(ev1.Result, dynObj.access_token, newfilename, folderid);

                                      } while (success == 0);
                                      success = 0;
                                      successaudioupload = true;
                                  };

                              }
                          };
                        }

                        else
                        {
                            string newfilename = Locationname + "_" + projectname + ".mp3";
                            bytearr = getByteArrayFromfile(audiopath + "/" + AudioFileName);
                            WebService objWebService = new WebService();
                            objWebService.UploadFileMultipleAsync(bytearr, AudioFileName, newfilename);
                            objWebService.UploadFileMultipleCompleted += delegate (object sender, UploadFileMultipleCompletedEventArgs ev)
                          {
                              objWebService.MargeAudioForGDriveAsync(newfilename);
                              objWebService.MargeAudioForGDriveCompleted += delegate (object sender1, MargeAudioForGDriveCompletedEventArgs ev1)
                               {
                                      do
                                      {
                                          success = uploadfiletodrive(ev1.Result, dynObj.access_token, newfilename, folderid);

                                      } while (success == 0);
                                      success = 0;
                                      successaudioupload = true;
                                  };
                          };

                        }

                    }
                    else
                    {

                        if (Image1name != "")
                        {
                            Image1name = Image1name.TrimEnd('|').TrimStart('|');

                            bytearr = getByteArrayFromfile(path + "/" + Image1name);


                            do
                            {
                                success = uploadfiletodrive(bytearr, dynObj.access_token, Image1name, folderid);



                            } while (success == 0);
                            success = 0;

                        }

                        if (Image2name != "")
                        {
                            bytearr = getByteArrayFromfile(path + "/" + Image2name);

                            do
                            {
                                success = uploadfiletodrive(bytearr, dynObj.access_token, Image2name, folderid);



                            } while (success == 0);
                            success = 0;
                        }

                        if (Image3name != "")
                        {

                            bytearr = getByteArrayFromfile(path + "/" + Image3name);


                            do
                            {
                                success = uploadfiletodrive(bytearr, dynObj.access_token, Image3name, folderid);



                            } while (success == 0);
                            success = 0;

                        }

                        if (Image4name != "")
                        {

                            bytearr = getByteArrayFromfile(path + "/" + Image4name);

                            do
                            {
                                success = uploadfiletodrive(bytearr, dynObj.access_token, Image4name, folderid);



                            } while (success == 0);
                            success = 0;


                        }
                        successsingleimageupload = true;


                    }

                } while (c.MoveToNext());
            }
            WebService objwebservice = new WebService();
            objwebservice.SaveManualforGDriveAsync(IMApplication.xml, "https://drive.google.com/drive/folders/" + IMApplication.folderid);

            objwebservice.SaveManualforGDriveCompleted += delegate (object sender, SaveManualforGDriveCompletedEventArgs ev)
            {




                string filename = IMApplication.projectname + "_" + DateTime.Now.ToString("dd_MM_yyyy") + "_" + DateTime.Now.Ticks.ToString() + ".xls";

                uploadxlsfiletodrive(ev.Result, IMApplication.accesstoken, filename, IMApplication.folderid, act);

                //act.Finish ();

                //wvview.Visibility = ViewStates.Invisible;
                try
                {
                    //Toast.MakeText (act, "Upload sucessful", ToastLength.Short).Show ();

                }
                catch (Exception ex)
                {


                }
                //prgdialog.Dismiss ();

            };
        }



        private string CreateParentFolder(string accesstoken)
        {

            string folderid = "";
            try
            {

                var request = (HttpWebRequest)WebRequest.Create("https://www.googleapis.com/drive/v2/files");
                List<string> _postData = new List<string>();

                string postData = string.Join(" ", _postData.ToArray());
                byte[] MetaDataByteArray = Encoding.UTF8.GetBytes(postData);
                request.Method = "GET";
                request.ContentType = "application/json";
                request.Headers.Add("Authorization", "Bearer " + accesstoken);
                //using (var stream = request.GetRequestStream ()) {
                //stream.Write (MetaDataByteArray, 0, MetaDataByteArray.Length);
                //}
                var response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                MyWebViewClient.ParentFolder objDriveFolder = JsonConvert.DeserializeObject<MyWebViewClient.ParentFolder>(responseString);
                //folderid = objDriveFolder.id;
                //IMApplication.folderurl = objDriveFolder.alternateLink;
                //IMApplication.folderid = folderid;

                foreach (var item in objDriveFolder.items)
                {

                    if (item.title == "IM Organized" && item.labels.trashed == false)
                    {
                        folderid = item.id;

                    }

                }




                if (folderid == "")
                {

                    try
                    {

                        var request1 = (HttpWebRequest)WebRequest.Create("https://www.googleapis.com/drive/v2/files");
                        List<string> _postData1 = new List<string>();
                        _postData1.Add("{");
                        _postData1.Add("\"title\": \"" + "IM Organized" + "\",");
                        _postData1.Add("\"mimeType\": \"" + "application/vnd.google-apps.folder" + "\"");

                        _postData1.Add("}");
                        string postData1 = string.Join(" ", _postData1.ToArray());
                        byte[] MetaDataByteArray1 = Encoding.UTF8.GetBytes(postData1);
                        request1.Method = "POST";
                        request1.ContentType = "application/json";
                        request1.Headers.Add("Authorization", "Bearer " + accesstoken);
                        using (var stream = request1.GetRequestStream())
                        {
                            stream.Write(MetaDataByteArray1, 0, MetaDataByteArray1.Length);
                        }

                        var response1 = (HttpWebResponse)request1.GetResponse();

                        var responseString1 = new StreamReader(response1.GetResponseStream()).ReadToEnd();
                        MyWebViewClient.drivefolder objDriveFolder1 = JsonConvert.DeserializeObject<MyWebViewClient.drivefolder>(responseString1);
                        folderid = objDriveFolder1.id;


                    }
                    catch (Exception ex)
                    {
                    }
                }




            }
            catch (Exception ex)
            {

            }

            return folderid;
        }

        private string CreateFolder(string accesstoken, string title, string parentfolderid)
        {

            string folderid = "";
            try
            {

                var request = (HttpWebRequest)WebRequest.Create("https://www.googleapis.com/drive/v2/files");
                List<string> _postData = new List<string>();
                _postData.Add("{");
                _postData.Add("\"title\": \"" + title + "\",");
                _postData.Add("\"parents\": [{\"id\":\"" + parentfolderid + "\"}],");
                _postData.Add("\"mimeType\": \"" + "application/vnd.google-apps.folder" + "\"");

                _postData.Add("}");
                string postData = string.Join(" ", _postData.ToArray());
                byte[] MetaDataByteArray = Encoding.UTF8.GetBytes(postData);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("Authorization", "Bearer " + accesstoken);
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(MetaDataByteArray, 0, MetaDataByteArray.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                MyWebViewClient.drivefolder objDriveFolder = JsonConvert.DeserializeObject<MyWebViewClient.drivefolder>(responseString);
                folderid = objDriveFolder.id;
                IMApplication.folderurl = objDriveFolder.alternateLink;
                IMApplication.folderid = folderid;

            }
            catch (Exception ex)
            {
            }

            return folderid;
        }



        private int uploadfiletodrive(byte[] imagebyte, string accesstoken, string imagename, string folderid)
        {
            int success = 1;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://www.googleapis.com/upload/drive/v2/files?&uploadType=resumable");
                string footer = "\r\n";
                List<string> _postData = new List<string>();
                _postData.Add("{");
                _postData.Add("\"title\": \"" + imagename + "\",");
                _postData.Add("\"parents\": [{\"id\":\"" + folderid + "\"}]");
                _postData.Add("}");
                string postData = string.Join(" ", _postData.ToArray());
                byte[] MetaDataByteArray = Encoding.UTF8.GetBytes(postData);
                int headerLenght = MetaDataByteArray.Length + footer.Length;

                request.Method = "POST";
                request.ContentType = "application/json; charset=UTF-8";
                request.ContentLength = headerLenght;
                request.Headers.Add("Authorization", "Bearer " + accesstoken);
                request.Headers.Add("X-Upload-Content-Type", "image/jpeg");
                request.Headers.Add("X-Upload-Content-Length", imagebyte.Length.ToString());
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(MetaDataByteArray, 0, MetaDataByteArray.Length); // write the MetaData
                dataStream.Write(Encoding.UTF8.GetBytes(footer), 0, Encoding.UTF8.GetByteCount(footer));  // done writeing add return just
                dataStream.Close();

                var response = (HttpWebResponse)request.GetResponse();
                string Location = response.Headers["Location"];


                request = (HttpWebRequest)WebRequest.Create(Location);

                request.Method = "POST";
                request.ContentType = "image/jpeg";
                request.ContentLength = imagebyte.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(imagebyte, 0, imagebyte.Length);
                }

                response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            }
            catch
            {
                success = 0;
            }

            return success;
        }

        private int uploadxlsfiletodrive(byte[] imagebyte, string accesstoken, string imagename, string folderid, Activity act)
        {
            int success = 1;
            //Thread.Sleep (1000);

            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://www.googleapis.com/upload/drive/v2/files?&uploadType=resumable");
                string footer = "\r\n";
                List<string> _postData = new List<string>();
                _postData.Add("{");
                _postData.Add("\"title\": \"" + imagename + "\",");
                _postData.Add("\"parents\": [{\"id\":\"" + folderid + "\"}]");
                _postData.Add("}");
                string postData = string.Join(" ", _postData.ToArray());
                byte[] MetaDataByteArray = Encoding.UTF8.GetBytes(postData);
                int headerLenght = MetaDataByteArray.Length + footer.Length;

                request.Method = "POST";
                request.ContentType = "application/json; charset=UTF-8";
                request.ContentLength = headerLenght;
                request.Headers.Add("Authorization", "Bearer " + accesstoken);
                request.Headers.Add("X-Upload-Content-Type", "application/vnd.ms-excel");
                request.Headers.Add("X-Upload-Content-Length", imagebyte.Length.ToString());
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(MetaDataByteArray, 0, MetaDataByteArray.Length); // write the MetaData
                dataStream.Write(Encoding.UTF8.GetBytes(footer), 0, Encoding.UTF8.GetByteCount(footer));  // done writeing add return just
                dataStream.Close();

                var response = (HttpWebResponse)request.GetResponse();
                string Location = response.Headers["Location"];


                request = (HttpWebRequest)WebRequest.Create(Location);

                request.Method = "POST";
                request.ContentType = "image/jpeg";
                request.ContentLength = imagebyte.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(imagebyte, 0, imagebyte.Length);
                }

                response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();


            }
            catch (Exception ex)
            {

                success = 0;
            }

            Toast.MakeText(act, "Upload sucessful", ToastLength.Short).Show();
            act.Finish();
            return success;
        }

        private int deletefiletodrive(byte[] imagebyte, string accesstoken, string imagename, string folderid)
        {

            return 0;
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


        private string CreateDirectoryForAudio()
        {
            String path = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryMusic).ToString() + "/ImInventory";
            if (!Directory.Exists(path))
            {

                Directory.CreateDirectory(path);

            }
            return path;
        }

        public static byte[] ConvertStreamToByteArray(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
        private byte[] getByteArrayFromfile(String filePath)
        {

            byte[] bytes = null;
            try
            {
                Java.IO.File file = new Java.IO.File(filePath);
                FileInputStream fis = new FileInputStream(file);
                ByteArrayOutputStream bos = new ByteArrayOutputStream();
                byte[] buf = new byte[1024];

                for (int readNum; (readNum = fis.Read(buf)) != -1;)
                {
                    bos.Write(buf, 0, readNum);
                }

                bytes = bos.ToByteArray();
            }
            catch
            {
                return bytes;
            }
            return bytes;
        }

        class MyWebViewClient : WebViewClient
        {
            private Button button;

            private TextView text;
            private Android.Webkit.WebView wvview;
            private Activity act;
            ProgressDialog prgdialog;
            int cancel = 0;


            public MyWebViewClient(Android.Webkit.WebView wview, Activity act)
            {


                this.wvview = wview;
                this.act = act;

            }

            public override void OnPageStarted(WebView view, string url, Android.Graphics.Bitmap favicon)
            {
                if (url.Contains("accounts.google.com/o/oauth2/approval"))
                {

                }
                if (url.Contains("?error"))
                {

                    act.Finish();
                }
            }

            public override async void OnPageFinished(Android.Webkit.WebView view, string url)
            {


                var left = "?code=";
                if (url.Contains(left))
                {

                    try
                    {
                        // Success Code will only be valid for a short period of time
                        var successCode = url.Split('=')[1].ToString().TrimEnd('#');

                        // Refresh Token is permanent, it can be stored an reused later
                        GoogleDriveWebview objWebview = new GoogleDriveWebview();

                        WebService objwebservice = new WebService();
                        view.Visibility = ViewStates.Invisible;
                        int cancel = 0;
                        if (prgdialog == null)
                        {
                            prgdialog = new ProgressDialog(act);

                            prgdialog.SetCanceledOnTouchOutside(false);
                            prgdialog.SetCancelable(false);
                            prgdialog.SetMessage("Uploading");
                            //prgdialog.SetButton ("Cancel Upload", (object buildersender, DialogClickEventArgs eve) => {
                            //cancel = 1;
                            //act.Finish ();


                            //});
                            //prgdialog.Show ();
                            //Thread.Sleep (2000);
                            //Task<int> uploadtodrive = UpdateUi (prgdialog, act, successCode, view);
                            //var intResult = await uploadtodrive;
                            //UpdateUi (prgdialog, act, successCode, view);

                            new Thread(new ThreadStart(delegate
                            {
                                //LOAD METHOD TO GET ACCOUNT INFO
                                act.RunOnUiThread(() =>
                                                  prgdialog.Show());


                                try
                                {
                                    Thread.Sleep(10000);
                                }
                                catch (Exception e)
                                {

                                }

                                act.RunOnUiThread(() =>

                                                  UpdateUi(prgdialog, act, successCode, view, cancel));


                            })).Start();



                            //prgdialog.SetButton ("Cancel Upload", (object buildersender, DialogClickEventArgs eve) => {

                            //act.Finish ();

                            //cancel = 1;
                            //});

                            //prgdialog.SetMessage ("Uploading");
                            //prgdialog.Show ();
                        }



                        //objWebview.CreatePostRequestForAuthorization(view,successCode,act);



                        //success=uploadfiletodrive(ev1.Result,dynObj.access_token,newfilename,folderid);

                        if (cancel == 1)
                        {
                            Toast.MakeText(act, "Upload cancelled by the user", ToastLength.Short).Show();
                            act.Finish();
                        }





                        //act.StartActivity (typeof(LocationList));

                        //}while( objWebview.successaudioupload && objWebview.successmultipleimageupload && objWebview.successsingleimageupload);

                    }
                    catch (Exception ex)
                    {

                    }





                }
                if (url.Contains("?error"))
                {
                    prgdialog.Dismiss();
                    act.Finish();
                }

                base.OnPageFinished(view, url);
            }

            public void UpdateUi(ProgressDialog prgdialog, Activity act, string successCode, Android.Webkit.WebView view, int cancel)
            {
                if (cancel == 0)
                {

                    GoogleDriveWebview objWebview = new GoogleDriveWebview();
                    objWebview.CreatePostRequestForAuthorization(view, successCode, act, prgdialog);
                }

            }

            public class access
            {

                public string access_token { get; set; }
                public string token_type { get; set; }
                public string expires_in { get; set; }
                public string refresh_token { get; set; }
                public string id_token { get; set; }
            }

            public class drivefolder
            {

                public string id { get; set; }

                public string alternateLink { get; set; }

            }
            public class ParentFolder
            {

                public List<Item> items { get; set; }

            }

            public class Item
            {
                public string kind { get; set; }
                public string id { get; set; }
                public string etag { get; set; }
                public string selfLink { get; set; }
                public string webContentLink { get; set; }
                public string alternateLink { get; set; }
                public string iconLink { get; set; }
                public string thumbnailLink { get; set; }
                public string title { get; set; }
                public Labels labels { get; set; }

            }

            public class Labels
            {
                public bool starred { get; set; }
                public bool hidden { get; set; }
                public bool trashed { get; set; }
                public bool restricted { get; set; }
                public bool viewed { get; set; }
            }
        }
    }
}

