using System;
using Xamarin.Forms;
using Xamarin.BrandColors;

namespace XamDesigner
{
	public class SlidingTrayButton : Button {
		public SlidingTrayButton( String optionName) {
			Text = optionName;
			TextColor = XamarinColor.LighterGray.getColor ();
		}
	}
}

