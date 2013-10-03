using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Solutionizer.Infrastructure;

namespace Solutionizer.ViewModels {
    public class UpdateViewModel : Screen {
        private readonly ObservableCollection<ReleaseInfo> _releases;

        public UpdateViewModel(UpdateManager updateManager) {
            DisplayName = "Updates";
            _releases = new ObservableCollection<ReleaseInfo>(updateManager.Releases);
        }

        public ObservableCollection<ReleaseInfo> Releases {
            get { return _releases; }
        }

        public bool CanUpdate {
            get { return _releases.Any(); }
        }

        public void Update() {
            TryClose(true);
        }

        public void Cancel() {
            TryClose(false);
        }
    }
}