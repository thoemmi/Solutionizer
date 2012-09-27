using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Solutionizer.Models;
using Solutionizer.Services;

namespace Solutionizer.FileScanning {
    public class ScanResult {
        public ScanResult(ProjectFolder projectFolder, IDictionary<string, Project> projects) {
            Projects = projects;
            ProjectFolder = projectFolder;
        }

        public ProjectFolder ProjectFolder { get; private set; }
        public IDictionary<string, Project> Projects { get; private set; }
    }

    public class ScanningCommand {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;

        private readonly bool _simplifyProjectTree;
        private readonly string _path;

        public IDictionary<string, Project> Projects { get { return _projects; } }

        public event EventHandler ProjectCountChanged;

        private void InvokeProjectCountChanged() {
            var handler = ProjectCountChanged;
            if (handler != null) {
                handler(this,EventArgs.Empty);
            }
        }

        private readonly ConcurrentDictionary<string, Project> _projects =
            new ConcurrentDictionary<string, Project>(StringComparer.InvariantCultureIgnoreCase);

        public ScanningCommand(string path, bool simplifyProjectTree) {
            _path = path;
            _simplifyProjectTree = simplifyProjectTree;
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
        }

        public Task<ScanResult> Start() {
            return Task<ScanResult>.Factory.StartNew(LoadProjects, _cancellationTokenSource.Token);
        }

        public void Cancel() {
            _cancellationTokenSource.Cancel();
        }

        private ScanResult LoadProjects() {
            var projectFolder = GetProjects(_path);
            return new ScanResult(projectFolder, _projects);
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

            var projectFolder = new ProjectFolder(path, parent);
            foreach (var subdirectory in Directory.EnumerateDirectories(path)) {
                var folder = CreateProjectFolder(subdirectory, projectFolder);
                if (folder != null && !folder.IsEmpty) {
                    if (_simplifyProjectTree && folder.Folders.Count == 0 && folder.Projects.Count == 1) {
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
            if (_simplifyProjectTree && projectFolder.Projects.Count == 0 && projectFolder.Folders.Count == 1) {
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
            return _projects.GetOrAdd(projectPath, path => {
                InvokeProjectCountChanged();
                return new Project(path, projectFolder);
            });
        }
    }

    public sealed class FileScanningViewModel : Screen {
        private readonly ISettings _settings;
        private string _loadingText;
        private string _progressText;
        private readonly ScanningCommand _scanningCommand;

        public FileScanningViewModel(ISettings settings, string path) {
            _settings = settings;
            DisplayName = null;

            _loadingText = "Loading projects from " + path.ToLowerInvariant();
            _scanningCommand = new ScanningCommand(path, _settings.SimplifyProjectTree);
        }

        private void OnProjectCountChanged(object sender, EventArgs eventArgs) {
            ProgressText = _scanningCommand.Projects.Count + " projects loaded";
        }

        public string ProgressText {
            get { return _progressText; }
            set {
                if (_progressText != value) {
                    _progressText = value;
                    NotifyOfPropertyChange(() => ProgressText);
                }
            }
        }

        public string LoadingText {
            get { return _loadingText; }
            set {
                if (_loadingText != value) {
                    _loadingText = value;
                    NotifyOfPropertyChange(() => LoadingText);
                }
            }
        }

        public void Cancel() {
            _scanningCommand.Cancel();
        }

        public ScanResult Result { get; private set; }
        
        protected override void OnActivate() {
            base.OnActivate();
            _scanningCommand.ProjectCountChanged += OnProjectCountChanged;
            _scanningCommand.Start().ContinueWith(t => {
                Result = t.Result;  TryClose(true); }, TaskScheduler.Current);
        }

        protected override void OnDeactivate(bool close) {
            _scanningCommand.ProjectCountChanged -= OnProjectCountChanged;
            base.OnDeactivate(close);
        }
    }
}