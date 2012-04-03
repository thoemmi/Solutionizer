using Caliburn.Micro;

namespace Solutionizer.ProjectRepository {
    public class ProjectRepositoryViewModel : PropertyChangedBase {
        private string _rootPath;

        public string RootPath {
            get { return _rootPath; }
            set {
                if (_rootPath != value) {
                    _rootPath = value;
                    NotifyOfPropertyChange(() => RootPath);
                }
            }
        }
    }
}