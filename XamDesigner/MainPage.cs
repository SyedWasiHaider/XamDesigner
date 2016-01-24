using System;

using Xamarin.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace XamDesigner
{
	public class MainPage : ContentView
	{

		enum ACTION {
			ADD, MOVE, RESIZE, COLOR, DELETE, MODE
		}

		public delegate void OptionDelegate();
		MR.Gestures.AbsoluteLayout layout = null;
		string activeType;
		Dictionary<string,string> dict;
		bool editMode = true;
		StackLayout activeView;
		MR.Gestures.Grid MenuGrid;
		bool menuVisible = false;
		ACTION currAction = ACTION.ADD;
		string [] options = new string[] { "Add", "Move", "Resize", "Colors", "Delete", "Change Mode" };
		Color tapOptionColor = Color.FromRgb (17, 24, 31);
		Color unTapOptionColor = Color.FromRgb (20, 60, 80);

		OptionDelegate[] delegates;

		public MainPage ()
		{
		}

		bool stupid = false;
		protected override void OnSizeAllocated (double width, double height)
		{
			base.OnSizeAllocated (width, height);

			if (!stupid) {
				stupid = true;
				Setup ();
				AddMenu ();
			}
		}

		public void Setup(){
			dict = App.SupportedTypes;
			delegates = new OptionDelegate[] {
				async delegate {
					currAction = ACTION.ADD;
					var action = await DisplayActionSheet ("What control?", "Cancel", null, dict.Keys.ToArray<string>());
					if (dict.Keys.Contains(action)){
						activeType = dict[action];
					}
				},
				delegate {
					currAction = ACTION.MOVE;
				},
				delegate {
					currAction = ACTION.RESIZE;
				},
				delegate {
					currAction = ACTION.COLOR;
				},
				delegate {
					currAction = ACTION.DELETE;
				},
				delegate {
					currAction = ACTION.MODE;
					editMode = !editMode;
					Title = editMode ? "Edit Mode" : "Preview Mode";
				}
			};

			activeType = typeof(Button).AssemblyQualifiedName;
			layout = new MR.Gestures.AbsoluteLayout  {};
			layout.Tapped += HandleTapOnLayout;


			double diffY = 0;
			layout.Pinching += (sweet, cool) => {
				if (editMode && activeView!=null && currAction == ACTION.RESIZE){
					Debug.WriteLine("hey hey hey" + activeView.Bounds.Center);
					var actualView = activeView.Children.FirstOrDefault();
					var finger1 = cool.Touches[0];
					var finger2 = cool.Touches[1];
					if (Math.Abs(finger1.X - finger2.X) > Math.Abs(finger1.Y - finger2.Y)){
						actualView.WidthRequest = actualView.Width * cool.DeltaScale;
					}else{
						var newDiff = finger1.Y - finger2.Y;
					
							if (newDiff > diffY){
							actualView.HeightRequest = actualView.Height * 1.1;
							}else{
							actualView.HeightRequest = actualView.Height * 0.9;
							}
						diffY = newDiff;

					}
				
				}
			};


			MessagingCenter.Subscribe<MenuPage, String> (this, App.ChangeControlMessage, delegate (MenuPage Page, string message) {
				activeType = message;
			});

			MessagingCenter.Subscribe<App, String> (this, App.ToggleOptionsDrawer, delegate (App Page, string message) {
				
				if (menuVisible){

					var an = new Animation( delegate(double obj) {
						MenuGrid.TranslationX = obj;
					}, 0, MenuGrid.Width);

					an.Commit(MenuGrid, "menugrid", 10, 200);
				}else{
					var an = new Animation( delegate(double obj) {
						MenuGrid.TranslationX = obj;
					}, MenuGrid.TranslationX, 0);

					an.Commit(MenuGrid, "menugrid", easing: Easing.BounceOut);
				}
				menuVisible = !menuVisible;

			});

			Content = layout;
		}

		private void AddMenu(){
			int numButtons = options.Count();

			MenuGrid = new MR.Gestures.Grid ();
			MenuGrid.RowSpacing = 0;
			MenuGrid.ColumnDefinitions.Add (new ColumnDefinition ());
			for (int i = 0; i < numButtons; i++) {
				MenuGrid.RowDefinitions.Add (new RowDefinition ());
				var innerGrid = new Grid () { BackgroundColor = Color.FromRgb(20, 60, 80) };
				innerGrid.ColumnDefinitions.Add (new ColumnDefinition ());
				innerGrid.RowDefinitions.Add (new RowDefinition ());
				var box = new MR.Gestures.BoxView ();
				Label lol = new Label (){ TextColor = Color.White, Text = options [i], 
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Fill, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center};
				innerGrid.Children.Add (box,0,0);
				innerGrid.Children.Add (lol,0,0);
				MenuGrid.Children.Add (innerGrid, 0, i);
			}

			MenuGrid.BackgroundColor = Color.Pink;
			AbsoluteLayout.SetLayoutFlags (MenuGrid,
				AbsoluteLayoutFlags.PositionProportional);

			AbsoluteLayout.SetLayoutBounds (MenuGrid,
				new Rectangle (1f,
					0f, AbsoluteLayout.AutoSize, Height));

			layout.Children.Add (MenuGrid);
			MenuGrid.WidthRequest = Height / numButtons;
			MenuGrid.HeightRequest = Height;
			MenuGrid.TranslationX = MenuGrid.Width;

		}

		public void DeleteControl(MR.Gestures.TapEventArgs e = null, View childToDelete = null){


			var children = layout.Children;
			if (childToDelete == null) {
				var x = e.Center.X;
				var y = e.Center.Y;
				foreach (var child in children) {
					if (child.Bounds.Contains (x, y)) {
						childToDelete = child;
						break;
					}
				}
			}

			if (childToDelete != null) {
				children.Remove (childToDelete);
			}
		}

		public void AddControl(MR.Gestures.TapEventArgs e){
			var x = e.Center.X;
			var y = e.Center.Y;

			//Another added control has been touched.
			foreach (var child in layout.Children) {
				if (child.Bounds.Contains (x, y) && child != MenuGrid) {
					return;
				}
			}

			MR.Gestures.StackLayout someView = new MR.Gestures.StackLayout() {
				BackgroundColor=Color.Transparent};


			if (activeType == dict["Button"]){
				var button = new Button (){ Text = "Button" };
				button.Clicked += (haha, snap) => {
					if (editMode && !menuVisible){
						if (activeView != someView){
							activeView = someView;
						}

						if (currAction == ACTION.DELETE) {
							DeleteControl (childToDelete: activeView);
						}

					}else if  (menuVisible){
							MessagingCenter.Send<App, String> (App.Current as App, App.ToggleOptionsDrawer, App.ToggleOptionsDrawer);
					}

				};
				someView.Children.Add(button);


			}else if (activeType == dict["Label"]){
				someView.Children.Add(new Label (){ Text = "Label" });
			}else if (activeType == dict["Entry"]){
				someView.Children.Add(new Entry (){ Text = "Entry" });
			}else if (activeType == dict["Rectangle"]){
				someView.Children.Add(new BoxView (){ BackgroundColor = Color.Yellow });
			}
			activeView = someView;
			someView.Tapped += (nice, man) => {
				if (editMode && man.NumberOfTouches==1){
					if (activeView != someView){
						activeView = someView;
					}
				}
			};

			AbsoluteLayout.SetLayoutFlags (someView,
				AbsoluteLayoutFlags.None);

			AbsoluteLayout.SetLayoutBounds (someView,
				new Rectangle (x,
					y, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));

			someView.Panning += (control, args) => {
				if (editMode && currAction == ACTION.MOVE){
					if (activeView != someView){
						activeView = someView;
					}

					var x0 = someView.X + args.TotalDistance.X;
					var y0 = someView.Y+ args.TotalDistance.Y;
					AbsoluteLayout.SetLayoutBounds (someView,
						new Rectangle (x0,
							y0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
				}
			};


			layout.Children.Insert (0, someView);
		}

		bool HandleMenuGrid (MR.Gestures.TapEventArgs e){
			var x = e.Center.X;
			var y = e.Center.Y;

			//Menu is touched
			if (MenuGrid.Bounds.Contains (x, y)) {
				var transX = Width - x;
				var children = MenuGrid.Children;


				for (int index =0; index < children.Count; index++) {
					var child = children [index];

					bool toggle = false;

				
					if (child.Bounds.Contains (transX, y)) {
					    delegates [index] ();
						child.BackgroundColor = tapOptionColor;

					} else {
						child.BackgroundColor = unTapOptionColor;
					}

				}
				return true;
			}

			return false;
		}

		public void HandleControlManipulations(MR.Gestures.TapEventArgs e){
			if (currAction == ACTION.ADD) {
				AddControl (e);
			} else if (currAction == ACTION.DELETE) {
				DeleteControl (e);
			}
		}

		public void HandleTapOnLayout(object sender, MR.Gestures.TapEventArgs e){

			bool tappedOnMenu = false;
			if (menuVisible) {
				tappedOnMenu = HandleMenuGrid (e);
				if (!tappedOnMenu) {
					var app = App.Current as App;
					MessagingCenter.Send<App, String> (app, App.ToggleOptionsDrawer, App.ToggleOptionsDrawer);
					return;
				}
			}else if (editMode) {
					HandleControlManipulations (e);
			}

			}

	}
}


