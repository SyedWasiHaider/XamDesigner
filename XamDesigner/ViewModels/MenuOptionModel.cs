using System;
using Xamarin.Forms;

namespace XamDesigner
{
	public class MenuOptionModel
	{
		public MenuOptionModel ()
		{
		}

		public string Title;
		public Command Command;
		public Command UnToggleCommand;
		public bool IsToggleable = false;
		public bool IsToggled = false;
		public bool UnToggleOthers = false;

	}
}

