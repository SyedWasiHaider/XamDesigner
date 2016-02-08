using System;
using Xamarin.Forms;
using Xamarin.BrandColors;
using System.Linq;

namespace XamDesigner
{
	public class MenuPage: ContentPage {

		private Command getCommand(){
			var command = new Command ( (ok) => {
				(App.Current.MainPage as MasterDetailPage).IsPresented = false;
			});
			return command;
		}

		public void AddButton(SlidingTrayButton button){
			layout.Children.Add (button);
		}

		public void AddButton(string title, Command action){
			layout.Children.Add (new SlidingTrayButton (title){ Command = action});
		}

		StackLayout layout;
		public MenuPage( ) {
			
			layout = new StackLayout {
				Padding = new Thickness ( 0, Device.OnPlatform<int>( 20, 0, 0 ), 0, 0 ),
			};

			layout.Children.Add (new SlidingTrayButton ("Change Mode") {Command = new Command(()=>{
				((App)App.Current).StartingPage.protoTypePage.ViewModel.ToggleMode();
				(App.Current.MainPage as MasterDetailPage).IsPresented = false;
			})});

			layout.Children.Add (new SlidingTrayButton ("Save") {Command = new Command(()=>{
				((App)App.Current).StartingPage.protoTypePage.SaveViewToStorage("page1");
				(App.Current.MainPage as MasterDetailPage).IsPresented = false;
			})});
				
			Content = layout;
			Title = "Controls";
			BackgroundColor = XamarinColor.DarkerBlue.getColor ();
			Icon = Device.OS == TargetPlatform.iOS ? "slideout.png" : null;
		}
	}
}

