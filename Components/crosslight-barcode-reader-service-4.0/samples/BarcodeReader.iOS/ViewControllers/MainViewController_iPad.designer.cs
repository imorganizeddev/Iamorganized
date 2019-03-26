// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;

namespace BarcodeReader.iOS
{
    [Register("MainViewController_iPad")]
    partial class MainViewController_iPad
    {
        [Outlet]
        UIKit.UILabel GreetingLabel { get; set; }

        [Outlet]
        UIKit.UITextField Text1 { get; set; }

        [Outlet]
        UIKit.UIButton Button1 { get; set; }

        [Outlet]
        UIKit.UILabel FooterLabel { get; set; }

        void ReleaseDesignerOutlets()
        {
            if (GreetingLabel != null)
            {
                GreetingLabel.Dispose();
                GreetingLabel = null;
            }

            if (Text1 != null)
            {
                Text1.Dispose();
                Text1 = null;
            }

            if (Button1 != null)
            {
                Button1.Dispose();
                Button1 = null;
            }

            if (FooterLabel != null)
            {
                FooterLabel.Dispose();
                FooterLabel = null;
            }
        }
    }
}
