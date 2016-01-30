using System;
using Xamarin.Forms.Platform.iOS;
using UIKit;
using CoreGraphics;

namespace XamDesigner.iOS
{
	public class BorderEffect : PlatformEffect
	{
		public BorderEffect ()
		{
		}

		public CGColor BorderColor = UIColor.LightGray.CGColor;
		public nfloat BorderWidth = 2;

		protected override void OnAttached ()
		{
			if (this.Control != null) {
				this.Control.Layer.BorderColor = BorderColor;
				this.Control.Layer.BorderWidth = BorderWidth;
			} 
			else if (this.Container != null) {
				this.Container.Layer.BorderColor = BorderColor;
				this.Container.Layer.BorderWidth = BorderWidth;	
			}
		}

		protected override void OnDetached ()
		{
			if (this.Control != null) {
				this.Control.Layer.BorderWidth = 0;			
			} 
			else if (this.Container != null) {
				this.Container.Layer.BorderWidth = 0;	
			}		
		}



	}
}

