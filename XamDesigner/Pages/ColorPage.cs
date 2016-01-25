using System;
using System.Reflection;
using System.Linq;

using Xamarin.Forms;

namespace XamDesigner
{
	public class ColorPage : ContentPage
	{
		Button SaveButton, CancelButton;
		Slider redSlider, blueSlider, greenSlider;
		BoxView colorBox;
		Picker picker;

		public Color CurrentColor { get; set;}
		public PropertyInfo CurrentProperty { get; set; }
		public ColorPage (View viewToEdit)
		{
			SaveButton = new Button(){ Text="Save", HorizontalOptions=LayoutOptions.Center };
			CancelButton = new Button() { Text="Cancel", HorizontalOptions=LayoutOptions.Center};
			redSlider = new Slider (0, 255, 0);
			greenSlider = new Slider (0, 255, 0);
			blueSlider = new Slider (0, 255, 0);

			picker = new Picker ();
			var properties = viewToEdit.GetType ().GetRuntimeProperties ();

			picker.SelectedIndexChanged+= (sender, e) => {
				CurrentProperty = (from property in properties
						where property.Name == picker.Items[picker.SelectedIndex]
					select property).FirstOrDefault();
			};

			foreach (var property in properties) {
				if (property.PropertyType == typeof(Color)) {
					picker.Items.Add (property.Name);
				}
			}

			if (picker.Items.Count > 0) {
				picker.SelectedIndex = 0;
			}

			colorBox = new BoxView ();
			blueSlider.ValueChanged+= SliderValueChanged;
			redSlider.ValueChanged += SliderValueChanged;
			greenSlider.ValueChanged += SliderValueChanged;

			Content = new StackLayout () {
				Children = {picker, colorBox, redSlider, greenSlider, blueSlider, SaveButton
					, CancelButton
				},
				Padding = new Thickness ( 0, Device.OnPlatform<int>( 20, 0, 0 ), 0, 0 ),
			};

			SaveButton.Clicked += async (sender, e) => {
				CurrentProperty.SetValue (viewToEdit, CurrentColor, null);
				await Navigation.PopModalAsync();
			};

			CancelButton.Clicked += async (sender, e) => {
				await Navigation.PopModalAsync();
			};
		}

		void SliderValueChanged (object sender, ValueChangedEventArgs e)
		{
			CurrentColor = Color.FromRgb((int)redSlider.Value, (int)greenSlider.Value, (int)blueSlider.Value);
			colorBox.BackgroundColor = CurrentColor;
		}


	}
}


