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

            ShowUpdatesCommand = new RelayCommand<bool>(checkForUpdates => _flyoutManager.ShowFlyout(_viewModelFactory.CreateUpdateViewModel(checkForUpdates)));
            ShowSettingsCommand = new RelayCommand(OnShowSettings);
            ShowAboutCommand = new RelayCommand(() => _flyoutManager.ShowFlyout(_viewModelFactory.CreateAboutViewModel()));
            SelectRootPathCommand = new AsyncRelayCommand(SelectRootPath);
            SetRootPathCommand = new AsyncRelayCommand<string>(LoadProjectsAsync, path => !String.Equals(path, RootPath));

            _updateTimer = new Timer(_ => _updateManager.CheckForUpdatesAsync(), null, -1, -1);
        }

        public string RootPath {
            get => _rootPath;
            set {
                if (_rootPath != value) {
                    _rootPath = value;
                    NotifyOfPropertyChange(() => RootPath);
                    Title = String.IsNullOrEmpty(_rootPath) ? "Solutionizer" : "Solutionizer - " + _rootPath;
                }
            }
        }

        public string Title {
            get => _title;
            set {
                if (value == _title) return;
                _title = value;
                NotifyOfPropertyChange(() => Title);
            }
        }

        public ObservableCollection<string> MostRecentUsedFolders => _mostRecentUsedFoldersRepository.Folders;

        public ProjectRepositoryViewModel ProjectRepository => _projectRepository;

        public SolutionViewModel Solution {
            get => _solution;
            set {
                if (_solution != value) {
                    _solution = value;
                    NotifyOfPropertyChange(() => Solution);
                }
            }
        }

        public ISettings Settings => _settings;

        public ICommand ShowUpdatesCommand { get; }

        public ICommand ShowSettingsCommand { get; }

        public ICommand ShowAboutCommand { get; }

        public ICommand SelectRootPathCommand { get; }

        public ICommand SetRootPathCommand { get; }

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

        public IFlyoutManager Flyouts => _flyoutManager;

        public IDialogManager Dialogs => _dialogManager;

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
                Show($"{result.Projects.Count} projects loaded.");
            } else {
                RootPath = oldRootPath;
            }
        }

        public void Show(string status) {
            StatusMessage = status;
        }

        public string StatusMessage {
            get => _statusMessage;
            set {
                if (_statusMessage != value) {
                    _statusMessage = value;
                    NotifyOfPropertyChange(() => StatusMessage);
                }
            }
        }
    }
}