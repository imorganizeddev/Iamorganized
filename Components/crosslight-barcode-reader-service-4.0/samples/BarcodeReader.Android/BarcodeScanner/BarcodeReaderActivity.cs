using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.OS;
using Android.Hardware;
using Android.Graphics;

using Android.Content;
using Android.Runtime;
using Android.Widget;

using ZXing;
using Android.Support.V4.App;
using Intersoft.Crosslight.Android;
using BarcodeReader.ViewModels;
using BarcodeReader.Android;

namespace BarcodeScanner
{
    [Activity(Label = "Crosslight App", Icon = "@drawable/icon")]
    class BarcodeReaderActivity : FragmentActivity
    {
        public static event Action<ZXing.Result> _onScanCompleted;
        public static event Action _onCanceled;

        public static event Action _onCancelRequested;
        public static event Action<bool> _onTorchRequested;
        public static event Action _onAutoFocusRequested;

        public static void RequestCancel()
        {
            Action evt = _onCancelRequested;
            if (evt != null)
                evt();
        }

        public static void RequestTorch(bool torchOn)
        {
            Action<bool> evt = _onTorchRequested;
            if (evt != null)
                evt(torchOn);
        }

        public static void RequestAutoFocus()
        {
            Action evt = _onAutoFocusRequested;
            if (evt != null)
                evt();
        }
        
        public static MobileBarcodeScanningOptions ScanningOptions { get; set; }

        BarcodeReaderScannerFragment scannerFragment;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.RequestWindowFeature(WindowFeatures.NoTitle);

            this.Window.AddFlags(WindowManagerFlags.Fullscreen); //to show
            this.Window.AddFlags(WindowManagerFlags.KeepScreenOn); //Don't go to sleep while scanning

            if (ScanningOptions.AutoRotate.HasValue && !ScanningOptions.AutoRotate.Value)
                RequestedOrientation = ScreenOrientation.Nosensor;

            SetContentView(BarcodeReader.Android.Resource.Layout.barcodereaderactivitylayout);

            scannerFragment = new BarcodeReaderScannerFragment(result =>
            {
                var evt = _onScanCompleted;
                if (evt != null)
                    _onScanCompleted(result);

                this.Finish();

            }, ScanningOptions);

            SupportFragmentManager.BeginTransaction()
                .Replace(BarcodeReader.Android.Resource.Id.contentFrame, scannerFragment, "ZXINGFRAGMENT")
                .Commit();

            _onCancelRequested += HandleCancelScan;
            _onAutoFocusRequested += HandleAutoFocus;
            _onTorchRequested += HandleTorchRequested;

        }

        void HandleTorchRequested(bool on)
        {
            this.SetTorch(on);
        }

        void HandleAutoFocus()
        {
            this.AutoFocus();
        }

        void HandleCancelScan()
        {
            this.CancelScan();
        }

        protected override void OnDestroy()
        {
            _onCancelRequested -= HandleCancelScan;
            _onAutoFocusRequested -= HandleAutoFocus;
            _onTorchRequested -= HandleTorchRequested;

            base.OnDestroy();
        }

        public void SetTorch(bool on)
        {
            scannerFragment.SetTorch(on);
        }

        public void AutoFocus()
        {
            scannerFragment.AutoFocus();
        }

        public void CancelScan()
        {
            Finish();
            var evt = _onCanceled;
            if (evt != null)
                evt();
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                case Keycode.Back:
                    CancelScan();
                    break;
                case Keycode.Focus:
                    return true;
            }

            return base.OnKeyDown(keyCode, e);
        }
    }
}