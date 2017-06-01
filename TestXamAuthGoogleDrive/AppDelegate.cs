using System;
using Foundation;
using UIKit;

namespace TestXamAuthGoogleDrive
{
  [Register("AppDelegate")]
  public class AppDelegate : UIApplicationDelegate
  {

    public override UIWindow Window { get; set; }

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions) { return true; }

    public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
    {
      if (sourceApplication == "com.apple.SafariViewService") // Google Authentication - because we are using NativeUI
      {
        Uri uri_abs = new Uri(url.AbsoluteString);

        ViewController vc = (TestXamAuthGoogleDrive.ViewController)Window.RootViewController;
        vc.DriveAgent.Authenticator.OnPageLoading(uri_abs);
        return true;
      }

      return true;
    }

  }
}

