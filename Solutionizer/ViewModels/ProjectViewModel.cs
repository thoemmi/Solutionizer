using System;
using Solutionizer.Models;

namespace Solutionizer.ViewModels {
    public class ProjectViewModel : ItemViewModel, IDisposable {
        private readonly Project _project;

        public ProjectViewModel(DirectoryViewModel parent, Project project) : base(parent) {
            _project = project;
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

        public override string Name {
            get { return _project.Name; }
        }

        public override string Path {
            get { return _project.Filepath; }
        }

        public bool IsLoaded {
            get { return _project.IsLoaded; }
        }

        public bool HasBrokenProjectReferences {
            get { return _project.HasBrokenProjectReferences; }
        }
    }
}