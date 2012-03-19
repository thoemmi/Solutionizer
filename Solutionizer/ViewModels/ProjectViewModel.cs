using System;
using GalaSoft.MvvmLight;
using Solutionizer.Models;

namespace Solutionizer.ViewModels {
    public class ProjectViewModel : ViewModelBase, IDisposable {
        private readonly Project _project;

        public ProjectViewModel(Project project) {
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
            RaisePropertyChanged(() => IsLoaded);
        }

        public Project Project {
            get { return _project; }
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