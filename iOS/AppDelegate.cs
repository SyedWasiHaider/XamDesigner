using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using Xamarin.Forms;

namespace XamDesigner.iOS
{
	[Register ("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init ();

			// Code for starting up the Xamarin Test Cloud Agent
			#if ENABLE_TEST_CLOUD
			Xamarin.Calabash.Start();
			#endif
			MR.Gestures.iOS.Settings.LicenseKey = "MYDE-VBMY-GKUK-U7T8-YDGV-W4GH-MEDC-M8NA-5ZQF-YN8A-CK63-MWPN-BT89";
			DependencyService.Register<Effect, BorderEffect> ();
			LoadApplication (new App ());

			return base.FinishedLaunching (app, options);
		}
	}
}

