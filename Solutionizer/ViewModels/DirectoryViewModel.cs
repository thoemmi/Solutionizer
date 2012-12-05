using System.Collections.Generic;
using System.Linq;
using Solutionizer.Models;

namespace Solutionizer.ViewModels {
    public class DirectoryViewModel : ItemViewModel {
        private readonly ProjectFolder _projectFolder;
        private readonly List<DirectoryViewModel> _directories = new List<DirectoryViewModel>();
        private readonly List<ProjectViewModel> _projects = new List<ProjectViewModel>();
        private bool _isVisible = true;

        public DirectoryViewModel(DirectoryViewModel parent, ProjectFolder projectFolder) : base(parent) {
            _projectFolder = projectFolder;
        }

        public override string Name {
            get { return _projectFolder.Name; }
        }

        public override string Path {
            get { return _projectFolder.FullPath; }
        }

        public ProjectFolder ProjectFolder {
            get { return _projectFolder; }
        }

        public List<DirectoryViewModel> Directories {
            get { return _directories; }
        }

        public List<ProjectViewModel> Projects {
            get { return _projects; }
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
            foreach (var directory in _directories) {
                directory.Filter(filter);
            }
            foreach (var project in _projects) {
                project.Filter(filter);
            }
            IsVisible = string.IsNullOrWhiteSpace(filter) || _directories.Any(d => d.IsVisible) || _projects.Any(p => p.IsVisible);
        }

        public IList<ItemViewModel> Children {
            get { return _directories.Cast<ItemViewModel>().OrderBy(d => d.Name).Concat(_projects.OrderBy(p => p.Name)).ToList(); }
        }
    }
}