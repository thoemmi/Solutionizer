using System.Collections.Generic;
using System.IO;

namespace Solutionizer.Models {
    public class ProjectFolder {
        public ProjectFolder(string fullPath, ProjectFolder parent) {
            FullPath = fullPath;
            Parent = parent;
            Name = Path.GetFileName(fullPath);
        }

        public string Name { get; }

        public string FullPath { get; }

        public ProjectFolder Parent { get; set; }

        public bool IsEmpty => Projects.Count == 0 && Folders.Count == 0;

        public List<ProjectFolder> Folders { get; } = new List<ProjectFolder>();

        public List<Project> Projects { get; } = new List<Project>();
    }
}