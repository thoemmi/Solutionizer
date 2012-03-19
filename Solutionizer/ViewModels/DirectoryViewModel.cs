using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;

namespace Solutionizer.ViewModels {
    public class DirectoryViewModel : ViewModelBase {
        private readonly string _name;
        private readonly string _path;
        private readonly List<DirectoryViewModel> _directories;
        private readonly List<ProjectViewModel> _projects;

        public DirectoryViewModel(string name, string path, List<ProjectViewModel> projects) {
            _name = name;
            _path = path;
            _directories = new List<DirectoryViewModel>();
            _projects = projects;
        }

        public string Name {
            get { return _name; }
        }

        public string Path {
            get { return _path; }
        }

        public IList<object> Children {
            get { return _directories.Cast<object>().Concat(_projects).ToList(); }
        }
    }
}