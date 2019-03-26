using Intersoft.Crosslight;
using Intersoft.Crosslight.Input;
using Intersoft.Crosslight.ViewModels;
using System.Threading;
using System.Threading.Tasks;
using Intersoft.Crosslight.Services.Barcode;
using System.Collections.Generic;

namespace BarcodeReader.ViewModels
{
    public class SimpleViewModel : ViewModelBase
    {
        #region Fields

        private string _footerText;
        private string _greetingText;
        private string _newText;

        #endregion

        #region Properties

        public string FooterText
        {
            get { return _footerText; }
            set
            {
                if (_footerText != value)
                {
                    _footerText = value;
                    OnPropertyChanged("FooterText");
                }
            }
        }

        public string GreetingText
        {
            get { return _greetingText; }
            set
            {
                if (_greetingText != value)
                {
                    _greetingText = value;
                    OnPropertyChanged("GreetingText");
                }
            }
        }

        public string NewText
        {
            get { return _newText; }
            set
            {
                if (_newText != value)
                {
                    _newText = value;
                    OnPropertyChanged("NewText");
                }
            }
        }

        public DelegateCommand ShowToastCommand { get; set; }

        #endregion

        #region Constructors

        public SimpleViewModel()
        {
            IApplicationContext context = this.GetService<IApplicationService>().GetContext();
            if (context.Platform.OperatingSystem == OSKind.Android)
                this.GreetingText = "Hello Android from Crosslight!";
            else if (context.Platform.OperatingSystem == OSKind.WinPhone)
                this.GreetingText = "Hello WinPhone from Crosslight!";
            else if (context.Platform.OperatingSystem == OSKind.WinRT)
                this.GreetingText = "Hello WinRT from Crosslight!";
            else if (context.Platform.OperatingSystem == OSKind.iOS)
                this.GreetingText = "Hello iOS from Crosslight!";

            this.FooterText = "Powered by Crosslight®";
            this.ShowToastCommand = new DelegateCommand(ShowToast);
        }

        #endregion

        #region Methods

        private async void ShowToast(object parameter)
        {
            BarcodeFormat format = new BarcodeFormat();
            format = BarcodeFormat.QRCode | BarcodeFormat.Code93 | BarcodeFormat.Pdf417;

            MobileBarcodeScanningOptions options = new MobileBarcodeScanningOptions();
            options.PossibleFormats.Clear();
            options.PossibleFormats.Add(format);
            options.DelayBetweenAnalyzingFrames = 200;

            IBarcodeReaderService service = ServiceProvider.GetService<IBarcodeReaderService>();
            service.SetOwner(this);

            Task<string> result = service.Scan();
           // Task<string> result = service.Scan(options);
           // Task<string> result = service.Scan(format);   
           // Task<string> result = service.Scan("Header Text","Footer Text",format);

            this.GreetingText = await result;
        }

        #endregion
    }
}