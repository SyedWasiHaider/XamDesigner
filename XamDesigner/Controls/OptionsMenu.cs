using System;
using MR.Gestures;
using Xamarin.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.BrandColors;

namespace XamDesigner
{
	public class OptionsMenu : MR.Gestures.Grid
	{

		public static readonly BindableProperty OptionsListProperty =
			BindableProperty.Create<OptionsMenu, List <MenuOptionModel>> (p => p.OptionsList, new List<MenuOptionModel>());

		public List <MenuOptionModel> OptionsList {
			get { return (List <MenuOptionModel>)GetValue (OptionsListProperty); }
			set { SetValue (OptionsListProperty, value); }
		}

		public bool[] ToggleAble;

		public static readonly BindableProperty IsMenuVisibleProperty =
			BindableProperty.Create<OptionsMenu, bool> (p => p.IsMenuVisible, false);

		public bool IsMenuVisible {
			get { return (bool)GetValue (IsMenuVisibleProperty); }
			set { SetValue (IsMenuVisibleProperty, value); }
		}

		Dictionary <Guid, int> IdPositionDictionary = new Dictionary <Guid, int>();
		public class OptionTappedEventArgs {
			public int Position {
				get;
				set;
			}
		}
		public event EventHandler<OptionTappedEventArgs> ItemTapped;

		public OptionsMenu ()
		{
			IsMenuVisible = false;
		}

		protected override void OnPropertyChanged (string propertyName)
		{
			base.OnPropertyChanged (propertyName);
			Debug.WriteLine ("PropertyChanged was: " + propertyName);
			if (propertyName == "Parent") {
				Setup ();
			}
		}

		int GetTappedIndex (MR.Gestures.TapEventArgs e){
			var relativeX = e.Center.X;
			var x = X + e.Center.X;
			var y = e.Center.Y;

			if (Bounds.Contains (x, y)) {
				var children = Children;


				for (int index =0; index < children.Count; index++) {
					var child = children [index];
					if (child.Bounds.Contains (relativeX, y)) {
						return index;
					}

				}
			}

			return -1;
		}



		Color untappedColor = XamarinColor.DarkerBlue.getColor ();
		Color tappedColor = XamarinColor.DarkBlue.getColor ();
		void Setup(){

			if (OptionsList.Count <= 0) {
				throw new ArgumentNullException ("You must add the options to the menu before adding it to the parent view");
			}

			if (Parent != null) {
				if ((Double)(Parent.GetValue (HeightProperty)) < 0) {
					Parent.PropertyChanged += (sender, e) => {
						if (e.PropertyName == HeightProperty.PropertyName) {
							Setup ();
						}
					};
					return;
				}
			} else if (Parent == null){
				return;
			}

			var numButtons = OptionsList.Count;
			HeightRequest = (Double)(Parent.GetValue (HeightProperty));
			WidthRequest = (Double)(HeightRequest)/ numButtons;
			Children.Clear ();
			RowSpacing = 0;
			ColumnDefinitions.Add (new ColumnDefinition ());
			for (int i = 0; i < numButtons; i++) {
				RowDefinitions.Add (new RowDefinition ());
				var innerGrid = new MR.Gestures.Grid () { BackgroundColor = untappedColor };
				innerGrid.ColumnDefinitions.Add (new ColumnDefinition ());
				innerGrid.RowDefinitions.Add (new RowDefinition ());
				var box = new MR.Gestures.BoxView ();
				var lol = new MR.Gestures.Label (){ TextColor = Color.White, Text = OptionsList[i].Title, 
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Fill, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center};
				innerGrid.Children.Add (box,0,0);
				innerGrid.Children.Add (lol,0,0);
				innerGrid.TranslationX = WidthRequest;
				IdPositionDictionary.Add (innerGrid.Id, i);
				innerGrid.Tapped+= 
					async (object sender, TapEventArgs e) => {
					var pos = IdPositionDictionary[((MR.Gestures.Grid)sender).Id];
					if (ItemTapped != null){
						ItemTapped(this, new OptionTappedEventArgs(){ Position = pos });
					}


					if (OptionsList[pos].IsToggleable){

						if (!OptionsList[pos].IsToggled && OptionsList[pos].UnToggleOthers){
							SetToggledAll(false);
						}

						SetToggled(pos, !OptionsList[pos].IsToggled);
						await ShowHideMenu();
					}else{
						SetToggledAll(false);
						innerGrid.BackgroundColor = tappedColor;
						innerGrid.Animate("wtifisthisanyway", new Animation(delegate(double obj) {
							innerGrid.Scale = obj;
						}, 0.90,1), easing: Easing.SpringOut, length:200, finished: 
						async delegate{
							await ShowHideMenu();
							innerGrid.BackgroundColor = untappedColor;
						});
					}
				
				};
				Children.Add (innerGrid, 0, i);
			}

			SetToggled (1, true);
			SetToggled (2, true);
		}

		public void SetToggled(int index, bool toggle){
			((MR.Gestures.Grid)Children [index]).BackgroundColor = toggle ? tappedColor : untappedColor;
			OptionsList [index].IsToggled = toggle;
		}

		public void SetToggledAll(bool toggle){
			Children.DoForEach(delegate(object grid, int index){
				SetToggled(index, toggle);
			});
		}

		bool isAnimating = false;
		public async Task ShowHideMenu(){

			int index = 0;
			int completed = 0;
			foreach (var innerGrid in Children) {
				if (IsMenuVisible) {

					var an = new Animation (delegate(double obj) {
						innerGrid.TranslationX = obj;
					}, 0, innerGrid.Width);

					an.Commit (innerGrid, "menugrid", 10, 200);
				} else {
					var an = new Animation (delegate(double obj) {
						innerGrid.TranslationX = obj;
					}, innerGrid.TranslationX, 0);

					await Task.Delay(index * 13);
					index++;
					an.Commit (innerGrid, "menugrid"+innerGrid.Id, easing: Easing.SinIn, length:90, rate:13);
				}
			}
			IsMenuVisible = !IsMenuVisible;
		}

	}
}

