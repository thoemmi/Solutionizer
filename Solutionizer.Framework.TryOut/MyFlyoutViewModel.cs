using System.Windows.Input;

namespace Solutionizer.Framework.TryOut {
    public class MyFlyoutViewModel : DialogViewModel {
        private readonly ICommand _closeCommand;
        private string _text;

        public MyFlyoutViewModel() {
            _closeCommand = new RelayCommand(Close);
        }

        public string Text {
            get { return _text; }
            set {
                if (_text != value) {
                    _text = value;
                    NotifyOfPropertyChange(() => Text);
                }
            }
        }

        public ICommand CloseCommand {
            get { return _closeCommand; }
        }
    }
}