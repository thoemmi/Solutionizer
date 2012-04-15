using System.Windows;
using Caliburn.Micro;
using Ookii.Dialogs.Wpf;
using Solutionizer.FileScanning;
using Solutionizer.Infrastructure;
using Solutionizer.ProjectRepository;
using Solutionizer.Settings;
using Solutionizer.Solution;

namespace Solutionizer.Shell {
    using System.ComponentModel.Composition;

    [Export(typeof(IShell))]
    public sealed class ShellViewModel : Screen, IShell {
        private readonly Services.Settings _settings;
        private readonly IDialogManager _dialogManager;
        private readonly ProjectRepositoryViewModel _projectRepository = new ProjectRepositoryViewModel();
        private SolutionViewModel _solution;

        [ImportingConstructor]
        public ShellViewModel(Services.Settings settings, IDialogManager dialogManager) {
            _settings = settings;
            _dialogManager = dialogManager;
            DisplayName = "Solutionizer";
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
            _dialogManager.ShowDialog(new SettingsViewModel());
        }

        public IDialogManager Dialogs {
            get { return _dialogManager; }
        }

        private void LoadProjects(string path) {
            var fileScanningViewModel = new FileScanningViewModel();
            fileScanningViewModel.Path = path;
            _dialogManager.ShowDialog(fileScanningViewModel);

            fileScanningViewModel.Deactivated += (sender, args) => {
                if (fileScanningViewModel.ProjectFolder != null) {
                    _projectRepository.RootPath = path;
                    _projectRepository.RootFolder = fileScanningViewModel.ProjectFolder;
                    Solution = new SolutionViewModel(path, fileScanningViewModel.Projects);
                }
            };
        }
    }
}