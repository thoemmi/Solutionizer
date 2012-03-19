using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            var projects = new List<Project>();
            // enumerate all projects and yield them
            foreach (var project in GetOrAddProjects(rootPath)) {
                projects.Add(project);
                yield return project;
            }

            // load project details asynchronously
            foreach (var p in projects) {
                var project = p;
                Task.Factory.StartNew(project.Load);
            }
        }

        private IEnumerable<Project> GetOrAddProjects(string rootPath) {
            return from projectPath in Directory.EnumerateFiles(rootPath, "*.csproj", SearchOption.AllDirectories)
                   select _projects.GetOrAdd(projectPath, path => new Project(path));
        }
    }
}