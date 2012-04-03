using Solutionizer.ProjectRepository;
using Solutionizer.Solution;

namespace Solutionizer.Shell {
    using System.ComponentModel.Composition;

    [Export(typeof(IShell))]
    public class ShellViewModel : IShell {
        private readonly ProjectRepositoryViewModel _projectRepository = new ProjectRepositoryViewModel();
        private readonly SolutionViewModel _solution = new SolutionViewModel();

        public ShellViewModel() {
            _projectRepository.RootPath = "xxx";
        }

        public ProjectRepositoryViewModel ProjectRepository {
            get { return _projectRepository; }
        }

        public SolutionViewModel Solution {
            get { return _solution; }
        }
    }
}
