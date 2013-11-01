using System.Windows.Input;

namespace Solutionizer.Framework.TryOut {
    public class ShellViewModel : PropertyChangedBase {
        private string _title;
        private readonly SubViewModel _subViewModel;
        private readonly IFlyoutManager _flyoutManager;
        private readonly IDialogManager _dialogManager;
        private readonly ICommand _showFlyoutCommand;
        private readonly ICommand _showDialogCommand;

        public ShellViewModel(SubViewModel subViewModel, IFlyoutManager flyoutManager, IDialogManager dialogManager) {
            _subViewModel = subViewModel;
            _flyoutManager = flyoutManager;
            _dialogManager = dialogManager;

            Title = "Hello world";
            _subViewModel.SomeText = "Some text";

            _showFlyoutCommand = new RelayCommand(OnShowFlyout);
            _showDialogCommand = new RelayCommand(OnShowDialog);
        }

        private void OnShowFlyout() {
            _flyoutManager.ShowFlyout(new MyFlyoutViewModel { Text = "This is my flyout" });
        }

        private async void OnShowDialog() {
            _subViewModel.SomeText = "Showing dialog";
            await _dialogManager.ShowDialog(new MyDialogViewModel { DialogText = "This is my dialog with just some text." });
            _subViewModel.SomeText = "Dialog was closed";
        }

        public string Title {
            get { return _title; }
            set {
                if (_title != value) {
                    _title = value;
                    NotifyOfPropertyChange(() => Title);
                }
            }
        }

        public SubViewModel SubViewModel {
            get { return _subViewModel; }
        }

        public IFlyoutManager Flyouts {
            get { return _flyoutManager; }
        }

        public IDialogManager Dialogs {
            get { return _dialogManager; }
        }

        public ICommand ShowFlyoutCommand {
            get { return _showFlyoutCommand; }
        }

        public ICommand ShowDialogCommand {
            get { return _showDialogCommand; }
        }
    }
}