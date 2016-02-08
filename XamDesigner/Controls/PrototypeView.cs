using System;

using Xamarin.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using Plugin.Media;
using Plugin.Media.Abstractions;
using MODE = XamDesigner.PrototypePageViewModel.MODE;
using ACTION = XamDesigner.PrototypePageViewModel.ACTION;
using XamDesigner.Models;
using Plugin.Settings;
using Newtonsoft.Json;

namespace XamDesigner
{
	public class PrototypeView : MR.Gestures.ContentView
	{
		public PrototypePageViewModel ViewModel;
		public string navControlId;
		MR.Gestures.AbsoluteLayout layout = null;
		Dictionary<string,string> dict;
		public List <string> options;
		public bool[] toggleable;
		public string pageId;

		public PrototypeView ()
		{
		}

		protected override void OnBindingContextChanged ()
		{
			base.OnBindingContextChanged ();
			if (BindingContext != null) {
				ViewModel = (PrototypePageViewModel)BindingContext;
				Setup ();
			}
		}

		protected override void OnPropertyChanged (string propertyName)
		{
			base.OnPropertyChanged (propertyName);
			if (propertyName == HeightProperty.PropertyName) {
				LoadViewFromStorage ();
			}
		}

		void Setup(){
			dict = App.SupportedTypes;
			layout = new MR.Gestures.AbsoluteLayout  {};
			layout.Tapped += HandleTapOnLayout;
			layout.LongPressing += Layout_LongPressed;


			layout.Pinching += (sweet, cool) => {

				if (ViewModel.CurrentMode == MODE.EDIT && ViewModel.ActiveView!=null && (ViewModel.CurrentAction == ACTION.RESIZE || ViewModel.CurrentAction == ACTION.FREEFORM)){
					Debug.WriteLine("hey hey hey" + ViewModel.ActiveView.Bounds.Center);
					var actualView = ((StackLayout)ViewModel.ActiveView).Children.FirstOrDefault();
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

		void Layout_DoubleTapped (object sender, MR.Gestures.TapEventArgs e)
		{
			
			SetActiveView ((StackLayout)sender);
			if (ViewModel.CurrentMode == MODE.EDIT && ViewModel.CurrentAction == ACTION.FREEFORM) {
				ViewModel.Navigation.PushModalAsync(new EditPropertiesPage(((StackLayout)ViewModel.ActiveView).Children[0]));
			}
		}

		void Layout_LongPressed (object sender, MR.Gestures.LongPressEventArgs e)
		{
			if (ViewModel.CurrentMode == MODE.EDIT && ViewModel.CurrentAction == ACTION.FREEFORM) {

				var x = e.Center.X;
				var y = e.Center.Y;
				var children = layout.Children;
				foreach(var item in children){
					if (item.GetType() == typeof(MR.Gestures.StackLayout) && item.Id == ViewModel.ActiveView.Id){
						if (item.Bounds.Contains(x,y)){
							layout.LowerChild (item);
							Debug.WriteLine ("Child lowered " + item.Id);
							break;
						}
					}
				}


			}
		}

		public void SetMode(MODE mode){
			ViewModel.CurrentMode = mode;


			if (ViewModel.CurrentMode == MODE.PREVIEW){
				layout.Children.DoForEach (delegate (object layout) {
					var container = (layout as StackLayout).Children;
					if (container.Count < 1){
						return;
					}
					var element =  container[0];
					if (element.GetType() == typeof(Entry) || element.GetType() == typeof(Button)){
						element.IsEnabled = true;
					}
				}, typeof(MR.Gestures.StackLayout));
				ToggleEffect ();
			}else{
				layout.Children.DoForEach (delegate (object layout) {
					var container = (layout as StackLayout).Children;
					if (container.Count < 1){
						return;
					}
					var element =  container[0];					
					if (element.GetType() == typeof(Entry) || element.GetType() == typeof(Button)){
						element.IsEnabled = false;
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

			if (childToDelete != ViewModel.ActiveView) {
				SetActiveView ((StackLayout)childToDelete);
				return;
			}

			if (childToDelete != null) {
				children.Remove (childToDelete);
				ViewModel.ActiveView = null;
			}
		}

		public async void AddControl(double x, double y, View viewToAdd = null){

			bool clone = false;
			if (viewToAdd != null) {
				ViewModel.ActiveType = viewToAdd.GetType ().AssemblyQualifiedName;
				clone = true;
			}

			MR.Gestures.StackLayout someView = new MR.Gestures.StackLayout() {
				BackgroundColor=Color.Transparent};


			Debug.WriteLine ("Wtf is happeneing" + ViewModel.ActiveType);
			//TODO: Find a better way than this ugly hack.
			//Need to handle button case seperately for now
			if (ViewModel.ActiveType  == dict ["Button"]) {
				var button = new Button (){ Text = "Button" };

				button.IsEnabled = false;
				button.Clicked += (haha, snap) => {
					if (ViewModel.CurrentMode == MODE.EDIT && 
						ViewModel.CurrentAction != ACTION.MOVE && ViewModel.CurrentAction != ACTION.RESIZE) {
						if (someView.Id == ViewModel.ActiveView.Id && ViewModel.CurrentAction == ACTION.DELETE) {
							DeleteControl (childToDelete: (Button)haha);
						}
					}
					SetActiveView (someView);

				};

				someView.Children.Add (button);


			} else if (ViewModel.ActiveType == dict ["Label"]) {
				someView.Children.Add (new Label () {
					Text = "Label",
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Fill,
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center
				});
			} else if (ViewModel.ActiveType  == dict ["Entry"]) {
				someView.Children.Add (new Entry (){ Text = "Entry", IsEnabled = false });
			} else if (ViewModel.ActiveType  == dict ["BoxView"]) {
				someView.Children.Add (new BoxView (){ BackgroundColor = Color.Blue });
			} else if (ViewModel.ActiveType  == dict ["Switch"]) {
				someView.Children.Add (new Switch ());
			} else if (ViewModel.ActiveType  == dict ["WebView"]) {
				someView.Children.Add (new WebView () { Source = "https://www.google.com", HeightRequest = 100, WidthRequest = 100 });
			} else if (ViewModel.ActiveType  == dict ["Image"]) {

				var cameraAvailable = CrossMedia.Current.IsCameraAvailable;
				var pickerAvailable = CrossMedia.Current.IsPickPhotoSupported;
				if (clone && ((Image)viewToAdd).Source != null) {
					someView.Children.Add (new Image ());
				} else if (CrossMedia.Current.IsCameraAvailable || CrossMedia.Current.IsPickPhotoSupported) {

					var cameraOptions = new List<string> ();
					if (pickerAvailable) {
						cameraOptions.Add ("Gallery");
					}
					if (cameraAvailable) {
						cameraOptions.Add ("Camera");
					}

					var action = await App.Current.MainPage.DisplayActionSheet ("Please select image source.", "Cancel", null, cameraOptions.ToArray());


					var imageView = new Image ();
					MediaFile file = null; 

					if (action == "Gallery") {
						file = await CrossMedia.Current.PickPhotoAsync ();
					} else if (action == "Camera") {
						file = await CrossMedia.Current.TakePhotoAsync (new StoreCameraMediaOptions () {
							Directory = "XamDesigner",
							Name = imageView.Id.ToString () + "quickie.jpg"
						});
					} else {
						someView.Children.Add (imageView);
						return;
					}

					imageView.Source = ImageSource.FromStream (() => {
						var stream = file.GetStream ();
						return stream;
					});
					someView.Children.Add (imageView);
				} else {
					await App.Current.MainPage.DisplayAlert ("No Camera or Image", "There is no camera or photo available", "Ok");
					return;
				}
			} else if (ViewModel.ActiveType  == dict ["ListView"]) {
				someView.Children.Add (new ListView { ItemsSource = new [] { "Item1", "Item2", "Item3" } });
			} else if (ViewModel.ActiveType  == dict ["Navigation"]) {
				await ViewModel.AddNavigation ();
				return;
			}


			SetActiveView (someView);
			someView.Tapped += (nice, man) => {
				if (ViewModel.CurrentMode == MODE.EDIT && man.NumberOfTouches==1
					&& ViewModel.CurrentAction != ACTION.MOVE && ViewModel.CurrentAction != ACTION.RESIZE){
				}
				SetActiveView (someView);
			};

			someView.DoubleTapped += Layout_DoubleTapped;


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

			someView.Panning +=  (control, args) => {

				if (ViewModel.CurrentMode == MODE.EDIT && (ViewModel.CurrentAction == ACTION.MOVE || ViewModel.CurrentAction == ACTION.FREEFORM)){
					SetActiveView(someView);

					var x0 = someView.X + args.TotalDistance.X;
					var y0 = someView.Y+ args.TotalDistance.Y;
					AbsoluteLayout.SetLayoutBounds (someView,
						new Rectangle (x0,
							y0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
				}

				if (ViewModel.TopPage.MenuGrid.IsMenuVisible){
					 ViewModel.TopPage.MenuGrid.ShowHideMenu();
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
			if (someView != null && (ViewModel.ActiveView == null || ViewModel.ActiveView.Id != someView.Id)){
				ViewModel.ActiveView = someView;
			}
			if (ViewModel.CurrentMode == MODE.EDIT) {
				ToggleEffect (someView);
			}
		}

		public async void HandleControlManipulations(MR.Gestures.TapEventArgs e){
			if (ViewModel.CurrentAction == ACTION.DELETE) {
				DeleteControl (e);
			} else if (ViewModel.TopPage.MenuGrid.IsMenuVisible) {
				await ViewModel.TopPage.MenuGrid.ShowHideMenu ();
			}
		}

		public void HandleTapOnLayout(object sender, MR.Gestures.TapEventArgs e){
			if (ViewModel.CurrentMode == MODE.EDIT) {
				HandleControlManipulations (e);
			}

		}


		public void LoadViewFromStorage(){
			pageId = "page1";
			if (pageId == null) {
				return;
			}
			var lol = CrossSettings.Current.GetValueOrDefault<String> (pageId, "");
			if (!string.IsNullOrEmpty (lol)) {
				var controls = JsonConvert.DeserializeObject<List<ControlModel>> (lol);	
				foreach (var controlModel in controls) {
					ViewModel.ActiveType = controlModel.ControlType.AssemblyQualifiedName;
					AddControl (controlModel.x * Width, controlModel.y * Height);
					var actualView = ViewModel.ActiveView.Children [0];
					var properties = GetViewProperties (actualView);
					foreach (var propKVP in controlModel.Properties){
						var propInfo = properties.Find ((delegate(PropertyInfo obj) {
							return obj.Name == propKVP.Key;
						}));


						try{
								propInfo.SetValue (actualView, propKVP.Value);
						}
						catch(Exception hahahahahah){
							Debug.WriteLine ("Bro come on " + hahahahahah.ToString ());
						}
					}

					foreach (var colorKVP in controlModel.ColorProperies){
						var propInfo = properties.Find ((delegate(PropertyInfo obj) {
							return obj.Name == colorKVP.Key;
						}));


						try{
							var colorTuple = colorKVP.Value as ControlModel.ColorTuple;
							propInfo.SetValue (actualView, Color.FromRgb(colorTuple.r, colorTuple.g, colorTuple.b));
						}
						catch(Exception hahahahahah){
							Debug.WriteLine ("Bro come on " + hahahahahah.ToString ());
						}
					}


					actualView.HeightRequest = ViewModel.ActiveView.HeightRequest = controlModel.Height * Height;
					actualView.WidthRequest = ViewModel.ActiveView.WidthRequest = controlModel.Width * Width;



				}
			}
		}


		public void SaveViewToStorage(string pageId){
			var controls = new List<ControlModel> ();	
			foreach (var child in layout.Children) {
				var actualChild = (child as StackLayout).Children [0];
				var model = new ControlModel ();
				model.ControlType = actualChild.GetType ();
				model.x = child.X / Width;
				model.y = child.Y / Height;
				model.Width = child.Width / Width;
				model.Height = child.Height / Height;
				var properties = GetViewProperties (actualChild);

				foreach (var property in properties) {
					if (property.PropertyType == typeof(Color)) {
						var color = (Color)property.GetValue (actualChild);
						//Necessary because these are -1 for some stupid reason by default.
						if (color.R < 0 || color.B < 0 || color.G < 0) {
							continue;
						}
						model.ColorProperies.Add(property.Name, new ControlModel.ColorTuple {r = color.R, g = color.G, b = color.B});
					} else {
						model.Properties.Add (property.Name, property.GetValue (actualChild));
					}
				}

				controls.Add (model);
			}
			CrossSettings.Current.AddOrUpdateValue<String> (pageId, JsonConvert.SerializeObject(controls));
		}

		public List<PropertyInfo>  GetViewProperties(View v){
			List<PropertyInfo> result = new List<PropertyInfo> ();
			var properties = v.GetType ().GetRuntimeProperties ();
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
				typeof(Color)
			};

			var restrictedProps = new String[] {
				View.HeightProperty.PropertyName,
				View.WidthProperty.PropertyName
			};

			foreach (var property in properties) {
				if (property.CanWrite && property.CanRead && property.GetValue (v) != null &&
				    supportedProps.Contains (property.PropertyType) && !restrictedProps.Contains (property.Name)) {
					result.Add (property);
				}
			}
			return result;
		}

	}
}


