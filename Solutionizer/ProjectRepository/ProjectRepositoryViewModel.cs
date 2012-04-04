using System.Collections;
using System.Linq;
using Caliburn.Micro;
using Solutionizer.Models;
using Solutionizer.ViewModels;

namespace Solutionizer.ProjectRepository {
    public class ProjectRepositoryViewModel : PropertyChangedBase {
        private string _rootPath;
        private ProjectFolder _rootFolder;
        private IList _nodes;

        public string RootPath {
            get { return _rootPath; }
            set {
                if (_rootPath != value) {
                    _rootPath = value;
                    NotifyOfPropertyChange(() => RootPath);
                }
            }
        }

        public ProjectFolder RootFolder {
            get { return _rootFolder; }
            set {
                if (_rootFolder != value) {
                    _rootFolder = value;
                    NotifyOfPropertyChange(() => RootFolder);
                    Nodes = CreateDirectoryViewModel(_rootFolder, null).Children.ToList();
                }
            }
        }

        public IList Nodes {
            get { return _nodes; }
            private set {
                if (_nodes != value) {
                    _nodes = value;
                    NotifyOfPropertyChange(() => Nodes);
                }
            }
        }

        private DirectoryViewModel CreateDirectoryViewModel(ProjectFolder projectFolder, DirectoryViewModel parent) {
            var viewModel = new DirectoryViewModel(projectFolder, parent);
            foreach (var folder in projectFolder.Folders) {
                viewModel.Directories.Add(CreateDirectoryViewModel(folder, viewModel));
            }
            foreach (var project in projectFolder.Projects) {
                viewModel.Projects.Add(CreateProjectViewModel(project, viewModel));
            }
            return viewModel;
        }

        private ProjectViewModel CreateProjectViewModel(Project project, DirectoryViewModel parent) {
            return new ProjectViewModel(project, parent);
        }

    }
}