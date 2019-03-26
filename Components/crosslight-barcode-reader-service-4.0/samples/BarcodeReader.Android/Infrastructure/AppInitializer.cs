using Android.App;
using BarcodeReader.Infrastructure;
using Intersoft.Crosslight;
using Intersoft.Crosslight.Android;
using Intersoft.Crosslight.Services.Barcode;
using Intersoft.Crosslight.Services.Barcode.Android;

namespace BarcodeReader.Android.Infrastructure
{
    public sealed class AppInitializer : IApplicationInitializer
    {
        #region IApplicationInitializer Members

        public IApplicationService GetApplicationService(IApplicationContext context)
        {
            return new CrosslightAppAppService(context);
        }

        public void InitializeApplication(IApplicationHost appHost)
        {
        }

        public void InitializeComponents(IApplicationHost appHost)
        {
        }

        public void InitializeServices(IApplicationHost appHost)
        {
            AndroidApp.PreserveAssembly((typeof(Intersoft.Crosslight.Services.Barcode.Android.ServiceInitializer).Assembly));
        }

        #endregion
    }
}