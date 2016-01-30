using System;
using Xamarin.Forms;

namespace XamDesigner
{
	public class MenuPage: ContentPage {

		private Command getCommand(){
			var command = new Command ( (ok) => {
				(App.Current.MainPage as MasterDetailPage).IsPresented = false;
			});
			return command;
		}

		public MenuPage( ) {

			var dict = App.SupportedTypes;
			var layout = new StackLayout {
				Padding = new Thickness ( 0, Device.OnPlatform<int>( 20, 0, 0 ), 0, 0 ),
			};

			layout.Children.Add (new SlidingTrayButton ("Change Mode") {Command = new Command(()=>{
				((App)App.Current).StartingPage.ToggleMode();
				(App.Current.MainPage as MasterDetailPage).IsPresented = false;
			})});

			var genericCommand = getCommand ();
			layout.Children.Add (new SlidingTrayButton ("New Project"){ Command = genericCommand});
			layout.Children.Add (new SlidingTrayButton ("New Page"){ Command = genericCommand});
			layout.Children.Add (new SlidingTrayButton ("Save Project"){Command = genericCommand});

			Content = layout;
			Title = "Controls";
			BackgroundColor = Color.Gray.WithLuminosity( 0.2 );
			Icon = Device.OS == TargetPlatform.iOS ? "slideout.png" : null;
		}
	}
}

