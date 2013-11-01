using System.Windows.Input;

namespace Solutionizer.Framework.TryOut {
    public class MyDialogViewModel : DialogViewModel {
        private readonly ICommand _closeCommand;
        private string _dialogText;

        public MyDialogViewModel() {
            _closeCommand = new RelayCommand(Close);
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