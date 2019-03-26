using Intersoft.Crosslight;
using Intersoft.Crosslight.Input;
using Intersoft.Crosslight.ViewModels;

namespace BarcodeReader.ViewModels
{
    public class BarcodeReaderViewModel : ViewModelBase
    {
        #region Properties
        private IBarcodeReaderService BarcodeReaderService
        {
            get { return this.GetService<IBarcodeReaderService>(); }
        }

        #endregion

        #region Constructors

        public BarcodeReaderViewModel()
        {
            
        }

        #endregion

        #region Methods

        #endregion
    }
}