using System;
using System.Threading.Tasks;
using Android.Content;
using ZXing;

namespace BarcodeScanner
{

    public class MobileBarcodeScanners : MobileBarcodeScannerBase
    {
        public MobileBarcodeScanners(Context context)
        {
            this.Context = context;
        }

        public Context Context { get; private set; }
        public Android.Views.View CustomOverlay { get; set; }
        //public int CaptureSound { get;set; }

        bool torch = false;

        public override Task<Result> Scan(MobileBarcodeScanningOptions options)
        {
            var task = Task.Factory.StartNew(() =>
            {

                var waitScanResetEvent = new System.Threading.ManualResetEvent(false);

                var scanIntent = new Intent(this.Context, typeof(BarcodeReaderActivity));

               // BarcodeReaderActivity.UseCustomView = this.UseCustomOverlay;
               // BarcodeReaderActivity.CustomOverlayView = this.CustomOverlay;
                BarcodeReaderActivity.ScanningOptions = options;
               // BarcodeReaderActivity.TopText = TopText;
               // BarcodeReaderActivity.BottomText = BottomText;

                Result scanResult = null;

                BarcodeReaderActivity._onCanceled += () =>
                {
                    waitScanResetEvent.Set();
                };

                BarcodeReaderActivity._onScanCompleted += (Result result) =>
                {
                    scanResult = result;
                    waitScanResetEvent.Set();
                };

                this.Context.StartActivity(scanIntent);

                waitScanResetEvent.WaitOne();

                return scanResult;
            });

            return task;
        }

        public override void Cancel()
        {
            BarcodeReaderActivity.RequestCancel();
        }

        public override void AutoFocus()
        {
            BarcodeReaderActivity.RequestAutoFocus();
        }

        public override void Torch(bool on)
        {
            torch = on;
            BarcodeReaderActivity.RequestTorch(on);
        }

        public override void ToggleTorch()
        {
            Torch(!torch);
        }

        public override bool IsTorchOn
        {
            get
            {
                return torch;
            }
        }

    }

}