using System.Collections.ObjectModel;
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
    }
}