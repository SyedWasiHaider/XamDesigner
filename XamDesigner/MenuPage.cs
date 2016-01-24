using System;
using Xamarin.Forms;

namespace XamDesigner
{
	public class MenuPage: ContentPage {

		private Command getCommand(string controlAssembleName){
			var command = new Command ( (ok) => {
				MessagingCenter.Send(this, App.ChangeControlMessage, controlAssembleName);
				(App.Current.MainPage as MasterDetailPage).IsPresented = false;
			});
			return command;
		}

		public MenuPage( ) {

			var dict = App.SupportedTypes;
			var layout = new StackLayout {
				Padding = new Thickness ( 0, Device.OnPlatform<int>( 20, 0, 0 ), 0, 0 ),
			};

			foreach(var typeSet in dict){
				layout.Children.Add (new SlidingTrayButton (typeSet.Key, typeSet.Value) { Command = getCommand(typeSet.Value)});
			}

			Content = layout;
			Title = "Controls";
			BackgroundColor = Color.Gray.WithLuminosity( 0.2 );
			Icon = Device.OS == TargetPlatform.iOS ? "slideout.png" : null;
		}
	}
}

