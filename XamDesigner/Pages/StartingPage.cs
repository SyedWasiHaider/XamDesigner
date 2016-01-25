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
		PrototypeView protoTypePage;
		public StartingPage(){
		}

		bool performingSetup = false;
		private void Setup(){
			AddProtoTypePage ();
			AddMenu ();
			absoluteLayout = new MR.Gestures.AbsoluteLayout () {Children = {protoTypePage, MenuGrid} };
			Content = absoluteLayout;
		}

		bool doOnce = true;
		protected override void OnAppearing ()
		{
			base.OnAppearing ();
			if (doOnce) {
				doOnce = false;
				Setup ();
			}
		}	

		public void AddProtoTypePage(){
			protoTypePage = new PrototypeView ();
			AbsoluteLayout.SetLayoutFlags (protoTypePage,
				AbsoluteLayoutFlags.PositionProportional);

			AbsoluteLayout.SetLayoutBounds (protoTypePage,
				new Rectangle (0f,
					0f, Width, Height));
		}

		public void AddMenu(){
			MenuGrid = new OptionsMenu ();
			MenuGrid.OptionsList = protoTypePage.options;
			AbsoluteLayout.SetLayoutFlags (MenuGrid,
				AbsoluteLayoutFlags.PositionProportional);

			AbsoluteLayout.SetLayoutBounds (MenuGrid,
				new Rectangle (1f,
					0f, AbsoluteLayout.AutoSize, Height));
			MenuGrid.ItemTapped += (object sender, OptionsMenu.OptionTappedEventArgs e) => {
				protoTypePage.ExecuteAction(e.Position);
			};
		}

		public void ToggleEditMode(){
			protoTypePage.TogglePreviewEdit (false);
		}
	}
}


