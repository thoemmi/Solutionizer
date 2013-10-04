using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Solutionizer.Infrastructure;

namespace Solutionizer.ViewModels {
    public class UpdateViewModel : Screen {
        private readonly UpdateManager _updateManager;
        private readonly IDialogManager _dialogManager;
        private readonly ObservableCollection<ReleaseInfo> _releases;

        public UpdateViewModel(UpdateManager updateManager, IDialogManager dialogManager) {
            _updateManager = updateManager;
            _dialogManager = dialogManager;
            DisplayName = "Updates";
            _releases = new ObservableCollection<ReleaseInfo>(
                updateManager.Releases
                    .OrderByDescending(r => r.Version)
                    .SkipWhile(r => String.IsNullOrWhiteSpace(r.DownloadUrl)));
        }

        public ObservableCollection<ReleaseInfo> Releases {
            get { return _releases; }
        }

        public bool CanUpdate {
            get { return _releases.Any(); }
        }

        public void Update() {
            TryClose(true);
            _dialogManager.ShowDialog(new UpdateDownloadViewModel(_updateManager, _releases.First()));
        }

        public void Cancel() {
            TryClose(false);
        }
    }
}