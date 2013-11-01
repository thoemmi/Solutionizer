using System.Windows.Input;

namespace Solutionizer.Framework.TryOut {
    public class MyFlyoutViewModel : PropertyChangedBase {
        private string _text;

        public string Text {
            get { return _text; }
            set {
                if (_text != value) {
                    _text = value;
                    NotifyOfPropertyChange(() => Text);
                }
            }
        }
    }
}