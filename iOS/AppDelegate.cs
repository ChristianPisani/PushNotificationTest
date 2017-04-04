using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Foundation;
using UIKit;

using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace PushNotificationExample.iOS
{
	[Register ("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, IAuthenticate
	{

		// Define a authenticated user.
 		private MobileServiceUser user;


		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// Initialize Azure Mobile Apps
			Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

			// Initialize Xamarin Forms
			global::Xamarin.Forms.Forms.Init ();


			 // Register for push notifications.
 			var settings = UIUserNotificationSettings.GetSettingsForTypes(
				UIUserNotificationType.Alert
				| UIUserNotificationType.Badge
				| UIUserNotificationType.Sound,
				new NSSet());

			UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
			UIApplication.SharedApplication.RegisterForRemoteNotifications();


			App.Init(this);

			LoadApplication (new App ());

			return base.FinishedLaunching (app, options);
		}

		public async Task<bool> Authenticate() {
			var success = false;
			var message = string.Empty;
			try {
				// Sign in with Facebook login using a server-managed flow.
				if (user == null) {
					user = await TodoItemManager.DefaultManager.CurrentClient
						.LoginAsync(UIApplication.SharedApplication.KeyWindow.RootViewController,
					                MobileServiceAuthenticationProvider.Facebook);
					if (user != null) {
						message = string.Format("You are now signed-in as {0}.", user.UserId);
						success = true;
					}
				}
			} catch (Exception ex) {
				message = ex.Message;
			}

			// Display the success or failure message.
			UIAlertView avAlert = new UIAlertView("Sign-in result", message, null, "OK", null);
			avAlert.Show();

			return success;
		}

		//////
		//PUSH
		//////

		public override void RegisteredForRemoteNotifications(UIApplication application,
	 		NSData deviceToken) {
			const string templateBodyAPNS = "{\"aps\":{\"alert\":\"$(messageParam)\"}}";

			JObject templates = new JObject();
			templates["genericMessage"] = new JObject
				{
		   {"body", templateBodyAPNS}
		 };

			// Register for push with your mobile app
			Push push = TodoItemManager.DefaultManager.CurrentClient.GetPush();
			push.RegisterAsync(deviceToken, templates);
		}

		 public override void DidReceiveRemoteNotification(UIApplication application,
			NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler) {
			NSDictionary aps = userInfo.ObjectForKey(new NSString("aps")) as NSDictionary;

			string alert = string.Empty;
			if (aps.ContainsKey(new NSString("alert")))
				alert = (aps[new NSString("alert")] as NSString).ToString();

			//show alert
			if (!string.IsNullOrEmpty(alert)) {
				UIAlertView avAlert = new UIAlertView("Notification", alert, null, "OK", null);
				avAlert.Show();
			}
		}


	}
}

