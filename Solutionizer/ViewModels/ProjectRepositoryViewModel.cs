using System.Collections;
using System.Linq;
using Caliburn.Micro;
using Solutionizer.Models;
using Solutionizer.Services;
using Solutionizer.Extensions;

namespace Solutionizer.ViewModels {
    public class ProjectRepositoryViewModel : PropertyChangedBase {
        private readonly ISettings _settings;
        private string _rootPath;
        private ProjectFolder _rootFolder;
        private IList _nodes;
        private string _filter;

        public ProjectRepositoryViewModel(ISettings settings) {
            _settings = settings;
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
                if (!ReferenceEquals(_nodes, value)) {
                    _nodes = value;
                    NotifyOfPropertyChange(() => Nodes);
                }
            }
        }

        public string Filter {
            get { return _filter; }
            set {
                if (_filter != value) {
                    _filter = value;
                    NotifyOfPropertyChange(() => Filter);
                    UpdateFilter();
                }
            }
        }

        private void UpdateFilter() {
            if (_nodes != null) {
                foreach (var item in _nodes.Cast<ItemViewModel>()) {
                    item.Filter(_filter);
                }
            }
        }

        private DirectoryViewModel CreateDirectoryViewModel(ProjectFolder projectFolder, DirectoryViewModel parent) {
            var viewModel = new DirectoryViewModel(parent, projectFolder);
            if (_settings.IsFlatMode) {
                foreach (var project in new[]{projectFolder}.Flatten(f => f.Projects, f => f.Folders)) {
                    viewModel.Projects.Add(CreateProjectViewModel(project, viewModel));
                }
            } else {
                foreach (var folder in projectFolder.Folders) {
                    viewModel.Directories.Add(CreateDirectoryViewModel(folder, viewModel));
                }
                foreach (var project in projectFolder.Projects) {
                    viewModel.Projects.Add(CreateProjectViewModel(project, viewModel));
                }
            }
            return viewModel;
        }

        private ProjectViewModel CreateProjectViewModel(Project project, DirectoryViewModel parent) {
            return new ProjectViewModel(parent, project);
        }
    }
}