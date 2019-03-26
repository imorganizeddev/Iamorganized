using BarcodeReader.Infrastructure;
using Intersoft.Crosslight;
using Intersoft.Crosslight.iOS;
using Intersoft.Crosslight.Services.Barcode;
using Intersoft.Crosslight.Services.Barcode.iOS;

namespace BarcodeReader.iOS.Infrastructure
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
            UIApplicationDelegate.PreserveAssembly((typeof(ServiceInitializer).Assembly));
        }

        #endregion
    }
}