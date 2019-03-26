using Android.App;
using BarcodeReader.ViewModels;
using Intersoft.Crosslight;
using Intersoft.Crosslight.Android;
using ZXing;
using Android.Content.PM;
using Android.Views;
using Android.Widget;
using Android.OS;
using BarcodeScanner;

namespace BarcodeReader.Android
{
    [Activity(Label = "Crosslight App", Icon = "@drawable/icon")]
    public class ReaderActivity : Activity<BarcodeReaderViewModel>
    {
        

        #region Constructors

        public ReaderActivity()
            : base(Resource.Layout.MainLayout)
        {
           
        }

        #endregion

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.Scan();
        }

        protected async void Scan()
        {
            //Start scanning
            MobileBarcodeScanners scanner = new MobileBarcodeScanners(this);
            var result = await scanner.Scan();

            HandleScanResult(result);
        }
        void HandleScanResult(ZXing.Result result)
        {
            string msg = "";

            if (result != null && !string.IsNullOrEmpty(result.Text))
                msg = "Found Barcode: " + result.Text;
            else
                msg = "Scanning Canceled!";

            this.RunOnUiThread(() => Toast.MakeText(this, msg, ToastLength.Long).Show());
            this.Finish();
        }
    }
}