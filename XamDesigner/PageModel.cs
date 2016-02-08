using System;
using System.Collections.ObjectModel;
using XamDesigner.Models;

namespace XamDesigner
{
	public class PageModel
	{
		public ObservableCollection<ControlModel> controls {
			get;
			set;
		}
		public PageModel ()
		{
			controls = new ObservableCollection<ControlModel> ();
		}
	}
}

