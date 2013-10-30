namespace Solutionizer.Framework.TryOut {
    public class SubViewModel : PropertyChangedBase {
        private string _someText;

        public string SomeText {
            get { return _someText; }
            set {
                if (_someText != value) {
                    _someText = value;
                    NotifyOfPropertyChange(() => SomeText);
                }
            }
        }
    }
}