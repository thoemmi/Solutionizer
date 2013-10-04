using Caliburn.Micro;

namespace Solutionizer.ViewModels {
    public class AboutViewModel : Screen {
        public AboutViewModel() {
            DisplayName = "About Solutionizer";
        }

        public void Ok() {
            TryClose();
        }
    }
}