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

		protected override void OnAttached ()
		{
			if (this.Control != null) {
					this.Control.Layer.BorderColor = UIColor.Red.CGColor;
				this.Control.Layer.BorderWidth = 2;			
			} 
			else if (this.Container != null) {
				this.Container.Layer.BorderColor = UIColor.Red.CGColor;
				this.Container.Layer.BorderWidth = 2;	
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

