using System;

using Xamarin.Forms;
using System.Collections.Generic;
using System.Linq;
using Xamarin.BrandColors;
using System.Diagnostics;


namespace XamDesigner
{
	public class App : Application
	{
		public const string ToggleOptionsDrawer = "OpenOptionsDrawer";

		public ContainerPage StartingPage;
		public MenuPage MenuPage;
		static public Dictionary<string,string> SupportedTypes;

		public NavigationPage innerNavPage, outerNavPage;
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
				typeof(WebView)
			};

			SupportedTypes = new Dictionary<string,string> ();
			foreach (var type in types) {
				SupportedTypes.Add (type.Name, type.AssemblyQualifiedName);
			}
			SupportedTypes.Add ("Navigation", typeof(PrototypeView).AssemblyQualifiedName);

			StartingPage = new ContainerPage (true);
			innerNavPage = new NavigationPage (StartingPage) { Title = "Xamarin Designer" } ;
			outerNavPage = new NavigationPage (
				                   innerNavPage) {
				BarBackgroundColor = XamarinColor.DarkBlue.getColor (), 
				BarTextColor = XamarinColor.LighterGray.getColor ()

			};
			MainPage = new MasterDetailPage() { 
				Master= MenuPage = new MenuPage() {Icon="hamburger.png", Title="wtf"}
					, Detail = outerNavPage
					, 
							IsGestureEnabled=false
			};

			NavigationPage.SetHasNavigationBar (StartingPage, false);
			AddOptionToolItem ();
		}

		public void AddOptionToolItem(){

			ToolbarItem toolBarItem = null;
			toolBarItem = new ToolbarItem ("Options", "", async () => {
				//Cringe
				try {
					await ((ContainerPage)((App)App.Current).StartingPage.Navigation.NavigationStack.LastOrDefault()).MenuGrid.ShowHideMenu();
				}catch(Exception e){
					Debug.WriteLine(e);
				}
			});
			outerNavPage.ToolbarItems.Add (toolBarItem);
			innerNavPage.ToolbarItems.Clear ();
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

