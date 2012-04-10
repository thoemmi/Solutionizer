using System.Windows;
using Caliburn.Micro;
using Ookii.Dialogs.Wpf;
using Solutionizer.ProjectRepository;
using Solutionizer.Settings;
using Solutionizer.Solution;

namespace Solutionizer.Shell {
    using System.ComponentModel.Composition;

    [Export(typeof(IShell))]
    public sealed class ShellViewModel : Screen, IShell {
        private readonly Services.Settings _settings;
        private readonly IWindowManager _windowManager;
        private readonly ProjectRepositoryViewModel _projectRepository = new ProjectRepositoryViewModel();
        private readonly SolutionViewModel _solution = new SolutionViewModel();

        [ImportingConstructor]
        public ShellViewModel(Services.Settings settings, IWindowManager windowManager) {
            _settings = settings;
            _windowManager = windowManager;
            DisplayName = "Solutionizer";
        }

        public ProjectRepositoryViewModel ProjectRepository {
            get { return _projectRepository; }
        }

        public SolutionViewModel Solution {
            get { return _solution; }
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
            _windowManager.ShowDialog(new SettingsViewModel());
        }

        private void LoadProjects(string path) {
            _projectRepository.RootPath = path;
            _projectRepository.RootFolder = Infrastructure.ProjectRepository.Instance.GetProjects(path);
            //Solution = new SolutionViewModel(dlg.SelectedPath);
        }
    }
}
