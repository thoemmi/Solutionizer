using Caliburn.Micro;
using Solutionizer.ProjectRepository;
using Solutionizer.Services;
using Solutionizer.Solution;

namespace Solutionizer.Shell {
    using System.ComponentModel.Composition;

    [Export(typeof(IShell))]
    public sealed class ShellViewModel : Screen, IShell {
        private readonly Settings _settings;
        private readonly ProjectRepositoryViewModel _projectRepository = new ProjectRepositoryViewModel();
        private readonly SolutionViewModel _solution = new SolutionViewModel();

        [ImportingConstructor]
        public ShellViewModel(Settings settings) {
            _settings = settings;
            DisplayName = "Solutionizer";
        }

        public ProjectRepositoryViewModel ProjectRepository {
            get { return _projectRepository; }
        }

        public SolutionViewModel Solution {
            get { return _solution; }
        }

        public Settings Settings {
            get { return _settings; }
        }

        protected override void OnViewLoaded(object view) {
            base.OnViewLoaded(view);

            if (_settings.ScanOnStartup) {
                _projectRepository.RootPath = _settings.RootPath;
                //_projectRepository.RootFolder = Solutionizer.Infrastructure.ProjectRepository.Instance.GetProjects(_settings.RootPath);
            }
        }
    }
}
