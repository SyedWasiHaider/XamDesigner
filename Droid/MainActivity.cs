using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin.Forms;
using XamDesigner.Android;

namespace XamDesigner.Droid
{
	[Activity (Label = "XamDesigner", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			global::Xamarin.Forms.Forms.Init (this, bundle);
			DependencyService.Register<Effect, BorderEffect> ();
			MR.Gestures.Android.Settings.LicenseKey = "MYDE-VBMY-GKUK-U7T8-YDGV-W4GH-MEDC-M8NA-5ZQF-YN8A-CK63-MWPN-BT89";
			LoadApplication (new App ());
		}
	}
}

