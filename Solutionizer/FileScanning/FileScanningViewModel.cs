using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Solutionizer.Models;

namespace Solutionizer.FileScanning {
    public sealed class FileScanningViewModel : Screen {
        private string _progressText;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;

        public FileScanningViewModel() {
            DisplayName = null;

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
        }

        public string Path { get; set; }
        public ProjectFolder ProjectFolder { get; private set; }
        public IDictionary<string, Project> Projects { get { return _projects; } }

        public string ProgressText {
            get { return _progressText; }
            set {
                if (_progressText != value) {
                    _progressText = value;
                    NotifyOfPropertyChange(() => ProgressText);
                }
            }
        }

        public void Cancel() {
            _cancellationTokenSource.Cancel();
        }

        private readonly ConcurrentDictionary<string, Project> _projects =
            new ConcurrentDictionary<string, Project>(StringComparer.InvariantCultureIgnoreCase);
        
        protected override void OnActivate() {
            base.OnActivate();
            Task.Factory
                .StartNew(LoadProjects, _cancellationTokenSource.Token)
                .ContinueWith(t => TryClose(true), TaskScheduler.Current);
        }

        public void LoadProjects() {
            ProjectFolder = GetProjects(Path);
        }

        public ProjectFolder GetProjects(string rootPath) {
            var projectFolder = CreateProjectFolder(rootPath, null);

            if (_cancellationToken.IsCancellationRequested) {
                return null;
            }

            // load project details asynchronously
            foreach (var p in _projects.Values.ToList().Where(p => !p.IsLoaded)) {
                var project = p;
                Task.Factory.StartNew(project.Load);
            }

            return projectFolder;
        }

        private ProjectFolder CreateProjectFolder(string path, ProjectFolder parent) {
            if (_cancellationToken.IsCancellationRequested) {
                return null;
            }

            var simplifyProjectTree = Services.Settings.Instance.SimplifyProjectTree;

            var projectFolder = new ProjectFolder(path, parent);
            foreach (var subdirectory in Directory.EnumerateDirectories(path)) {
                var folder = CreateProjectFolder(subdirectory, projectFolder);
                if (folder != null && !folder.IsEmpty) {
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
            ProgressText = _projects.Count + " projects loaded";
            return _projects.GetOrAdd(projectPath, path => new Project(path, projectFolder));
        }
    }
}