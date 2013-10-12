﻿using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Ookii.Dialogs.Wpf;
using Solutionizer.Infrastructure;
using System.ComponentModel.Composition;

namespace Solutionizer.ViewModels {
    [Export(typeof(IShell))]
    public sealed class ShellViewModel : Screen, IShell {
        private readonly Services.Settings _settings;
        private readonly IDialogManager _dialogManager;
        private readonly UpdateManager _updateManager;
        private readonly ProjectRepositoryViewModel _projectRepository;
        private SolutionViewModel _solution;
        private string _rootPath;
        private bool _areUpdatesAvailable;

        [ImportingConstructor]
        public ShellViewModel(Services.Settings settings, IDialogManager dialogManager) {
            _settings = settings;
            _projectRepository = new ProjectRepositoryViewModel(settings);
            _updateManager = new UpdateManager(_settings, AppEnvironment.CurrentVersion);
            _updateManager.UpdatesAvailable +=
                (sender, args) => AreUpdatesAvailable = _updateManager.Releases != null && _updateManager.Releases.Any(r => r.IsNew && (_settings.IncludePrereleaseUpdates || !r.IsPrerelease));
            _dialogManager = dialogManager;
            DisplayName = "Solutionizer";
        }

        public string RootPath {
            get { return _rootPath; }
            set {
                if (_rootPath != value) {
                    _rootPath = value;
                    NotifyOfPropertyChange(() => RootPath);
                }
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

        public Services.Settings Settings {
            get { return _settings; }
        }

        protected override void OnViewLoaded(object view) {
            base.OnViewLoaded(view);

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

        public void ShowSettings() {
            _dialogManager.ShowDialog(new SettingsViewModel(_settings));
        }

        public void ShowUpdate(bool checkForUpdates) {
            _dialogManager.ShowDialog(new UpdateViewModel(_updateManager, _dialogManager, _settings, checkForUpdates));
        }

        public void ShowAbout() {
            _dialogManager.ShowDialog(new AboutViewModel());
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

        private void LoadProjects(string path) {
            var fileScanningViewModel = new FileScanningViewModel(_settings, path);
            _dialogManager.ShowDialog(fileScanningViewModel);

            fileScanningViewModel.Deactivated += (sender, args) => {
                if (fileScanningViewModel.Result != null) {
                    _projectRepository.RootPath = path;
                    _projectRepository.RootFolder = fileScanningViewModel.Result.ProjectFolder;
                    Solution = new SolutionViewModel(_settings, path, fileScanningViewModel.Result.Projects);
                    DisplayName = "Solutionizer -";
                    RootPath = path;
                }
            };
        }

        public void OnDoubleClick(ItemViewModel itemViewModel) {
            var projectViewModel = itemViewModel as ProjectViewModel;
            if (projectViewModel != null) {
                _solution.AddProject(projectViewModel.Project);
            }
        }
    }
}