using System;
using Caliburn.Micro;
using Solutionizer.Models;

namespace Solutionizer.ViewModels {
    public class ProjectViewModel : PropertyChangedBase, IDisposable {
        private readonly Project _project;
        private readonly DirectoryViewModel _parent;

        public ProjectViewModel(Project project, DirectoryViewModel parent) {
            _project = project;
            _parent = parent;
            if (!_project.IsLoaded) {
                _project.Loaded += ProjectOnLoaded;
            }
        }

        void IDisposable.Dispose() {
            _project.Loaded -= ProjectOnLoaded;
        }

        private void ProjectOnLoaded(object sender, EventArgs eventArgs) {
            _project.Loaded -= ProjectOnLoaded;
            NotifyOfPropertyChange(() => IsLoaded);
        }

        public Project Project {
            get { return _project; }
        }

        public DirectoryViewModel Parent {
            get { return _parent; }
        }

        public string Name {
            get { return _project.Name; }
        }

        public string Path {
            get { return _project.Filepath; }
        }

        public bool IsLoaded {
            get { return _project.IsLoaded; }
        }
    }
}