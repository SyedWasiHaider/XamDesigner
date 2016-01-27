using System;
using System.Reflection;
using System.Linq;

using Xamarin.Forms;

namespace XamDesigner
{
	public class ColorPage : ContentPage
	{
		Button DoneButton;
		Slider redSlider, blueSlider, greenSlider;
		BoxView colorBox;
		Picker picker;

		public Color CurrentColor { get; set;}
		public PropertyInfo CurrentProperty { get; set; }
		View viewToEdit;
		public ColorPage (View viewToEdit)
		{
			this.viewToEdit = viewToEdit;
			DoneButton = new Button(){ Text="Done", HorizontalOptions=LayoutOptions.Center };
			redSlider = new Slider (0, 255, 0);
			greenSlider = new Slider (0, 255, 0);
			blueSlider = new Slider (0, 255, 0);

			picker = new Picker ();
			var properties = viewToEdit.GetType ().GetRuntimeProperties ();

			picker.SelectedIndexChanged+= (sender, e) => {
				CurrentProperty = (from property in properties
						where property.Name == picker.Items[picker.SelectedIndex]
					select property).FirstOrDefault();
				var color = (Color)CurrentProperty.GetValue(viewToEdit);
				redSlider.Value = (int)(color.R * 255);
				greenSlider.Value = (int)(color.G * 255);
				blueSlider.Value = (int)(color.B * 255);
			};

			foreach (var property in properties) {
				if (property.PropertyType == typeof(Color)) {
					picker.Items.Add (property.Name);
				}
			}

			if (picker.Items.Count > 0) {
				picker.SelectedIndex = 0;
				var color = (Color)CurrentProperty.GetValue(viewToEdit);
				redSlider.Value = (int)(255*color.R);
				greenSlider.Value = (int)(255*color.G);
				blueSlider.Value = (int)(255*color.B);
			}

			colorBox = new BoxView ();
			blueSlider.ValueChanged+= SliderValueChanged;
			redSlider.ValueChanged += SliderValueChanged;
			greenSlider.ValueChanged += SliderValueChanged;
			CurrentColor = Color.FromRgb((int)redSlider.Value, (int)greenSlider.Value, (int)blueSlider.Value);

			Content = new StackLayout () {
				Children = {picker, colorBox, redSlider, greenSlider, blueSlider, DoneButton
				},
				Padding = new Thickness ( 0, Device.OnPlatform<int>( 20, 0, 0 ), 0, 0 ),
			};

			DoneButton.Clicked += async (sender, e) => {
				CurrentProperty.SetValue (viewToEdit, CurrentColor, null);
				await Navigation.PopModalAsync();
			};
		}

		void SliderValueChanged (object sender, ValueChangedEventArgs e)
		{
			CurrentColor = Color.FromRgb((int)redSlider.Value, (int)greenSlider.Value, (int)blueSlider.Value);
			colorBox.BackgroundColor = CurrentColor;
			CurrentProperty.SetValue (viewToEdit, CurrentColor);
		}


	}
}


