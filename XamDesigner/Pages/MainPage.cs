using System;

using Xamarin.Forms;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;

namespace XamDesigner
{
	public class MainPage : ContentPage
	{
		[FlagsAttribute] 
		enum ACTION {
			ADD = 0, MOVE=1, RESIZE=2, FREEFORM=4 ,COLOR=8, DELETE=16, MODE = 32
		}

		MR.Gestures.AbsoluteLayout layout = null;
		string activeType;
		Dictionary<string,string> dict;
		bool editMode = true;
		StackLayout activeView;
		MR.Gestures.Grid MenuGrid;
		bool menuVisible = false;
		ACTION currAction = ACTION.ADD;
		string [] options = new string[] { "Add", "Move", "Resize", "Free Form", "Colors", "Delete", "Change Mode" };
		Color tapOptionColor = Color.FromRgb (17, 24, 31);
		Color unTapOptionColor = Color.FromRgb (20, 60, 80);

		XamDesigner.EnumerableExtensions.GenericDelegate<object>[] optionDelegates;

		public MainPage ()
		{
		}

		bool stupid = false;

		private async Task StupidestThingEver(){
			await Task.Delay (500);
			Setup ();
			AddMenu ();
		}

		protected override void OnSizeAllocated (double width, double height)
		{
			base.OnSizeAllocated (width, height);
			if (!stupid) {
				stupid = true;
				StupidestThingEver ();
			}
		}

