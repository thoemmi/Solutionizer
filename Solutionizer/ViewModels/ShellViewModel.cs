using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Autofac;
using Ookii.Dialogs.Wpf;
using Solutionizer.Framework;
using Solutionizer.Infrastructure;
using Solutionizer.Services;

namespace Solutionizer.ViewModels {
    public sealed class ShellViewModel : PropertyChangedBase, IShell, IOnLoadedHandler {
        private readonly ISettings _settings;
        private readonly IDialogManager _dialogManager;
        private readonly IFlyoutManager _flyoutManager;
        private readonly UpdateManager _updateManager;
        private readonly ProjectRepositoryViewModel _projectRepository;
        private SolutionViewModel _solution;
        private string _rootPath;
        private bool _areUpdatesAvailable;
        private string _title = "Solutionizer";
        private readonly ICommand _showUpdatesCommand;
        private readonly ICommand _showSettingsCommand;
        private readonly ICommand _showAboutCommand;
        private readonly ICommand _selectRootPathCommand;

        public ShellViewModel(ISettings settings, IDialogManager dialogManager, IFlyoutManager flyoutManager) {
            _settings = settings;
            _projectRepository = new ProjectRepositoryViewModel(settings, new RelayCommand<ItemViewModel>(OnDoubleClick));
            _updateManager = new UpdateManager(_settings, AppEnvironment.CurrentVersion);
            _updateManager.UpdatesAvailable +=
                (sender, args) => AreUpdatesAvailable = _updateManager.Releases != null && _updateManager.Releases.Any(r => r.IsNew && (_settings.IncludePrereleaseUpdates || !r.IsPrerelease));
            _dialogManager = dialogManager;
            _flyoutManager = flyoutManager;

            _showUpdatesCommand = new RelayCommand<bool>(ShowUpdate);
            _showSettingsCommand = new RelayCommand(ShowSettings);
            _showAboutCommand = new RelayCommand(ShowAbout);
            _selectRootPathCommand = new RelayCommand(SelectRootPath);
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

        public void OnLoaded() {
            if (_settings.ScanOnStartup) {
                LoadProjects(_settings.RootPath);
            }

            Task.Run(() => _updateManager.CheckForUpdatesAsync());
        }

        public void SelectRootPath() {
            var dlg = new VistaFolderBrowserDialog {
                SelectedPath = _settings.RootPath
            };
            if (dlg.ShowDialog(Application.Current.MainWindow) == true) {
                _settings.RootPath = dlg.SelectedPath;
                LoadProjects(dlg.SelectedPath);
            }
        }

        public IFlyoutManager Flyouts {
            get { return _flyoutManager; }
        }

        public void ShowSettings() {
            _flyoutManager.ShowFlyout(new SettingsViewModel(_settings));
        }

        public async void ShowUpdate(bool checkForUpdates) {
            var result = await _flyoutManager.ShowFlyout(new UpdateViewModel(_updateManager, _settings, checkForUpdates));
            if (result != null) {
                await _dialogManager.ShowDialog(new UpdateDownloadViewModel(_updateManager, result));
            }
        }

        public void ShowAbout() {
            _flyoutManager.ShowFlyout(new AboutViewModel());
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

        private async void LoadProjects(string path) {
            var oldRootPath = RootPath;
            RootPath = path;

            var fileScanningViewModel = new FileScanningViewModel(_settings, path);
            var result = await _dialogManager.ShowDialog(fileScanningViewModel);

            if (result != null) {
                _projectRepository.RootPath = path;
                _projectRepository.RootFolder = result.ProjectFolder;
                Solution = new SolutionViewModel(_settings, path, result.Projects);
            } else {
                RootPath = oldRootPath;
            }
        }

        public void OnDoubleClick(ItemViewModel itemViewModel) {
            var projectViewModel = itemViewModel as ProjectViewModel;
            if (projectViewModel != null) {
                _solution.AddProject(projectViewModel.Project);
            }
        }
    }
}