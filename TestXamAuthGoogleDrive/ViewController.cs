using System;

using UIKit;

namespace TestXamAuthGoogleDrive
{
  public partial class ViewController : UIViewController
  {
    public GoogleDriveAgent DriveAgent { get; set; }

    partial void TestSignIn(UIButton sender)
    {
      DriveAgent = new GoogleDriveAgent();
      PresentViewController(DriveAgent.LoginController, true, null);
    }

    partial void TestSignOut(UIButton sender)
    {
      DriveAgent.SignOut();
      DriveAgent.UpdateSignInStatus();
    }

    protected ViewController(IntPtr handle) : base(handle)
    {
      // Note: this .ctor should not contain any initialization logic.
    }

    public override void ViewDidLoad()
    {
      base.ViewDidLoad();
      // Perform any additional setup after loading the view, typically from a nib.
    }

    public override void DidReceiveMemoryWarning()
    {
      base.DidReceiveMemoryWarning();
      // Release any cached data, images, etc that aren't in use.
    }
  }
}
