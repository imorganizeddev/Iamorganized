using Android.App;
using BarcodeReader.ViewModels;
using Intersoft.Crosslight;
using Intersoft.Crosslight.Android;

namespace BarcodeReader.Android
{
    [Activity(Label = "Crosslight App", Icon = "@drawable/icon")]
    [ImportBinding(typeof(SimpleBindingProvider))]
    public class SimpleActivity : Activity<SimpleViewModel>
    {
        #region Constructors

        public SimpleActivity()
            : base(Resource.Layout.MainLayout)
        {
        }

        #endregion
    }
}