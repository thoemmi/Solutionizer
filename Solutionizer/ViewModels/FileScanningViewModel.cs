using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;
using Caliburn.Micro;
using NLog;
using Solutionizer.Models;
using Solutionizer.Services;

namespace Solutionizer.ViewModels {
    public class ScanResult {
        public ScanResult(ProjectFolder projectFolder, IDictionary<string, Project> projects) {
            Projects = projects;
            ProjectFolder = projectFolder;
        }

        public ProjectFolder ProjectFolder { get; private set; }
        public IDictionary<string, Project> Projects { get; private set; }
    }

    public class ScanningCommand {
        private static readonly Logger _log = NLog.LogManager.GetCurrentClassLogger();

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;

        private readonly bool _simplifyProjectTree;
        private readonly string _path;

        public IDictionary<string, Project> Projects { get { return _projects; } }

        private string _progressText;
        public event EventHandler ProgressTextChanged;

        public string ProgressText {
            get { return _progressText; }
            private set {
                if (_progressText != value) {
                    _progressText = value;
                    var handler = ProgressTextChanged;
                    if (handler != null) {
                        handler(this, EventArgs.Empty);
                    }
                }
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
            SetTaskbarItemProgressState(TaskbarItemProgressState.Indeterminate);
            try {
                ProjectFolder projectFolder = null;
                try {
                    var sp = System.Diagnostics.Stopwatch.StartNew();
                    projectFolder = GetProjects(_path);
                    _log.Debug("Loading project took {0}", sp.Elapsed);
                } catch (Exception ex) {
                    _log.ErrorException("Loading projects from " + _path + " failed", ex);
                }
                return new ScanResult(projectFolder, _projects);
            }
            finally {
                SetTaskbarItemProgressState(TaskbarItemProgressState.None);
            }
        }

        private static void SetTaskbarItemProgressState(TaskbarItemProgressState state) {
            // HACK speaking to Application.Current and Application.Current.MainWindow inside a ViewModel is not acceptable, but oh my
            try {
                Application.Current.Dispatcher.BeginInvoke((System.Action)(() => Application.Current.MainWindow.TaskbarItemInfo.ProgressState = state));
            } catch (Exception e) {
                _log.ErrorException("Setting TaskbarItemInfo to " + state + " failed", e);
            }
        }

        public ProjectFolder GetProjects(string rootPath) {
            if (!Directory.Exists(rootPath)) {
                return null;
            }

            var projectFolder = CreateProjectFolder(rootPath, null);

            if (_cancellationToken.IsCancellationRequested) {
                return null;
            }

            ProgressText = "Analyzing project dependencies";

            // load project details asynchronously
            var options = new ParallelOptions { CancellationToken = _cancellationToken };
            try {
                Parallel.ForEach(_projects.Values, options, project => {
                    project.Load();
                    project.BrokenProjectReferences.AddRange(project.ProjectReferences.Where(path => !_projects.ContainsKey(path)));
                });
            } catch (OperationCanceledException) {
            }

            if (_cancellationToken.IsCancellationRequested) {
                return null;
            } else {
                return projectFolder;
            }
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
                ProgressText = _projects.Count + " projects loaded";
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

        private void OnProgressTextChanged(object sender, EventArgs eventArgs) {
            ProgressText = _scanningCommand.ProgressText;
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
            _scanningCommand.ProgressTextChanged += OnProgressTextChanged;
            _scanningCommand.Start().ContinueWith(t => {
                Result = t.IsCompleted ? t.Result : null;
                TryClose(true); 
            }, TaskScheduler.Current);
        }

        protected override void OnDeactivate(bool close) {
            _scanningCommand.ProgressTextChanged -= OnProgressTextChanged;
            base.OnDeactivate(close);
        }
    }
}