using System;
using System.Collections.Generic;
using System.Diagnostics;

using Xamarin.Auth;

using UIKit;
using Newtonsoft.Json;

using Google.Apis.Drive.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Services;
using Google.Apis.Json;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace TestXamAuthGoogleDrive
{

	public class GoogleDriveAgent
	{
		#region members
		private bool m_isSignedIn;
		private AccountStore m_store;
		private Account m_savedAccount;
		private UIViewController m_loginViewController;
		#endregion

		#region events
		public event GoogleDriveSignInEventHandler GoogleDriveSignInEventHandler;
		#endregion

		#region properties
		public DriveService Service { get; private set; }

    public OAuth2Authenticator Authenticator { get; private set; }

    // TODO: Change this a Google API testing app's client secret
    private static string ClientID { get { return "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.apps.googleusercontent.com"; } }

    // TODO: Change this a Google API testing app's client secret
    private static string ClientSecret { get { return "xxxxxxxxxxxxxxxxxxxxxxxxxxxx"; } }

    // TODO: Change this to match a Google API testing app's Uri...AND add this to the Info.plist's Xamarin Auth Google URL scheme
    private static Uri RedirectUrl { get { return new Uri("com.googleusercontent.apps.xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx:/oauth2redirect"); } }

    private static string GoogleDriveScope { get { return "https://www.googleapis.com/auth/drive.readonly"; } }

    private static string GoogleUserInfoProfileScope { get { return "https://www.googleapis.com/auth/userinfo.profile"; } }

    private static Uri AuthorizationUrl { get { return new Uri("https://accounts.google.com/o/oauth2/auth"); } }

    private static Uri AccessTokenUrl { get { return new Uri("https://www.googleapis.com/oauth2/v4/token"); } }

		public UIViewController LoginController { 
			get {
				if (m_loginViewController != null)
					return m_loginViewController;

				m_loginViewController = Authenticator.GetUI ();
			
				return m_loginViewController;
			}

			private set {
				m_loginViewController = value;
			}
		}

		private AccountStore Store { 
			get {
				if (m_store == null)
					m_store = AccountStore.Create ();

				return m_store;
			}
		}

		private Account SavedAccount { 
			get {
				var savedAccounts = Store.FindAccountsForService ("google");
				if ((savedAccounts as List<Account>).Count > 0)
					m_savedAccount = (savedAccounts as List<Account>) [0];
				else
					m_savedAccount = null;

				return m_savedAccount;
			}
		}
			
		public bool IsSignedIn { 
			get {
				m_isSignedIn = SavedAccount != null ? true : false;
				return m_isSignedIn;
			}

			private set {
				m_isSignedIn = value;
			}
		}

    public string AccountName { get; set; }
		
		private GoogleDriveSignInEvent GoogleDriveSignIn { get; set; }

		#endregion

		#region constructors and disposal
		public GoogleDriveAgent ()
		{
			InitializeSession ();
		}
		#endregion

		#region methods
		private void InitializeSession ()
		{
      Authenticator = new Xamarin.Auth.OAuth2Authenticator (ClientID, ClientSecret, GoogleDriveScope, AuthorizationUrl, RedirectUrl, AccessTokenUrl, null, true);
      Authenticator.Completed += OnAuthCompleted;
			Authenticator.Error += OnAuthError;

			if (SavedAccount != null)
				StartService ();
				
			UpdateSignInStatus ();
		}
		

    private void OnAuthCompleted(object sender, AuthenticatorCompletedEventArgs e)
    {
      if (e.IsAuthenticated)
      {  // Success
        UpdateSignInStatus();
        Store.Save(e.Account, "google");

        Console.WriteLine("Authenticated! {0}", e.Account);
        StartService();
        LoginController.DismissViewController(true, null);

      } else {                 // Cancelled or no success
        UpdateSignInStatus();
        LoginController.DismissViewController(true, null);
        LoginController = null;
        InitializeSession();
      }
    }


    private void OnAuthError (object sender, AuthenticatorErrorEventArgs e)
    {
      var authenticator = sender as OAuth2Authenticator;

      if (authenticator != null) {
        authenticator.Completed -= OnAuthCompleted;
        authenticator.Error -= OnAuthError;
      }

      Console.WriteLine("Authentication error: " + e.Message);
    }


    private bool StartService()
    {
      try
      {
        var responseCredential = JsonConvert.SerializeObject(SavedAccount.Properties);

        // Maybe we need to add a type because the raw response credentail does not contain one and will throw
        // {System.InvalidOperationException: Error creating credential from JSON. Unrecognized credential type . 
        // when creating the credential...?
        JObject credentailAsJson = JObject.Parse(responseCredential);
        credentailAsJson.Add("type", "service_account");
        var credentialAsSerializedJson = JsonConvert.SerializeObject(credentailAsJson);

        // ...? Nope, turns out the credential is not a valid service_account credential.
        GoogleCredential credential = GoogleCredential.FromJson(credentialAsSerializedJson).CreateScoped(GoogleDriveScope);

        Service = new DriveService(new BaseClientService.Initializer()
        {
          HttpClientInitializer = credential,
          ApplicationName = "com.companyname.testxamauthgoogledrive",
        });

        return true;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
        return false;
      }
    }


		public void UpdateSignInStatus ()
		{
			if (GoogleDriveSignIn == null)
				GoogleDriveSignIn = new GoogleDriveSignInEvent ();

			if (GoogleDriveSignInEventHandler != null) {
				GoogleDriveSignIn = new GoogleDriveSignInEvent();
				GoogleDriveSignIn.DidSignIn = IsSignedIn;
				GoogleDriveSignInEventHandler (this, GoogleDriveSignIn);
			}
		}

		public void SignOut ()
		{
			if (SavedAccount == null) {
				Debug.WriteLine("Already signed out or attempting to sign out when no credentials are saved.");
				return;
			}

			// Attempt to revoke access to Google API...
			string accessToken = "";
			if (SavedAccount.Properties.ContainsKey ("access_token"))
				accessToken = SavedAccount.Properties ["access_token"];
			var client = new RestClient ("https://accounts.google.com/o/oauth2/revoke");
			var request = new RestRequest (Method.POST);
			request.AddParameter("token", accessToken);
			client.ExecuteAsync (request, response => Debug.WriteLine (response.Content));

			// Delete the Saved Account ...
			if (SavedAccount != null)
				Store.Delete (SavedAccount, "google");
				
			// Destroy the Authenticator, etc...
			Service = null;
			LoginController = null;
			Authenticator = null;
			AccountName = String.Empty;
			IsSignedIn = false;

			InitializeSession ();
		}
		#endregion

	}
}

