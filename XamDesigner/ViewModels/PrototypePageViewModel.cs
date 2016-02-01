using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XamDesigner
{
	public class PrototypePageViewModel : BaseViewModel
	{
		Dictionary<string,string> supportedTypes = App.SupportedTypes;
		Dictionary<Guid, ContainerPage> ContainerPages = new Dictionary<Guid, ContainerPage>();
		public PrototypePageViewModel ()
		{
			ActiveType = typeof(Button).AssemblyQualifiedName;

			MenuOptions = new List<MenuOptionModel>(){

				new MenuOptionModel(){ Title = "Add", Command = new Command<PrototypeView>(async (protoView) => {
					CurrentAction = ACTION.FREEFORM;
					var action = await DisplayOptionAlert ("Pick a control to add.", "Cancel", null, supportedTypes.Keys.ToArray<string>());
					if (action != null && supportedTypes.Keys.Contains(action)){
						ActiveType = supportedTypes[action];
						protoView.AddControl(protoView.Width/2, protoView.Height/2);
					}
					TopPage.MenuGrid.SetToggled(1,true);
					TopPage.MenuGrid.SetToggled(2,true);
				})},

				new MenuOptionModel(){ Title = "Move", IsToggleable=true, Command = new Command( () => {
					if (CurrentAction == ACTION.RESIZE){
						CurrentAction = ACTION.FREEFORM;
					}else{
						CurrentAction = ACTION.MOVE;
					}
					TopPage.MenuGrid.SetToggled(5, false);// Untoggled delete button
				}),
					UnToggleCommand = new Command(()=> {
						if (CurrentAction == ACTION.FREEFORM){
							CurrentAction = ACTION.RESIZE;
						}else{
							CurrentAction = ACTION.NONE;
						}
					})},

				new MenuOptionModel(){ Title = "Resize", IsToggleable=true, Command = new Command( () => {
					if (CurrentAction == ACTION.MOVE){
							CurrentAction = ACTION.FREEFORM;
					}else{
							CurrentAction = ACTION.RESIZE;
					}
					TopPage.MenuGrid.SetToggled(5, false);// Untoggled delete button

				}), UnToggleCommand = new Command(()=> {
					if (CurrentAction == ACTION.FREEFORM){
						CurrentAction = ACTION.MOVE;
					}else{
						CurrentAction = ACTION.NONE;
					}
				})
				},
				new MenuOptionModel(){ Title = "Clone", Command = new Command<PrototypeView>((protoView) => {
					CurrentAction = ACTION.FREEFORM;
					if (ActiveView == null){
						DisplayMessageAlert ("Woah There!", "You must select an element first.", "OK");
					}else{
						var stackLayout = ActiveView as StackLayout;
						protoView.AddControl(protoView.Width/2, protoView.Height/2, stackLayout.Children[0]);
					}
				})},

				new MenuOptionModel(){ Title = "Colors", Command = new Command(async () => {
					CurrentAction = ACTION.NONE;
					if (ActiveView == null){
						await DisplayMessageAlert ("Woah There!", "You must select an element first.", "OK");
					}else{
						await Navigation.PushModalAsync (new ColorPage (ActiveView.Children[0]));
					}
				})},

				new MenuOptionModel(){ Title = "Delete", IsToggleable=true, Command = new Command(() => {
					CurrentAction = ACTION.DELETE;
				}), UnToggleOthers = true, UnToggleCommand = new Command(()=>{})},

				new MenuOptionModel(){ Title = "More", Command = new Command(async () => {
					CurrentAction = ACTION.NONE;
					if (ActiveView == null){
						await DisplayMessageAlert ("Woah There!", "You must select an element first.", "OK");
					}else{
						var stackLayout = ActiveView as StackLayout;
						await Navigation.PushModalAsync(new EditPropertiesPage(stackLayout.Children[0]));
					}
				})}

			};

			ButtonCommand = new Command<Button>(view => {
				var parent = (StackLayout)view.Parent;
				var rootContainer = ((PrototypeView)parent.Parent);

				if (CurrentMode == MODE.EDIT) {
					//Parent because StackLayout is the container for the button.
					if (parent.Id == ActiveView.Id && CurrentMode == MODE.EDIT && CurrentAction == ACTION.DELETE) {
						rootContainer.DeleteControl (childToDelete: ActiveView);
					}
				}
				rootContainer.SetActiveView (parent);
			});
		}



		public void ExecuteMenuAction(int action){
			if (MenuOptions [action].IsToggleable && MenuOptions [action].IsToggled) {
				MenuOptions [action].UnToggleCommand.Execute (this);
			} else {
				MenuOptions [action].Command.Execute (TopPage.protoTypePage);
			}
			ToggleMode(true);
		}



		public void ToggleMode(bool forceEdit = false){

			var modeToSet = CurrentMode;
			if (!forceEdit) {
				modeToSet = modeToSet == MODE.EDIT ? MODE.PREVIEW : MODE.EDIT;
			} else {
				modeToSet = MODE.EDIT;
			}

			var pages = Navigation.NavigationStack;
			foreach(var page in pages){
				var containerpage = page as ContainerPage;	
				containerpage.protoTypePage.ViewModel.CurrentMode = modeToSet;
				containerpage.protoTypePage.SetMode (modeToSet);
			}

		}


		public async void HideMenu(){
			var menu = TopPage.MenuGrid;
			if (menu.IsMenuVisible){
				await menu.ShowHideMenu();
			}
		}

		ContainerPage PushBlankPage(Guid id, ContainerPage containerPage = null){
			bool alreadyExists = true;
			if (containerPage == null) {
				alreadyExists = false;
				containerPage = new ContainerPage ();
			}

			if (containerPage == TopPage) {
				return containerPage;
			} else if (alreadyExists) {
				Navigation.RemovePage(containerPage);
			}
				
			Navigation.PushAsync (containerPage);
			NavigationPage.SetHasNavigationBar (containerPage, false);
			ContainerPages[id] = containerPage;
			return containerPage;
		}


		public async Task AddNavigation(){
			if (ActiveView == null || ActiveView.Children [0].GetType() != typeof(Button)) {
				await DisplayMessageAlert ("No control picked", "You must select a button that will change pages", "Ok");
			} else if (ContainerPages.ContainsKey(ActiveView.Children [0].Id)){
				await DisplayMessageAlert ("Navigation Already Exists", "Select another button or delete this button's navigation.", "Ok");
			}else {
				string action;
				if (Navigation.NavigationStack.Count > 1) {
					action = await DisplayOptionAlert ("Navigation", "Cancel", null, "New Page", "Go Back");
				} else {
					action = await DisplayOptionAlert ("Navigation", "Cancel", null, "New Page");
				}

				if (action == "Go Back") {
					((Button)ActiveView.Children [0]).Clicked += (sender, e) => {
						if (CurrentMode == MODE.PREVIEW) {
							Navigation.PopAsync();
						}
					};
					ContainerPages [((Button)ActiveView.Children [0]).Id] = (ContainerPage)Navigation.NavigationStack.Last ();

				} else if (action == "New Page"){
					

					((Button)ActiveView.Children [0]).Clicked += OnButtonClick;

					var page = PushBlankPage(id:((Button)ActiveView.Children [0]).Id);
					AddNewMenuButtonForPage (page);
					ActiveView = null;
					ActiveType = null;
				}
			}
		}

		public void AddNewMenuButtonForPage(ContainerPage page){
			var menuPage = ((App)App.Current).MenuPage;
			var menuButton = new SlidingTrayButton ("Page " + (((StackLayout)menuPage.Content).Children.Count - 1));
			menuPage.AddButton(menuButton);
			menuButton.Clicked += OnButtonClick;
			ContainerPages[menuButton.Id] = page;
		}

		public void OnButtonClick(object sender, EventArgs args){

			if (sender.GetType () == typeof(SlidingTrayButton)) {
				(App.Current.MainPage as MasterDetailPage).IsPresented = false;
			}

			if (CurrentMode == MODE.PREVIEW || sender.GetType() == typeof(SlidingTrayButton)){
				if (ContainerPages.Keys.Contains(((Button)sender).Id)) {
					PushBlankPage(((Button)sender).Id, ContainerPages[((Button)sender).Id]);
				}else{
					PushBlankPage(id: ((Button)sender).Id);
				}
			}
		}

		public List<MenuOptionModel> MenuOptions;


		public enum ACTION {
			NONE, MOVE, RESIZE, FREEFORM, DELETE
		} 

		public enum MODE {
			PREVIEW, EDIT
		}

		public Command ButtonCommand;


		private StackLayout _activeView;
		public StackLayout ActiveView {
			get{ return _activeView; }
			set { _activeView = value; RaisePropertyChanged (); }
		}
		
		private string _activeType;
			public string ActiveType {
				get{ return _activeType; }
				set { _activeType = value; RaisePropertyChanged (); }
		}


		private ACTION _currAction;
		public ACTION CurrentAction {
			get{ return _currAction; }
			set { _currAction = value; RaisePropertyChanged (); }
		}

		private MODE _currMode;
		public MODE CurrentMode {
			get{ return _currMode; }
			set { _currMode = value; RaisePropertyChanged (); }
		}

	}
}

