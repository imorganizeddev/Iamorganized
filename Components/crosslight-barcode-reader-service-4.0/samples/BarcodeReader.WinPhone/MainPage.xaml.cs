using BarcodeReader.ViewModels;
using Intersoft.Crosslight;
using Intersoft.Crosslight.Services.Barcode;
using Intersoft.Crosslight.Services.Barcode.WinPhone;
using System;
using System.Windows;

namespace BarcodeReader.WinPhone
{
    [ViewModelType(typeof(SimpleViewModel))]
    public partial class MainPage
    {
        MobileBarcodeScanner scanner;

        #region Constructors

        public MainPage()
        {
            InitializeComponent();
            scanner = new MobileBarcodeScanner(this.Dispatcher);
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        #endregion
    }
}