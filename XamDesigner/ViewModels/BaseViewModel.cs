﻿using System;
using System.ComponentModel;

namespace XamDesigner
{
	public class BaseViewModel : INotifyPropertyChanged
	{
		public BaseViewModel ()
		{
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this,
					new PropertyChangedEventArgs(propertyName));
		}
	}
}

