using Caliburn.Micro;

namespace Solutionizer.Settings {
    public sealed class SettingsViewModel : Screen {
        public SettingsViewModel() {
            DisplayName = "Settings";
        }

        public void Ok() {
            TryClose();
        }

        public void Cancel() {
            TryClose();
        }
    }
}