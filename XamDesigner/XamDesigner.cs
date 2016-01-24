using System;

using Xamarin.Forms;
using System.Collections.Generic;


namespace XamDesigner
{
	public class App : Application
	{
		public const string ChangeControlMessage = "ChangeControlMessage";
		public const string ToggleOptionsDrawer = "OpenOptionsDrawer";

		static public Dictionary<string,string> SupportedTypes;
		public App ()
		{
			// The root page of your application

			var types = new [] {
				typeof(Button),
				typeof(Entry),
				typeof(Label),
				typeof(Rectangle)
			};

			SupportedTypes = new Dictionary<string,string> ();
			foreach (var type in types) {
				SupportedTypes.Add (type.Name, type.AssemblyQualifiedName);
			}

			MainPage = new MasterDetailPage() { 
				Master= new MenuPage() {Icon="hamburger.png", Title="wtf"}
					, Detail = new NavigationPage(new MainPage() { Title="Edit Mode"}), IsGestureEnabled=false};

			ToolbarItem toolBarItem = null;
			toolBarItem = new ToolbarItem ("Options", "", () => {
				MessagingCenter.Send(this, ToggleOptionsDrawer, ToggleOptionsDrawer);
			});
			MainPage.ToolbarItems.Add (toolBarItem);
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}

