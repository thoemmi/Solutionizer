using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Solutionizer.Helper;
using Solutionizer.Models;

namespace Solutionizer.Infrastructure {
    public class ProjectRepository {
        private static ProjectRepository _instance;

        public static ProjectRepository Instance {
            get {
                return _instance ?? (_instance = new ProjectRepository());
            }
        }

        private readonly ConcurrentDictionary<string,Project> _projects = new ConcurrentDictionary<string, Project>(StringComparer.InvariantCultureIgnoreCase);

        public IEnumerable<Project> GetProjects(string rootPath) {
            var projects = GetOrAddProjects(rootPath).ToList();

            PopulateLogicalPaths(rootPath, projects);

            // load project details asynchronously
            foreach (var p in projects) {
                var project = p;
                Task.Factory.StartNew(project.Load);
            }
            return projects;
        }

        private void PopulateLogicalPaths(string rootPath, List<Project> projects) {
            var separators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
            var ps = projects.ToDictionary(
                project => project,
                project => FileSystem.GetRelativePath(rootPath, Path.GetDirectoryName(project.Filepath)).Split(separators, StringSplitOptions.RemoveEmptyEntries)
                );

            var folder = new ProjectFolder(String.Empty);
            Populate(folder, ps);
        }

        private void Populate(ProjectFolder folder, Dictionary<Project, string[]> projects) {
            var directChildren = projects.Where(pair => pair.Value.Length == 0).Select(pair => pair.Key).ToList();
            folder.Projects.AddRange(directChildren);
            foreach (var directChild in directChildren) {
                projects.Remove(directChild);
            }

            var subfolderNames = projects.GroupBy(pair => pair.Value[0]).ToList();
            foreach (var subfolderName in subfolderNames) {
                var subthing = new ProjectFolder(subfolderName.Key);
                folder.Folders.Add(subthing);

                var subprojects = subfolderName.ToDictionary(pair => pair.Key, pair => pair.Value.Skip(1).ToArray());
                Populate(subthing, subprojects);
            }
        }

        private IEnumerable<Project> GetOrAddProjects(string rootPath) {
            return from projectPath in Directory.EnumerateFiles(rootPath, "*.csproj", SearchOption.AllDirectories)
                   select _projects.GetOrAdd(projectPath, path => new Project(path));
        }

        public Project GetProject(string filepath) {
            Project project;
            _projects.TryGetValue(filepath, out project);
            return project;
        }
    }
}