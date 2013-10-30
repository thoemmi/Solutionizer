using System.Windows.Input;

namespace Solutionizer.Framework.TryOut {
    public class ShellViewModel : PropertyChangedBase {
        private string _title;
        private readonly SubViewModel _subViewModel;
        private readonly IFlyoutManager _flyoutManager;
        private readonly ICommand _showFlyoutCommand;

        public ShellViewModel(SubViewModel subViewModel, IFlyoutManager flyoutManager) {
            _subViewModel = subViewModel;
            _flyoutManager = flyoutManager;

            Title = "Hello world";
            _subViewModel.SomeText = "Some text";

            _showFlyoutCommand = new RelayCommand(OnShowFlyout);
        }

        private void OnShowFlyout() {
            _flyoutManager.ShowFlyout(new MyFlyoutViewModel { Text = "This is my flyout" });
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

        public ICommand ShowFlyoutCommand {
            get { return _showFlyoutCommand; }
        }
    }
}