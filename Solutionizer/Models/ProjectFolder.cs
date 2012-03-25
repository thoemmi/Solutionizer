using System.Collections.Generic;

namespace Solutionizer.Models {
    public class ProjectFolder {
        private readonly string _name;
        private readonly List<ProjectFolder> _folders = new List<ProjectFolder>();
        private readonly List<Project> _projects = new List<Project>();

        public ProjectFolder(string name) {
            _name = name;
        }

        public string Name {
            get { return _name; }
        }

        public List<ProjectFolder> Folders {
            get { return _folders; }
        }

        public List<Project> Projects {
            get { return _projects; }
        }
    }
}