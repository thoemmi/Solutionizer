using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Solutionizer.Models;

namespace Solutionizer.ViewModels {
    public class DirectoryViewModel : PropertyChangedBase {
        private readonly ProjectFolder _projectFolder;
        private readonly DirectoryViewModel _parent;
        private readonly List<DirectoryViewModel> _directories = new List<DirectoryViewModel>();
        private readonly List<ProjectViewModel> _projects = new List<ProjectViewModel>();

        public DirectoryViewModel(ProjectFolder projectFolder, DirectoryViewModel parent) {
            _projectFolder = projectFolder;
            _parent = parent;
        }

        public string Name {
            get { return _projectFolder.Name; }
        }

        public string Path {
            get { return _projectFolder.FullPath; }
        }

        public DirectoryViewModel Parent {
            get { return _parent; }
        }

        public List<DirectoryViewModel> Directories {
            get { return _directories; }
        }

        public List<ProjectViewModel> Projects {
            get { return _projects; }
        }

        public IList<object> Children {
            get { return _directories.Cast<object>().Concat(_projects).ToList(); }
        }
    }
}