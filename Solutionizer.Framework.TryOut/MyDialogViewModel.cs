using System.Windows.Input;

namespace Solutionizer.Framework.TryOut {
    public class MyDialogViewModel : DialogViewModel<bool> {
        private readonly ICommand _closeCommand;
        private string _title = "Hello worlddd";
        private string _dialogText;

        public MyDialogViewModel() {
            _closeCommand = new RelayCommand(() => Close(true));
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

        public string DialogText {
            get { return _dialogText; }
            set {
                if (_dialogText != value) {
                    _dialogText = value;
                    NotifyOfPropertyChange(() => DialogText);
                }
            }
        }

        public ICommand CloseCommand {
            get { return _closeCommand; }
        }
    }
}