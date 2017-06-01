//
// GoogleDriveSignInEvent.cs
//
using System;

namespace TestXamAuthGoogleDrive
{
	public delegate void GoogleDriveSignInEventHandler(object sender, GoogleDriveSignInEvent e);

	// ReSharper disable once InconsistentNaming
	public class GoogleDriveSignInEvent : EventArgs
	{
		public bool DidSignIn { get; set; }
	}
}

