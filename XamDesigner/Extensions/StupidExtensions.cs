using System;
using Xamarin.Forms;
using Xamarin.BrandColors;

namespace XamDesigner
{
	public static class StupidExtensions
	{
		public static Color getColor(this XamarinColor xamColor){
			return Color.FromRgb (xamColor.R, xamColor.G, xamColor.B);
		}
	}
}

