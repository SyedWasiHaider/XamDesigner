using System;
using Xamarin.Forms.Platform.Android;
using Android.Graphics.Drawables;
using Android.Graphics;

namespace XamDesigner.Android
{
	public class BorderEffect : PlatformEffect
	{
		public BorderEffect ()
		{
		}

		protected override void OnAttached ()
		{
			GradientDrawable gd = new GradientDrawable();
			gd.SetColor (100); // Changes this drawbale to use a single color instead of a gradient
			gd.SetCornerRadius(2);
			gd.SetStroke (3, Color.Red);

			if (this.Control != null) {
				this.Control.SetBackgroundDrawable (gd);
			} else if (this.Container != null) {
				this.Container.SetBackgroundDrawable (gd);
			}

		}

		protected override void OnDetached ()
		{
			if (this.Control != null) {
				this.Control.SetBackgroundDrawable (null);
			} else if (this.Container != null) {
				this.Container.SetBackgroundDrawable (null);
			}
		}

	}
}

