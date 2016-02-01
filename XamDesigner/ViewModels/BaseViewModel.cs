using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;

namespace XamDesigner
{
	public class BaseViewModel : INotifyPropertyChanged
	{
		public BaseViewModel ()
		{
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void RaisePropertyChanged([CallerMemberName] string caller = ""){
			OnPropertyChanged (caller);
		}

		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this,
					new PropertyChangedEventArgs(propertyName));
		}

		public INavigation Navigation { get { return ((App)App.Current).innerNavPage.Navigation; } }

		public ContainerPage TopPage { get { return (ContainerPage)Navigation.NavigationStack.LastOrDefault (); } }


		//This should probably be done using messaging but naahhhh
		public Task<string> DisplayOptionAlert(string title, string cancel, string destruction, params string[] buttons){
			return App.Current.MainPage.DisplayActionSheet (title, cancel, destruction, buttons);
		}

		public Task DisplayMessageAlert(string title, string message, string cancel){
			return App.Current.MainPage.DisplayAlert (title, message, cancel);
		}

	}
}

