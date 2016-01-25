using System;
using MR.Gestures;
using Xamarin.Forms;
using System.Diagnostics;
using System.Collections.Generic;

namespace XamDesigner
{
	public class OptionsMenu : MR.Gestures.Grid
	{

		public static readonly BindableProperty OptionsListProperty =
			BindableProperty.Create<OptionsMenu, List <string>> (p => p.OptionsList, new List<string>());

		public List <string> OptionsList {
			get { return (List <string>)GetValue (OptionsListProperty); }
			set { SetValue (OptionsListProperty, value); }
		}

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
			this.Tapped += OptionsMenu_Tapped;
		}

		protected override void OnPropertyChanged (string propertyName)
		{
			base.OnPropertyChanged (propertyName);
			Debug.WriteLine ("PropertyChanged was: " + propertyName);
			if  (propertyName == "Parent") {
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
			var untappedColor = Color.FromRgb (20, 60, 80);
			var tappedColor = Color.FromRgb(40, 80, 100);
			Children.Clear ();
			RowSpacing = 0;
			ColumnDefinitions.Add (new ColumnDefinition ());
			for (int i = 0; i < numButtons; i++) {
				RowDefinitions.Add (new RowDefinition ());
				var innerGrid = new MR.Gestures.Grid () { BackgroundColor = i== 0 ? tappedColor : untappedColor };
				innerGrid.ColumnDefinitions.Add (new ColumnDefinition ());
				innerGrid.RowDefinitions.Add (new RowDefinition ());
				var box = new MR.Gestures.BoxView ();
				var lol = new MR.Gestures.Label (){ TextColor = Color.White, Text = OptionsList[i], 
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Fill, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center};
				innerGrid.Children.Add (box,0,0);
				innerGrid.Children.Add (lol,0,0);
				IdPositionDictionary.Add (innerGrid.Id, i);
				innerGrid.Tapped+= (object sender, TapEventArgs e) => {
					if (ItemTapped != null){
						ItemTapped(this, new OptionTappedEventArgs(){ Position = IdPositionDictionary[((MR.Gestures.Grid)sender).Id] });
					}
					Children.DoForEach(delegate(object grid){
						((MR.Gestures.Grid)grid).BackgroundColor = untappedColor; 
					});
					innerGrid.BackgroundColor = tappedColor;
				
					ToggleMenu();
				};
				Children.Add (innerGrid, 0, i);
			}

			HeightRequest = (Double)(Parent.GetValue (HeightProperty));
			WidthRequest = (Double)(HeightRequest)/ numButtons;
			TranslationX = WidthRequest;
		}

		public void OptionsMenu_Tapped (object sender, TapEventArgs e)
		{
			
		}

		public void ToggleMenu(){
			if (IsMenuVisible){

				var an = new Animation( delegate(double obj) {
					TranslationX = obj;
				}, 0, Width);

				an.Commit(this, "menugrid", 10, 200);
			}else{
				var an = new Animation( delegate(double obj) {
					TranslationX = obj;
				},TranslationX, 0);

				an.Commit(this, "menugrid", easing: Easing.BounceOut);
			}
			IsMenuVisible = !IsMenuVisible;
		}

	}
}

