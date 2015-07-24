using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using Solutionizer.Infrastructure;
using Solutionizer.Services;
using TinyLittleMvvm;

namespace Solutionizer.ViewModels {
    public sealed class ShellViewModel : PropertyChangedBase, IShell, IOnLoadedHandler, IStatusMessenger {
        private readonly ISettings _settings;
        private readonly IDialogManager _dialogManager;
        private readonly IFlyoutManager _flyoutManager;
        private readonly IUpdateManager _updateManager;
        private readonly IViewModelFactory _viewModelFactory;
        private readonly IMostRecentUsedFoldersRepository _mostRecentUsedFoldersRepository;
        private readonly ProjectRepositoryViewModel _projectRepository;
        private SolutionViewModel _solution;
        private string _rootPath;
        private bool _areUpdatesAvailable;
        private string _title = "Solutionizer";
        private readonly ICommand _showUpdatesCommand;
        private readonly ICommand _showSettingsCommand;
        private readonly ICommand _showAboutCommand;
        private readonly ICommand _selectRootPathCommand;
        private readonly ICommand _setRootPathCommand;
        private readonly Timer _updateTimer;
        private string _statusMessage;

        public ShellViewModel(ISettings settings, IDialogManager dialogManager, IFlyoutManager flyoutManager, IUpdateManager updateManager, IViewModelFactory viewModelFactory, IMostRecentUsedFoldersRepository mostRecentUsedFoldersRepository) {
            _settings = settings;
            _dialogManager = dialogManager;
            _flyoutManager = flyoutManager;
            _updateManager = updateManager;
            _viewModelFactory = viewModelFactory;
            _mostRecentUsedFoldersRepository = mostRecentUsedFoldersRepository;

            _projectRepository = _viewModelFactory.CreateProjectRepositoryViewModel(new RelayCommand<ProjectViewModel>(projectViewModel => _solution.AddProject(projectViewModel.Project)));
            _updateManager.UpdatesAvailable +=
                (sender, args) => AreUpdatesAvailable = _updateManager.Releases != null && _updateManager.Releases.Any(r => r.IsNew && (_settings.IncludePrereleaseUpdates || !r.IsPrerelease));

            _showUpdatesCommand = new RelayCommand<bool>(checkForUpdates => _flyoutManager.ShowFlyout(_viewModelFactory.CreateUpdateViewModel(checkForUpdates)));
            _showSettingsCommand = new RelayCommand(OnShowSettings);
            _showAboutCommand = new RelayCommand(() => _flyoutManager.ShowFlyout(_viewModelFactory.CreateAboutViewModel()));
            _selectRootPathCommand = new AsyncRelayCommand(SelectRootPath);
            _setRootPathCommand = new AsyncRelayCommand<string>(LoadProjectsAsync, path => !String.Equals(path, RootPath));

            _updateTimer = new Timer(_ => _updateManager.CheckForUpdatesAsync(), null, -1, -1);
        }

        public string RootPath {
            get { return _rootPath; }
            set {
                if (_rootPath != value) {
                    _rootPath = value;
                    NotifyOfPropertyChange(() => RootPath);
                    Title = String.IsNullOrEmpty(_rootPath) ? "Solutionizer" : "Solutionizer - " + _rootPath;
                }
            }
        }

        public string Title {
            get { return _title; }
            set {
                if (value == _title) return;
                _title = value;
                NotifyOfPropertyChange(() => Title);
            }
        }

        public ObservableCollection<string> MostRecentUsedFolders {
            get { return _mostRecentUsedFoldersRepository.Folders; }
        }

        public ProjectRepositoryViewModel ProjectRepository {
            get { return _projectRepository; }
        }

        public SolutionViewModel Solution {
            get { return _solution; }
            set {
                if (_solution != value) {
                    _solution = value;
                    NotifyOfPropertyChange(() => Solution);
                }
            }
        }

        public ISettings Settings {
            get { return _settings; }
        }

        public ICommand ShowUpdatesCommand {
            get { return _showUpdatesCommand; }
        }

        public ICommand ShowSettingsCommand {
            get { return _showSettingsCommand; }
        }

        public ICommand ShowAboutCommand {
            get { return _showAboutCommand; }
        }

        public ICommand SelectRootPathCommand {
            get { return _selectRootPathCommand; }
        }

        public ICommand SetRootPathCommand {
            get { return _setRootPathCommand; }
        }

        public async Task OnLoadedAsync() {
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && Directory.Exists(args[1])) {
                await LoadProjectsAsync(args[1]);
            } else if (_settings.ScanOnStartup) {
                await LoadProjectsAsync(_settings.RootPath);
            }

            if (_settings.AutoUpdateCheck) {
                _updateTimer.Change(0, 1000 * 60 * 60);
            }
        }

        private async void OnShowSettings() {
            await _flyoutManager.ShowFlyout(_viewModelFactory.CreateSettingsViewModel());
            if (_settings.AutoUpdateCheck) {
                _updateTimer.Change(0, 1000 * 60 * 60);
            } else {
                _updateTimer.Change(-1, -1);
            }
        }

        public async Task SelectRootPath() {
            var dlg = new VistaFolderBrowserDialog {
                SelectedPath = _settings.RootPath
            };
            if (dlg.ShowDialog(Application.Current.MainWindow) == true) {
                await LoadProjectsAsync(dlg.SelectedPath);
            }
        }

        public IFlyoutManager Flyouts {
            get { return _flyoutManager; }
        }

        public IDialogManager Dialogs {
            get { return _dialogManager; }
        }

        public bool AreUpdatesAvailable {
            get { return _areUpdatesAvailable; }
            set {
                if (_areUpdatesAvailable != value) {
                    _areUpdatesAvailable = value;
                    NotifyOfPropertyChange(() => AreUpdatesAvailable);
                }
            }
        }

        private async Task LoadProjectsAsync(string path) {
            var oldRootPath = RootPath;
            RootPath = path;

            var fileScanningViewModel = _viewModelFactory.CreateFileScanningViewModel(path);
            var result = await _dialogManager.ShowDialogAsync(fileScanningViewModel);

            if (result != null) {
                _settings.RootPath = path;
                _projectRepository.RootPath = path;
                _projectRepository.RootFolder = result.ProjectFolder;
                _mostRecentUsedFoldersRepository.SetCurrentFolder(path);
                Solution = _viewModelFactory.CreateSolutionViewModel(path, result.Projects);
                Show(String.Format("{0} projects loaded.", result.Projects.Count));
            } else {
                RootPath = oldRootPath;
            }
        }

        public void Show(string status) {
            StatusMessage = status;
        }

        public string StatusMessage {
            get { return _statusMessage; }
            set {
                if (_statusMessage != value) {
                    _statusMessage = value;
                    NotifyOfPropertyChange(() => StatusMessage);
                }
            }
        }
    }
}