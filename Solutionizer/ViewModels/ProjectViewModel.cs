using System;
using Solutionizer.Models;

namespace Solutionizer.ViewModels {
    public class ProjectViewModel : ItemViewModel {
        private readonly Project _project;
        private bool _isVisible = true;

        public ProjectViewModel(DirectoryViewModel parent, Project project) : base(parent) {
            _project = project;
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

        public bool IsVisible {
            get { return _isVisible; }
            private set {
                if (_isVisible != value) {
                    _isVisible = value;
                    NotifyOfPropertyChange(() => IsVisible);
                }
            }
        }

        public override void Filter(string filter) {
            IsVisible = String.IsNullOrEmpty(filter) || Name.ToUpperInvariant().Contains(filter.ToUpperInvariant());
        }

        public bool HasBrokenProjectReferences {
            get { return _project.HasBrokenProjectReferences; }
        }

        public string BrokenProjectReferences {
            get { return _project.BrokenProjectReferences != null ? String.Join(",\n", _project.BrokenProjectReferences) : String.Empty; }
        }
    }
}