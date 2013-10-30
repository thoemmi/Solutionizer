namespace Solutionizer.Framework.TryOut {
    public class ShellViewModel : PropertyChangedBase {
        private string _title;
        private readonly SubViewModel _subViewModel;

        public ShellViewModel(SubViewModel subViewModel) {
            _subViewModel = subViewModel;

            Title = "Hello world";
            _subViewModel.SomeText = "Some text";
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
    }
}