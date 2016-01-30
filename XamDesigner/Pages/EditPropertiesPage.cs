using System;
using System.Reflection;
using Xamarin.Forms;
using System.Linq;
using System.Collections.Generic;

namespace XamDesigner
{
	public class EditPropertiesPage: ContentPage
	{
		public class PropertyTuple{
			public string name { get; set;}
			public string value { get; set;}
		}
		Button SaveButton, CancelButton;
		public EditPropertiesPage (View viewToEdit)
		{
			var properties = viewToEdit.GetType ().GetRuntimeProperties ();
			SaveButton = new Button(){ Text="Save", HorizontalOptions=LayoutOptions.Center };
			CancelButton = new Button() { Text="Cancel", HorizontalOptions=LayoutOptions.Center};
			var list = new ListView () {
			};

			var supportedProps = new Type[] {
				typeof(int),
				typeof(sbyte),
				typeof(byte),
				typeof(short),
				typeof(ushort),
				typeof(int),
				typeof(uint),
				typeof(long),
				typeof(ulong),
				typeof(float),
				typeof(double),
				typeof(decimal),
				typeof(string),
				typeof(bool),
	
			};

			var source = new List<PropertyTuple> ();
			foreach (var property in properties) {
				if (property.CanWrite && property.CanRead && property.GetValue(viewToEdit) != null && 
					supportedProps.Contains(property.PropertyType)) {
					source.Add (new PropertyTuple () { name = property.Name, value = property.GetValue (viewToEdit).ToString () });
				}
			}

			var template = new DataTemplate (typeof(PropertyCell));
			template.SetBinding (PropertyCell.LabelProperty, new Binding(".name", BindingMode.TwoWay));
			template.SetBinding (PropertyCell.EntryProperty, new Binding(".value", BindingMode.TwoWay));
			list.ItemTemplate = template;
			list.ItemsSource = source;
			Content = new StackLayout () {
				Children = {new Label() { Text="Edit Properties" }, list, SaveButton
					, CancelButton
					 },
				Padding = new Thickness ( 10, Device.OnPlatform<int>( 20, 0, 0 ), 10, 0 ),
			};

			SaveButton.Clicked += async (sender, e) => {
				foreach (var property in source){
					PropertyInfo propertyInfo = viewToEdit.GetType().GetRuntimeProperty(property.name);
						try{
							propertyInfo.SetValue(viewToEdit, Convert.ChangeType(property.value, propertyInfo.PropertyType), null);
						}catch(Exception){

						}
				}
				await Navigation.PopModalAsync();
			};

			CancelButton.Clicked += async (sender, e) => {
				await Navigation.PopModalAsync();
			};

		}
	}
}

