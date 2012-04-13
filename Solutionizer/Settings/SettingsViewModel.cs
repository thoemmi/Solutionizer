using Caliburn.Micro;

namespace Solutionizer.Settings {
    public class SettingsViewModel : PropertyChangedBase, IHaveDisplayName {
        public string DisplayName {
            get { return "Settings"; }
            set { ; }
        }
    }
}