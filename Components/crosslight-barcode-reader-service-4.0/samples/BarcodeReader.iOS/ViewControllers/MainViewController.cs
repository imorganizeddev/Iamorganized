using BarcodeReader.ViewModels;
using Intersoft.Crosslight;
using Intersoft.Crosslight.iOS;
using Foundation;
using UIKit;

namespace BarcodeReader.iOS
{
    [ImportBinding(typeof(SimpleBindingProvider))]
    [RegisterNavigation(DeviceKind.Phone)]
    public partial class MainViewController : UIViewController<SimpleViewModel>
    {

        #region Constructors

        public MainViewController() :
        base("MainViewController", null)
        {
        }

        #endregion

        #region Methods

        public override bool HideKeyboardOnTap
        {
            get
            {
                return true;
            }
        }

        protected override void OnViewInitialized()
        {
            base.OnViewInitialized();

            this.Title = "Crosslight App";
        }

        #endregion
    }
}