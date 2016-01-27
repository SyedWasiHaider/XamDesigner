using System;

using Xamarin.Forms;
using System.Collections.Generic;


namespace XamDesigner
{
	public class App : Application
	{
		public const string ToggleOptionsDrawer = "OpenOptionsDrawer";

		public StartingPage StartingPage; 
		static public Dictionary<string,string> SupportedTypes;
		public App ()
		{
			// The root page of your application

			var types = new [] {
				typeof(Button),
				typeof(Entry),
				typeof(Label),
				typeof(Switch),
				typeof(BoxView),
				typeof(Image),
				typeof(ListView),
			};

			SupportedTypes = new Dictionary<string,string> ();
			foreach (var type in types) {
				SupportedTypes.Add (type.Name, type.AssemblyQualifiedName);
			}



			MainPage = new MasterDetailPage() { 
				Master= new MenuPage() {Icon="hamburger.png", Title="wtf"}
					, Detail = new NavigationPage(StartingPage = new StartingPage () { Title = "Xamarin Designer" }), IsGestureEnabled=false};
			AddOptionToolItem ();
		}

		public void AddOptionToolItem(){

			ToolbarItem toolBarItem = null;
			toolBarItem = new ToolbarItem ("Options", "", () => {
				StartingPage.MenuGrid.ToggleMenu();
			});
			App.Current.MainPage.ToolbarItems.Add (toolBarItem);
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

