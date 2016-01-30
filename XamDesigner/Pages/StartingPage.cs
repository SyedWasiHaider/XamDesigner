using System;

using Xamarin.Forms;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;

namespace XamDesigner
{
	public class StartingPage : ContentPage
	{
		MR.Gestures.AbsoluteLayout absoluteLayout;
		public OptionsMenu MenuGrid;
		Stack<PrototypeView> protoTypePages = new Stack<PrototypeView>();
		public PrototypeView activePage;
		public StartingPage(){
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
				protoTypePages.Peek ().Effects.Clear ();
				protoTypePages.Peek ().Effects.Add(BorderEffect);
				new Animation (delegate(double obj) {
					protoTypePages.Peek().TranslationX = obj;
				}, 0, -1*Width).Commit (this, "something", easing: Easing.Linear, finished: delegate {
					absoluteLayout.Children.Remove(protoTypePages.Peek ());
					protoTypePages.Peek ().Effects.RemoveAt(0);
				});
			}
		}

		public void SetupProtoTypePage(string id = null){			


			PrototypeView protoTypePage = null;
			if (id != null) {
				protoTypePage = (from page in protoTypePages
				            where page.navControlId == id
				            select page).FirstOrDefault ();
			} 

			if (protoTypePage == null){
				protoTypePage = new PrototypeView () { BackgroundColor = Color.White } ;
				protoTypePage.navControlId = id;
				protoTypePages.Push (protoTypePage);
			}

			activePage = protoTypePage;

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

		}

		public void SetupMenu(){
			var protoTypePage = protoTypePages.Peek ();
			MenuGrid = new OptionsMenu ();
			MenuGrid.OptionsList = protoTypePage.options;
			MenuGrid.ToggleAble = protoTypePage.toggleable;
			AbsoluteLayout.SetLayoutFlags (MenuGrid,
				AbsoluteLayoutFlags.PositionProportional);

			AbsoluteLayout.SetLayoutBounds (MenuGrid,
				new Rectangle (1f,
					0f, AbsoluteLayout.AutoSize, Height));
			MenuGrid.ItemTapped += (object sender, OptionsMenu.OptionTappedEventArgs e) => {
				activePage.ExecuteAction(e.Position);
			};
			absoluteLayout.Children.Add (MenuGrid);
		}

		public void ToggleMode(bool forceEdit = false){

			PrototypeView.MODE modeToSet = activePage.currMode;
			if (!forceEdit) {
				modeToSet = modeToSet == PrototypeView.MODE.EDIT ? PrototypeView.MODE.PREVIEW : PrototypeView.MODE.EDIT;
			} else {
				modeToSet = PrototypeView.MODE.EDIT;
			}


			foreach (var page in protoTypePages) {
				page.SetMode (modeToSet);
			}
		}
	}
}


