using System;
using Xamarin.Forms;

namespace XamDesigner
{
	public class PropertyCell : ViewCell
	{
		public PropertyCell()
		{
			
		}
		Label propertyName; 
		Entry propertyValue;
		private void Setup(){
			Grid wrapper = new Grid ();
			wrapper.ColumnDefinitions.Add (new ColumnDefinition ());
			wrapper.ColumnDefinitions.Add (new ColumnDefinition ());
			wrapper.RowDefinitions.Add (new RowDefinition ());

			propertyName = new Label (){Text="Property", HorizontalOptions=LayoutOptions.CenterAndExpand};
			propertyValue = new Entry () { Text = "Value", HorizontalOptions=LayoutOptions.CenterAndExpand };
			propertyValue.TextChanged += (sender, e) => {
				((XamDesigner.EditPropertiesPage.PropertyTuple)(BindingContext)).value = e.NewTextValue;
			};

			Grid.SetColumn (propertyName, 0);
			Grid.SetColumn (propertyValue, 1);
			wrapper.Children.Add (propertyName);
			wrapper.Children.Add (propertyValue);

			View = wrapper;
		}

		public static readonly BindableProperty EntryProperty =
			BindableProperty.Create<PropertyCell, string> (p => p.EntryValue, "");

		public string EntryValue {
			get { return(string)GetValue (EntryProperty); }
			set { SetValue (EntryProperty, value); }
		}

		public static readonly BindableProperty LabelProperty =
			BindableProperty.Create<PropertyCell, string> (p => p.LabelValue, "");

		public string LabelValue {
			get { return(string)GetValue (LabelProperty); }
			set { SetValue (LabelProperty, value); }
		}

		protected override void OnBindingContextChanged ()
		{
			base.OnBindingContextChanged ();

			if (BindingContext != null) {
				Setup ();
				propertyName.Text = LabelValue;
				propertyValue.Text = EntryValue;
			}
		}

	}
}

