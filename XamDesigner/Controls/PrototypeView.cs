using System;

using Xamarin.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using System.Linq;

namespace XamDesigner
{
	public class PrototypeView : MR.Gestures.ContentView
	{
		[FlagsAttribute] 
		enum ACTION {
			ADD = 0, MOVE=1, RESIZE=2, FREEFORM=4 ,COLOR=8, DELETE=16, PROPERTIES = 32
		}

		MR.Gestures.AbsoluteLayout layout = null;
		string activeType;
		Dictionary<string,string> dict;
		bool editMode = true;
		StackLayout activeView;
		ACTION currAction = ACTION.ADD;
		public List <string> options;
		XamDesigner.EnumerableExtensions.GenericDelegate<object>[] optionDelegates;

		public PrototypeView ()
		{
			Setup ();
		}

		public void ExecuteAction(int action){
			optionDelegates[action] (null);
		}

		void Setup(){
			dict = App.SupportedTypes;
			options = new List<string> { "Add", "Move", "Resize", "Free Form", "Colors", "Delete", "Properties" };
			optionDelegates = new XamDesigner.EnumerableExtensions.GenericDelegate<object>[] {
				async delegate {
					currAction = ACTION.ADD;
					var action = await App.Current.MainPage.DisplayActionSheet ("Pick a control and tap on the screen.", "Cancel", null, dict.Keys.ToArray<string>());
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
					var action = await App.Current.MainPage.DisplayActionSheet ("Pick a control and tap on the screen.", "Cancel", null, dict.Keys.ToArray<string>());
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
					currAction = ACTION.PROPERTIES;
				}
			};

			activeType = typeof(Button).AssemblyQualifiedName;
			layout = new MR.Gestures.AbsoluteLayout  {};
			layout.Tapped += HandleTapOnLayout;
			layout.LongPressing += Layout_LongPressed;

			double diffY = 0;
			layout.Pinching += (sweet, cool) => {

				if (editMode && activeView!=null && (currAction == ACTION.RESIZE || currAction == ACTION.FREEFORM)){
					Debug.WriteLine("hey hey hey" + activeView.Bounds.Center);
					var actualView = activeView.Children.FirstOrDefault();
					var finger1 = cool.Touches[0];
					var finger2 = cool.Touches[1];
					if (Math.Abs(finger1.X - finger2.X) > Math.Abs(finger1.Y - finger2.Y)){
						actualView.WidthRequest = actualView.Width * cool.DeltaScale;
					}else{
						var newDiff = Math.Abs(finger1.Y - finger2.Y);

						if (newDiff > diffY){
							actualView.HeightRequest = actualView.Height * 1.0 + (diffY/newDiff);
						}else{
							actualView.HeightRequest = actualView.Height * 1.0 - (diffY/newDiff);
						}
						diffY = newDiff;

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

		public void PropColorControl(MR.Gestures.TapEventArgs e = null, View childToColor = null){

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
				if (currAction == ACTION.COLOR) {
					(App.Current.MainPage as MasterDetailPage).Detail.Navigation.PushModalAsync (new ColorPage (childToColor));
				} else if (currAction == ACTION.PROPERTIES) {
					(App.Current.MainPage as MasterDetailPage).Detail.Navigation.PushModalAsync(new EditPropertiesPage(activeView.Children[0]));
				}
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

			MR.Gestures.StackLayout someView = new MR.Gestures.StackLayout() {
				BackgroundColor=Color.Transparent};

			//TODO: Find a better way than this ugly hack.
			//Need to handle button case seperately for now
			if (activeType == dict["Button"]){
				var button = new Button (){ Text = "Button" };
				button.Clicked += (haha, snap) => {
					if (editMode){


						if (someView.Id == activeView.Id && currAction == ACTION.DELETE) {
							DeleteControl (childToDelete: activeView);
						}else if(someView.Id == activeView.Id && (currAction == ACTION.COLOR || currAction == ACTION.PROPERTIES)) {
							PropColorControl(childToColor: button);
						}else {
							SetActiveView(someView);
						}

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

		public void HandleControlManipulations(MR.Gestures.TapEventArgs e){
			if (currAction == ACTION.ADD || currAction == ACTION.FREEFORM) {
				AddControl (e);
			} else if (currAction == ACTION.DELETE) {
				DeleteControl (e);
			} else if (currAction == ACTION.COLOR || currAction == ACTION.PROPERTIES) {
				PropColorControl (e);
			}
		}

		public void HandleTapOnLayout(object sender, MR.Gestures.TapEventArgs e){
			if (editMode) {
				HandleControlManipulations (e);
			}

		}

	}
}


