using Solutionizer.ProjectRepository;

namespace Solutionizer {
    using System.ComponentModel.Composition;

    [Export(typeof(IShell))]
    public class ShellViewModel : IShell {
        private readonly ProjectRepositoryViewModel _projectRepository = new ProjectRepositoryViewModel();

        public ProjectRepositoryViewModel ProjectRepository {
            get { return _projectRepository; }
        }

        public ShellViewModel() {
            _projectRepository.RootPath = "xxx";
        }
    }
}
