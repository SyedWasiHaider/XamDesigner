using System;
using Xamarin.Forms;
using System.Reflection;
using System.Collections.Generic;

namespace XamDesigner.Models
{
	public class ControlModel
	{
		public ControlModel ()
		{
			Properties = new Dictionary<string, object> ();
			ColorProperies = new Dictionary<string, ColorTuple> ();
		}

		public class ColorTuple
		{
			public double r;
			public double g;
			public double b;
		}

		public Type ControlType;
		public double x {get;set;}
		public double y {get;set;}
		public double Width {get;set;}
		public double Height { get; set; }
		public Command Action;
		public Dictionary<string, object> Properties {get;set;}
		public Dictionary<string, ColorTuple> ColorProperies {get;set;}

		public object Data { get; set; }
	}
}