		public void Setup(){
			dict = App.SupportedTypes;
			optionDelegates = new XamDesigner.EnumerableExtensions.GenericDelegate<object>[] {
				async delegate {
					currAction = ACTION.ADD;
					var action = await DisplayActionSheet ("What control?", "Cancel", null, dict.Keys.ToArray<string>());
					if (action != null && dict.Keys.Contains(action)){
						activeType = dict[action];
					}
					TogglePreviewEdit();
				},
				delegate {
					currAction = ACTION.MOVE;
					TogglePreviewEdit();
				},
				delegate {
					currAction = ACTION.RESIZE;
					TogglePreviewEdit();
				},
				async delegate {
					currAction = ACTION.FREEFORM;
					var action = await DisplayActionSheet ("What control?", "Cancel", null, dict.Keys.ToArray<string>());
					if (action != null && dict.Keys.Contains(action)){
						activeType = dict[action];
					}
					TogglePreviewEdit();
				},
				delegate {
					currAction = ACTION.COLOR;
					TogglePreviewEdit();
				},
				delegate {
					currAction = ACTION.DELETE;
					TogglePreviewEdit();
				},
				delegate {
					currAction = ACTION.MODE;
					TogglePreviewEdit(false);
				}
			};

			activeType = typeof(Button).AssemblyQualifiedName;
			layout = new MR.Gestures.AbsoluteLayout  {};
			layout.Tapped += HandleTapOnLayout;
			layout.LongPressing += Layout_LongPressed;

			double diffY = 0;
			layout.Pinching += (sweet, cool) => {

				if (menuVisible){
					ToggleMenu();
				}

				if (editMode && activeView!=null && (currAction == ACTION.RESIZE || currAction == ACTION.FREEFORM)){
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

		void Layout_LongPressed (object sender, MR.Gestures.LongPressEventArgs e)
		{
			if (editMode && currAction == ACTION.FREEFORM) {

				var x = e.Center.X;
				var y = e.Center.Y;
				var children = layout.Children;
				foreach(var item in children){
					if (item.GetType() == typeof(MR.Gestures.StackLayout) && item.Id == activeView.Id){
						if (item.Bounds.Contains(x,y)){
							layout.LowerChild (item);
							Debug.WriteLine ("Child lowered " + item.Id);
							break;
						}
				}
			}
			

		}
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


		public void TogglePreviewEdit(bool forceEdit = true){
			editMode = !editMode || forceEdit;
			Title = editMode ? "Edit Mode" : "Preview Mode";
			if (!editMode){
				layout.Children.DoForEach (delegate (object layout) {
					var element = (layout as StackLayout).Children [0];
					if (element.GetType() == typeof(Entry)){
						(element as Entry).IsEnabled = true;
					}
				}, typeof(MR.Gestures.StackLayout));
				ToggleEffect ();
			}else{
				layout.Children.DoForEach (delegate (object layout) {
					var element = (layout as StackLayout).Children [0];
					if (element.GetType() == typeof(Entry)){
						(element as Entry).IsEnabled = false;
					}
				}, typeof(MR.Gestures.StackLayout));
			}
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
				if (childToDelete == null) {
					return;
				}
			}

			if (childToDelete.Id != activeView.Id) {
				SetActiveView (activeView);
				return;
			}

			if (childToDelete != null) {
				children.Remove (childToDelete);
			}
		}

		public void ColorControl(MR.Gestures.TapEventArgs e = null, View childToColor = null){

			if (e != null) {
				var x = e.Center.X;
				var y = e.Center.Y;
				var children = layout.Children;
				foreach (var child in children) {
					if (child.Bounds.Contains (x, y) && child.GetType() == typeof(MR.Gestures.StackLayout)) {
						SetActiveView (child as StackLayout);
						childToColor = (child as StackLayout).Children [0];
						break;
					}
				}
			}

			if (childToColor != null) {
				var properties = childToColor.GetType ().GetRuntimeProperties ();
				foreach (var property in properties) {
					var ran = new Random ();
					if (property.PropertyType == typeof(Color))
						property.SetValue (childToColor, Color.FromRgb (
							(byte)(ran.NextDouble () * 255), 
							(byte)(ran.NextDouble () * 255),
							(byte)(ran.NextDouble () * 255)), null);
				}
			}
		}

		public void AddControl(MR.Gestures.TapEventArgs e){
			var x = e.Center.X;
			var y = e.Center.Y;

			//The tap event was either directed towards another control or the menu was touched.
			foreach (var child in layout.Children) {
				if (child.Bounds.Contains (x, y) && child != MenuGrid) {
					return;
				}
			}

			MR.Gestures.StackLayout someView = new MR.Gestures.StackLayout() {
				BackgroundColor=Color.Transparent};

			//TODO: Find a better way than this ugly hack.
			//Need to handle button case seperately for now
			if (activeType == dict["Button"]){
				var button = new Button (){ Text = "Button" };
				button.Clicked += (haha, snap) => {
					if (editMode && !menuVisible){


						if (someView.Id == activeView.Id && currAction == ACTION.DELETE) {
							DeleteControl (childToDelete: activeView);
						}else if(someView.Id == activeView.Id && currAction == ACTION.COLOR) {
							ColorControl(childToColor: button);
						}else {
							SetActiveView(someView);
						}

					}else if  (menuVisible){
						ToggleMenu();
					}

				};
				someView.Children.Add(button);


			}else if (activeType == dict["Label"]){
				someView.Children.Add(new Label (){ Text = "Label", HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Fill, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center});
			}else if (activeType == dict["Entry"]){
				someView.Children.Add(new Entry (){ Text = "Entry", IsEnabled = false });
			}else if (activeType == dict["Rectangle"]){
				someView.Children.Add(new BoxView (){ BackgroundColor = Color.Yellow });
			}
			SetActiveView (someView);
			someView.Tapped += (nice, man) => {
				if (editMode && man.NumberOfTouches==1){
					SetActiveView (someView);
				}
			};

			AbsoluteLayout.SetLayoutFlags (someView,
				AbsoluteLayoutFlags.None);

			AbsoluteLayout.SetLayoutBounds (someView,
				new Rectangle (x,
					y, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));

			someView.Panning += (control, args) => {

				if (menuVisible){
					ToggleMenu();
				}

				if (editMode && (currAction == ACTION.MOVE || currAction == ACTION.FREEFORM)){
					SetActiveView(someView);

					var x0 = someView.X + args.TotalDistance.X;
					var y0 = someView.Y+ args.TotalDistance.Y;
					AbsoluteLayout.SetLayoutBounds (someView,
						new Rectangle (x0,
							y0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
				}
			};


			layout.Children.Insert (0, someView);
		}

		public void ToggleEffect(StackLayout someView = null){

			foreach (var view in layout.Children) {
				if (view.GetType () == typeof(MR.Gestures.StackLayout)) {
					while (view.Effects.Count > 0) {
						view.Effects.RemoveAt (0);
					}
				}
			}

			if (someView != null) {
				Effect BorderEffect = DependencyService.Get<Effect> (DependencyFetchTarget.NewInstance);
				someView.Effects.Add (BorderEffect);
			}
		}

		public void SetActiveView(StackLayout someView){
			if (someView != null && (activeView == null || activeView.Id != someView.Id)){
				activeView = someView;
			}
			ToggleEffect (someView);
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
					if (child.Bounds.Contains (transX, y)) {
					    optionDelegates [index] (null);
						child.BackgroundColor = tapOptionColor;

					} else {
						child.BackgroundColor = unTapOptionColor;
					}

				}
				ToggleMenu ();
				return true;
			}

			return false;
		}

		public void HandleControlManipulations(MR.Gestures.TapEventArgs e){
			if (currAction == ACTION.ADD || currAction == ACTION.FREEFORM) {
				AddControl (e);
			} else if (currAction == ACTION.DELETE) {
				DeleteControl (e);
			} else if (currAction == ACTION.COLOR) {
				ColorControl (e);
			}
		}

		public void ToggleMenu(){
			var app = App.Current as App;
			MessagingCenter.Send<App, String> (app, App.ToggleOptionsDrawer, App.ToggleOptionsDrawer);
		}

		public void HandleTapOnLayout(object sender, MR.Gestures.TapEventArgs e){

			bool tappedOnMenu = false;
			if (menuVisible) {
				tappedOnMenu = HandleMenuGrid (e);
				if (!tappedOnMenu) {
					ToggleMenu ();
					return;
				}
			}else if (editMode) {
					HandleControlManipulations (e);
			}

			}


	}
}


