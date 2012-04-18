using System.Collections.Generic;
using System.Linq;
using Solutionizer.Models;

namespace Solutionizer.ViewModels {
    public class DirectoryViewModel : ItemViewModel {
        private readonly ProjectFolder _projectFolder;
        private readonly List<DirectoryViewModel> _directories = new List<DirectoryViewModel>();
        private readonly List<ProjectViewModel> _projects = new List<ProjectViewModel>();

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

        public IList<ItemViewModel> Children {
            get { return _directories.Cast<ItemViewModel>().Concat(_projects).ToList(); }
        }
    }
}