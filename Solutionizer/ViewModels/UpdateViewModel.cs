using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Caliburn.Micro;
using Solutionizer.Infrastructure;
using Solutionizer.Services;

namespace Solutionizer.ViewModels {
    public class UpdateViewModel : Screen {
        private readonly UpdateManager _updateManager;
        private readonly IDialogManager _dialogManager;
        private readonly ISettings _settings;
        private readonly bool _checkForUpdates;
        private readonly ObservableCollection<ReleaseInfo> _releases = new ObservableCollection<ReleaseInfo>();
        private bool _isUpdating = true;
        private bool _isUpToDate = false;
        private bool _canUpdate = false;
        private bool _showOldReleases;

        public UpdateViewModel(UpdateManager updateManager, IDialogManager dialogManager, ISettings settings, bool checkForUpdates) {
            _updateManager = updateManager;
            _dialogManager = dialogManager;
            _settings = settings;
            _checkForUpdates = checkForUpdates;
            DisplayName = checkForUpdates ? "Check for Updates" : "Available Updates";
            _releases = new ObservableCollection<ReleaseInfo>();

            var collectionView = CollectionViewSource.GetDefaultView(Releases);
            collectionView.Filter = item => ((ReleaseInfo)item).IsNew;

            BindingOperations.EnableCollectionSynchronization(_releases, _releases);
        }

        protected override void OnViewLoaded(object view) {
            base.OnViewLoaded(view);
            Task.Run(() => Populate());
        }

        private async void Populate() {
            if (_checkForUpdates) {
                await _updateManager.CheckForUpdatesAsync();
            }

            var releases = _updateManager.Releases
                .Where(r => _settings.IncludePrereleaseUpdates || !r.IsPrerelease)
                .OrderByDescending(r => r.Version)
                .SkipWhile(r => String.IsNullOrWhiteSpace(r.DownloadUrl)).ToList();

            releases.ForEach(_releases.Add);

            IsUpdating = false;
            IsUpToDate = _releases.All(r => !r.IsNew);
            CanUpdate = _releases.Any(r => r.IsNew);
        }

        public ObservableCollection<ReleaseInfo> Releases {
            get { return _releases; }
        }

        public bool CanUpdate {
            get { return _canUpdate; }
            set {
                if (value.Equals(_canUpdate)) {
                    return;
                }
                _canUpdate = value;
                NotifyOfPropertyChange(() => CanUpdate);
            }
        }

        public bool IsUpdating {
            get { return _isUpdating; }
            set {
                if (_isUpdating != value) {
                    _isUpdating = value;
                    NotifyOfPropertyChange(() => IsUpdating);
                }
            }
        }

        public bool IsUpToDate {
            get { return _isUpToDate; }
            set {
                if (_isUpToDate != value) {
                    _isUpToDate = value;
                    NotifyOfPropertyChange(() => IsUpToDate);
                }
            }
        }

        public bool ShowOldReleases {
            get { return _showOldReleases; }
            set {
                if (_showOldReleases != value) {
                    _showOldReleases = value;
                    NotifyOfPropertyChange(() => ShowOldReleases);

                    var collectionView = CollectionViewSource.GetDefaultView(Releases);
                    if (_showOldReleases) {
                        collectionView.Filter = null;
                    } else {
                        collectionView.Filter = item => ((ReleaseInfo) item).IsNew;
                    }
                }
            }
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