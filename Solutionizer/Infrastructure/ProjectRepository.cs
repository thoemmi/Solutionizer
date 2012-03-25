using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Solutionizer.Models;

namespace Solutionizer.Infrastructure {
    public class ProjectRepository {
        private static ProjectRepository _instance;

        public static ProjectRepository Instance {
            get { return _instance ?? (_instance = new ProjectRepository()); }
        }

        private readonly ConcurrentDictionary<string, Project> _projects =
            new ConcurrentDictionary<string, Project>(StringComparer.InvariantCultureIgnoreCase);

        public ProjectFolder GetProjects(string rootPath) {
            var projectFolder = CreateProjectFolder(rootPath, null);

            // load project details asynchronously
            foreach (var p in _projects.Values.ToList().Where(p => !p.IsLoaded)) {
                var project = p;
                Task.Factory.StartNew(project.Load);
            }

            return projectFolder;
        }

        private ProjectFolder CreateProjectFolder(string path, ProjectFolder parent) {
            bool simplifyProjectTree = Settings.Instance.SimplifyProjectTree;

            var projectFolder = new ProjectFolder(path, parent);
            foreach (var subdirectory in Directory.EnumerateDirectories(path)) {
                var folder = CreateProjectFolder(subdirectory, projectFolder);
                if (!folder.IsEmpty) {
                    if (simplifyProjectTree && folder.Folders.Count == 0 && folder.Projects.Count == 1) {
                        // if a subfolder contains a project only and no other folders, just add that project instead of the subfolder
                        var project = folder.Projects[0];
                        project.Parent = projectFolder;
                        projectFolder.Projects.Add(project);
                    } else {
                        projectFolder.Folders.Add(folder);
                    }
                }
            }
            foreach (var projectPath in Directory.EnumerateFiles(path, "*.csproj", SearchOption.TopDirectoryOnly)) {
                projectFolder.Projects.Add(CreateProject(projectPath, projectFolder));
            }

            // if the folder contains no project but one subfolder, skip the subfolder and add its content instead
            if (simplifyProjectTree && projectFolder.Projects.Count == 0 && projectFolder.Folders.Count == 1) {
                var subfolder = projectFolder.Folders[0];
                projectFolder.Folders.Clear();
                foreach (var folder in subfolder.Folders) {
                    folder.Parent = projectFolder;
                    projectFolder.Folders.Add(folder);
                }
                foreach (var project in subfolder.Projects) {
                    project.Parent = projectFolder;
                    projectFolder.Projects.Add(project);
                }
            }

            return projectFolder;
        }

        private Project CreateProject(string projectPath, ProjectFolder projectFolder) {
            return _projects.GetOrAdd(projectPath, path => new Project(path, projectFolder));
        }

        public Project GetProject(string projectPath) {
            Project project;
            _projects.TryGetValue(projectPath, out project);
            return project;
        }

        public bool AllProjectLoaded {
            get { return _projects.Values.All(project => project.IsLoaded); }
        }
    }
}