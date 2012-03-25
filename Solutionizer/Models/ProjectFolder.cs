using System.Collections.Generic;
using System.IO;

namespace Solutionizer.Models {
    public class ProjectFolder {
        private readonly string _name;
        private readonly string _fullPath;
        private ProjectFolder _parent;
        private readonly List<ProjectFolder> _folders = new List<ProjectFolder>();
        private readonly List<Project> _projects = new List<Project>();

        public ProjectFolder(string fullPath, ProjectFolder parent) {
            _fullPath = fullPath;
            _parent = parent;
            _name = Path.GetFileName(fullPath);
        }

        public string Name {
            get { return _name; }
        }

        public string FullPath {
            get { return _fullPath; }
        }

        public ProjectFolder Parent {
            get { return _parent; }
            set { _parent = value; }
        }

        public bool IsEmpty {
            get { return _projects.Count == 0 && _folders.Count == 0; }
        }

        public List<ProjectFolder> Folders {
            get { return _folders; }
        }

        public List<Project> Projects {
            get { return _projects; }
        }
    }
}