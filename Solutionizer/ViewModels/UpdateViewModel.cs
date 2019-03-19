using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Solutionizer.Infrastructure;
using Solutionizer.Services;
using TinyLittleMvvm;

namespace Solutionizer.ViewModels {
    public class UpdateViewModel : DialogViewModel, IOnLoadedHandler {
        public delegate UpdateViewModel Factory(bool checkForUpdates);

        private readonly IUpdateManager _updateManager;
        private readonly ISettings _settings;
        private readonly bool _checkForUpdates;
        private bool _isUpdating = true;
        private bool _isUpToDate = false;
        private bool _canUpdate = false;
        private bool _showOldReleases;

        public UpdateViewModel(IUpdateManager updateManager, ISettings settings, IDialogManager dialogManager, UpdateDownloadViewModel.Factory updateDownloadViewModelFactory, bool checkForUpdates) {
            _updateManager = updateManager;
            _settings = settings;
            _checkForUpdates = checkForUpdates;
            Releases = new ObservableCollection<ReleaseInfo>();

            UpdateCommand = new AsyncRelayCommand(async () => {
                Close();
                await dialogManager.ShowDialogAsync(updateDownloadViewModelFactory.Invoke(Releases.First()));
            }, () => CanUpdate);
            CancelCommand = new RelayCommand(Close);

            var collectionView = CollectionViewSource.GetDefaultView(Releases);
            collectionView.Filter = item => ((ReleaseInfo)item).IsNew;

            BindingOperations.EnableCollectionSynchronization(Releases, Releases);
        }

        public async Task OnLoadedAsync() {
            if (_checkForUpdates) {
                await _updateManager.CheckForUpdatesAsync();
            }

            var releases = _updateManager.Releases
                .Where(r => _settings.IncludePrereleaseUpdates || !r.IsPrerelease)
                .OrderByDescending(r => r.Version)
                .SkipWhile(r => String.IsNullOrWhiteSpace(r.DownloadUrl)).ToList();

            releases.ForEach(Releases.Add);

            IsUpdating = false;
            IsUpToDate = Releases.All(r => !r.IsNew);
            CanUpdate = Releases.Any(r => r.IsNew);
        }


        public string Title => _checkForUpdates ? "Check for Updates" : "Available Updates";

        public ObservableCollection<ReleaseInfo> Releases { get; }

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

        public ICommand UpdateCommand { get; }

        public ICommand CancelCommand { get; }
    }
}