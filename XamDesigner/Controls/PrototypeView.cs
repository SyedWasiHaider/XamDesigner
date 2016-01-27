using System;

using Xamarin.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using Plugin.Media;
using Plugin.Media.Abstractions;

namespace XamDesigner
{
	public class PrototypeView : MR.Gestures.ContentView
	{
		
		enum ACTION {
			MOVE, RESIZE, FREEFORM, DELETE,
		} 

		MR.Gestures.AbsoluteLayout layout = null;
		string activeType;
		Dictionary<string,string> dict;
		bool editMode = true;
		StackLayout activeView;
		ACTION currAction = ACTION.FREEFORM;
		public List <string> options;
		public bool[] toggleable;
		XamDesigner.EnumerableExtensions.GenericDelegate<object>[] optionDelegates;

		public PrototypeView ()
		{
			Setup ();
		}

		public void ExecuteAction(int action){
			optionDelegates[action] (null);
			TogglePreviewEdit();
		}

		void Setup(){
			dict = App.SupportedTypes;
			options = new List<string> { "Add", "Move", "Resize", "Free Form", "Clone", "Colors", "Delete", "More" };
			toggleable = new bool[options.Count];
			for (int i = 0; i < toggleable.Count(); i++) {
				toggleable[i] = false;
			}

			//Why? Why not!?
			toggleable [options.IndexOf ("Move")] = 
				toggleable [options.IndexOf ("Resize")] = 
					toggleable [options.IndexOf ("Free Form")] = 
						toggleable [options.IndexOf ("Delete")] = true;
			
			optionDelegates = new XamDesigner.EnumerableExtensions.GenericDelegate<object>[] {
				async delegate {
					currAction = ACTION.FREEFORM;
					var action = await App.Current.MainPage.DisplayActionSheet ("Pick a control to add.", "Cancel", null, dict.Keys.ToArray<string>());
					if (action != null && dict.Keys.Contains(action)){
						activeType = dict[action];
						AddControl(Width/2, Height/2);
					}
				},
				delegate {
					currAction = ACTION.MOVE;
				},
				delegate {
					currAction = ACTION.RESIZE;
				},
				delegate {
					currAction = ACTION.FREEFORM;
				},
				delegate {
					if (activeView == null){
						App.Current.MainPage.DisplayAlert ("Woah There!", "You must select an element first.", "OK");
					}else{
						AddControl(Width/2, Height/2, activeView.Children[0]);
					}
					currAction = ACTION.FREEFORM;
				},
				delegate {
					if (activeView == null){
						App.Current.MainPage.DisplayAlert ("Woah There!", "You must select an element first.", "OK");
					}else{
						(App.Current.MainPage as MasterDetailPage).Detail.Navigation.PushModalAsync (new ColorPage (activeView.Children[0]));
					}
					currAction = ACTION.FREEFORM;
				},
				delegate {
					currAction = ACTION.DELETE;
				},
				delegate {
					if (activeView == null){
						App.Current.MainPage.DisplayAlert ("Woah There!", "You must select an element first.", "OK");
					}else{
						(App.Current.MainPage as MasterDetailPage).Detail.Navigation.PushModalAsync(new EditPropertiesPage(activeView.Children[0]));
					}
					currAction = ACTION.FREEFORM;
				}
			};

			activeType = typeof(Button).AssemblyQualifiedName;
			layout = new MR.Gestures.AbsoluteLayout  {};
			layout.Tapped += HandleTapOnLayout;
			layout.LongPressing += Layout_LongPressed;


			layout.Pinching += (sweet, cool) => {

				if (editMode && activeView!=null && (currAction == ACTION.RESIZE || currAction == ACTION.FREEFORM)){
					Debug.WriteLine("hey hey hey" + activeView.Bounds.Center);
					var actualView = activeView.Children.FirstOrDefault();
					var finger1 = cool.Touches[0];
					var finger2 = cool.Touches[1];
					if (Math.Abs(finger1.X - finger2.X) > Math.Abs(finger1.Y - finger2.Y)){
						actualView.WidthRequest = actualView.Width * cool.DeltaScale;
					}else{
						actualView.HeightRequest = actualView.Height * cool.DeltaScale;
					}

				}
			};

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

		public void TogglePreviewEdit(bool forceEdit = true){
			editMode = !editMode || forceEdit;
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
			
		public void AddControl(MR.Gestures.TapEventArgs e){
			var x = e.Center.X;
			var y = e.Center.Y;
			//The tap event was either directed towards another control or the menu was touched.
			foreach (var child in layout.Children) {
				if (child.Bounds.Contains (x, y)) {
					return;
				}
			}
			AddControl (x, y);
		}


		public async void AddControl(double x, double y, View viewToAdd = null){

			bool clone = false;
			if (viewToAdd != null) {
				activeType = viewToAdd.GetType ().AssemblyQualifiedName;
				clone = true;
			}

			MR.Gestures.StackLayout someView = new MR.Gestures.StackLayout() {
				BackgroundColor=Color.Transparent};



			//TODO: Find a better way than this ugly hack.
			//Need to handle button case seperately for now
			if (activeType == dict ["Button"]) {
				var button = new Button (){ Text = "Button" };

				button.Clicked += (haha, snap) => {
					if (editMode) {

						if (someView.Id == activeView.Id && currAction == ACTION.DELETE) {
							DeleteControl (childToDelete: activeView);
						} else {
							SetActiveView (someView);
						}

					}

				};
				someView.Children.Add (button);


			} else if (activeType == dict ["Label"]) {
				someView.Children.Add (new Label () {
					Text = "Label",
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Fill,
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center
				});
			} else if (activeType == dict ["Entry"]) {
				someView.Children.Add (new Entry (){ Text = "Entry", IsEnabled = false });
			} else if (activeType == dict ["BoxView"]) {
				someView.Children.Add (new BoxView (){ BackgroundColor = Color.Blue });
			} else if (activeType == dict ["Switch"]) {
				someView.Children.Add (new Switch ());
			} else if (activeType == dict ["Image"]) {
				if (clone && ((Image)viewToAdd).Source != null) {
					someView.Children.Add (new Image ());
				} else if (CrossMedia.Current.IsCameraAvailable) {
					var imageView = new Image ();
					var file = await CrossMedia.Current.TakePhotoAsync (new StoreCameraMediaOptions () {
						Directory = "XamDesigner",
						Name = imageView.Id.ToString () + "quickie.jpg"
					});

					imageView.Source = ImageSource.FromStream (() => {
						var stream = file.GetStream ();
						return stream;
					});
					someView.Children.Add (imageView);
				} else {
					App.Current.MainPage.DisplayAlert ("No Camera or Image", "There is no camera or photo available", "Ok");
					return;
				}
			} else if (activeType == dict ["ListView"]) {
				someView.Children.Add (new ListView { ItemsSource = new [] { "Item1", "Item2", "Item3" } });
			}


			SetActiveView (someView);
			someView.Tapped += (nice, man) => {
				if (editMode && man.NumberOfTouches==1){
					SetActiveView (someView);
				}
			};

			if (clone) {
				View v = someView.Children [0];
				var properties = viewToAdd.GetType ().GetRuntimeProperties ();
				foreach (var prop in properties)
				{
					if (prop.CanRead && prop.CanWrite) {
						try{
							var value = prop.GetValue (viewToAdd);
							prop.SetValue (v, value);
						}catch(Exception e){
							Debug.WriteLine ("lolz you can't set the following property:" + prop);
						}
					}
				}
			}

			AbsoluteLayout.SetLayoutFlags (someView,
				AbsoluteLayoutFlags.None);

			AbsoluteLayout.SetLayoutBounds (someView,
				new Rectangle (x,
					y, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));

			someView.Panning += (control, args) => {

				if (editMode && (currAction == ACTION.MOVE || currAction == ACTION.FREEFORM)){
					SetActiveView(someView);

					var x0 = someView.X + args.TotalDistance.X;
					var y0 = someView.Y+ args.TotalDistance.Y;
					AbsoluteLayout.SetLayoutBounds (someView,
						new Rectangle (x0,
							y0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
				}
			};

			layout.Children.Add (someView);
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

		public void HandleControlManipulations(MR.Gestures.TapEventArgs e){
			if (currAction == ACTION.DELETE) {
				DeleteControl (e);
			}
		}

		public void HandleTapOnLayout(object sender, MR.Gestures.TapEventArgs e){
			if (editMode) {
				HandleControlManipulations (e);
			}

		}

	}
}


