using System;
using Xamarin.Forms;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using MODE = XamDesigner.PrototypePageViewModel.MODE;
using ACTION = XamDesigner.PrototypePageViewModel.ACTION;

namespace XamDesigner
{
	public class ContainerPage : ContentPage
	{
		MR.Gestures.AbsoluteLayout absoluteLayout;
		public OptionsMenu MenuGrid;
		public PrototypeView protoTypePage;
		bool isFirstPage = false;
		public ContainerPage(bool firstPage = false){
			isFirstPage = firstPage;
		}
			
		private void Setup(){
			SetupProtoTypePage ();
			SetupMenu ();
		}

		bool mainSetup = true;
		protected override void OnAppearing ()
		{
			base.OnAppearing ();
			if (mainSetup) {
				mainSetup = false;
				absoluteLayout = new MR.Gestures.AbsoluteLayout () { };
				Setup ();
				Content = absoluteLayout;
			} else {
				SetupMenu ();
			}
		}

		public void NavigateBack(){
			if (absoluteLayout.Children.Count > 1) {

				var BorderEffect = DependencyService.Get<Effect> ();
				protoTypePage.Effects.Clear ();
				protoTypePage.Effects.Add(BorderEffect);
				new Animation (delegate(double obj) {
					protoTypePage.TranslationX = obj;
				}, 0, -1*Width).Commit (this, "something", easing: Easing.Linear, finished: delegate {
					absoluteLayout.Children.Remove(protoTypePage);
					protoTypePage.Effects.RemoveAt(0);
				});
			}
		}

		public void SetupProtoTypePage(string id = null){			

			if (protoTypePage == null) {
				protoTypePage = new PrototypeView () { 
					BindingContext = new PrototypePageViewModel (),
					BackgroundColor = Color.White
				};
				protoTypePage.navControlId = id;
			} else {
				return;
			}

			AbsoluteLayout.SetLayoutFlags (protoTypePage,
				AbsoluteLayoutFlags.PositionProportional);

			AbsoluteLayout.SetLayoutBounds (protoTypePage,
				new Rectangle (0f,
					0f, Width, Height));

			if (absoluteLayout.Children.Count > 0){
				var menu = absoluteLayout.Children.Last ();
				absoluteLayout.Children.Remove (menu);
				absoluteLayout.Children.Add(protoTypePage);
				SetupMenu ();
				var BorderEffect = DependencyService.Get<Effect> ();
				protoTypePage.Effects.Add(BorderEffect);
				protoTypePage.TranslationX =  -1 * Width;
				new Animation (delegate(double obj) {
					protoTypePage.TranslationX = obj;
				},  -1* Width, 0).Commit (this, "something", finished: delegate {
					protoTypePage.Effects.RemoveAt(0);
				},easing: Easing.BounceIn);
			
			}else{
				absoluteLayout.Children.Add(protoTypePage);
			}

			if (isFirstPage) {
				protoTypePage.ViewModel.AddNewMenuButtonForPage (this);
			}
		}

		public void SetupMenu(){
			MenuGrid = new OptionsMenu ();
			MenuGrid.OptionsList = ((PrototypePageViewModel)protoTypePage.BindingContext).MenuOptions;
		
			AbsoluteLayout.SetLayoutFlags (MenuGrid,
				AbsoluteLayoutFlags.PositionProportional);

			AbsoluteLayout.SetLayoutBounds (MenuGrid,
				new Rectangle (1f,
					0f, AbsoluteLayout.AutoSize, Height));
			MenuGrid.ItemTapped += (object sender, OptionsMenu.OptionTappedEventArgs e) => {
				protoTypePage.ViewModel.ExecuteMenuAction(e.Position);
			};
			absoluteLayout.Children.Add (MenuGrid);

		}
	}
}


