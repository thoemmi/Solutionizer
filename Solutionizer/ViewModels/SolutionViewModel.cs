using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using NLog;
using Ookii.Dialogs.Wpf;
using Solutionizer.Commands;
using Solutionizer.Extensions;
using Solutionizer.Helper;
using Solutionizer.Models;
using Solutionizer.Services;
using TinyLittleMvvm;

namespace Solutionizer.ViewModels {
    public class SolutionViewModel : PropertyChangedBase {
        public delegate SolutionViewModel Factory(string rootPath, IDictionary<string, Project> projects);

        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly IStatusMessenger _statusMessenger;
        private readonly string _rootPath;
        private readonly IDictionary<string, Project> _projects;

        private bool _isSccBound;
        private bool _isDirty;
        private readonly SolutionFolder _solutionRoot = new SolutionFolder(null);
        private SolutionItem _selectedItem;
        private readonly ISettings _settings;
        private readonly IVisualStudioInstallationsProvider _visualStudioInstallationsProvider;
        private string _fileName;

        public SolutionViewModel(IStatusMessenger statusMessenger, ISettings settings, IVisualStudioInstallationsProvider visualStudioInstallationsProvider, string rootPath, IDictionary<string, Project> projects) {
            _statusMessenger = statusMessenger;
            _rootPath = rootPath;
            _projects = projects;
            _settings = settings;
            _visualStudioInstallationsProvider = visualStudioInstallationsProvider;
            DropCommand = new RelayCommand<object>(OnDrop, obj => obj is ProjectViewModel);
            RemoveSelectedItemCommand = new RelayCommand(RemoveSolutionItem);
            _settings.PropertyChanged += (sender, args) => {
                if (args.PropertyName == "ShowLaunchElevatedButton") {
                    NotifyOfPropertyChange(() => ShowLaunchElevatedButton);
                }
                if (args.PropertyName == "ShowProjectCount") {
                    NotifyOfPropertyChange(() => ShowProjectCount);
                }
            };
            LaunchCommand = new RelayCommand<bool>(Launch, _ => _solutionRoot.Items.Any());
            SaveCommand = new RelayCommand(Save, () => _solutionRoot.Items.Any());
            ClearCommand = new RelayCommand(Clear, () => _solutionRoot.Items.Any());
        }

        private void OnDrop(object node) {
            var project = ((ProjectViewModel) node).Project;
            project.Load();
            AddProject(project);
        }

        private string GetTargetFolder() {
            switch (_settings.SolutionTargetLocation) {
                case SolutionTargetLocation.TempFolder:
                    return Path.GetTempPath();
                case SolutionTargetLocation.CustomFolder:
                    return _settings.CustomTargetFolder;
                case SolutionTargetLocation.BelowRootPath:
                    return Path.Combine(_rootPath, _settings.CustomTargetSubfolder);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Launch(bool elevated) {
            InternalSave(null);
            var installation = _visualStudioInstallationsProvider.GetVisualStudioInstallationByVersionId(_settings.VisualStudioVersion);
            var psi = new ProcessStartInfo(installation.DevEnvExePath, "\"" + FileName + "\"");
            if (elevated) {
                psi.Verb = "runas";
            }
            try {
                Process.Start(psi);
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
            } catch (Win32Exception ex) {
                // if NativeErrorCode = 1223, the user cancelled the UAC dialog
                if (ex.NativeErrorCode != 1223) {
                    throw;
                }
            }
        }

        private void Save() {
            var dlg = new VistaSaveFileDialog {
                Filter = "Solution File (*.sln)|*.sln",
                AddExtension = true,
                DefaultExt = ".sln"
            };
            if (dlg.ShowDialog() == true) {
                InternalSave(dlg.FileName);
            }
        }

        private void InternalSave(string fileName) {
            if (!String.IsNullOrEmpty(fileName)) {
                FileName = fileName;
            }

            if (String.IsNullOrEmpty(FileName)) {
                var targetFolder = GetTargetFolder();
                if (!Directory.Exists(targetFolder)) {
                    Directory.CreateDirectory(targetFolder);
                }

                var firstProject = _solutionRoot.Items.OfType<SolutionProject>().FirstOrDefault();
                if (firstProject != null) {
                    FileName = Path.Combine(targetFolder, firstProject.Name + " " + DateTime.Now.ToString("yyyy-MM-dd_HHmmss")) + ".sln";
                } else {
                    FileName = Path.Combine(targetFolder, DateTime.Now.ToString("yyyy-MM-dd_HHmmss")) + ".sln";
                }
            }

            new SaveSolutionCommand(_settings, _visualStudioInstallationsProvider, FileName, _settings.VisualStudioVersion, this).Execute();
            _statusMessenger.Show(String.Format("Solution saved as '{0}'.", FileName));
            IsDirty = false;
        }

        private void Clear() {
            _solutionRoot.Items.Clear();
            SelectedItem = null;
            FileName = null;
            IsDirty = false;
            _statusMessenger.Show("Solution cleared.");
            Refresh();
        }

        public ICommand DropCommand { get; }

        public ICommand LaunchCommand { get; }

        public ICommand SaveCommand { get; }

        public ICommand ClearCommand { get; }

        public ICommand RemoveSelectedItemCommand { get; }

        public IList<SolutionItem> SolutionItems => _solutionRoot.Items;

        public bool IsDirty {
            get { return _isDirty; }
            set {
                if (_isDirty != value) {
                    _isDirty = value;
                    NotifyOfPropertyChange(() => IsDirty);
                }
            }
        }

        public string RootPath => _rootPath;

        public bool IsSccBound {
            get { return _isSccBound; }
            set {
                if (_isSccBound != value) {
                    _isSccBound = value;
                    NotifyOfPropertyChange(() => IsSccBound);
                }
            }
        }

        public bool ShowProjectCount => _settings.ShowProjectCount;

        public string FileName {
            get { return _fileName; }
            set {
                if (_fileName != value) {
                    _fileName = value;
                    NotifyOfPropertyChange(() => FileName);
                }
            }
        }

        public void AddProject(Project project) {
            IsSccBound |= project.IsSccBound;

            if (_solutionRoot.ContainsProject(project)) {
                return;
            }

            _solutionRoot.AddProject(project);

            var referenceFolder = _solutionRoot.Items.OfType<SolutionFolder>().SingleOrDefault();
            if (referenceFolder != null) {
                RemoveProject(referenceFolder, project);
            }

            if (_settings.IncludeReferencedProjects) {
                AddReferencedProjects(project, _settings.ReferenceTreeDepth);
            }

            var projectCount = SolutionItems.Flatten<SolutionItem, SolutionProject, SolutionFolder>(p => p.Items).Count();
            _statusMessenger.Show($"{projectCount} projects in the solution.");

            Refresh();
        }

        private static bool RemoveProject(SolutionFolder solutionFolder, Project project) {
            var removed = false;
            var item = solutionFolder.Items.SingleOrDefault(p => p.Guid == project.Guid);
            if (item != null) {
                solutionFolder.Items.Remove(item);
                removed = true;
            }

            var foldersToRemove = new List<SolutionFolder>();
            foreach (var subfolder in solutionFolder.Items.OfType<SolutionFolder>()) {
                if (RemoveProject(subfolder, project)) {
                    removed = true;
                    if (subfolder.Items.Count == 0) {
                        foldersToRemove.Add(subfolder);
                    }
                }
            }
            if (foldersToRemove.Count > 0) {
                foreach (var folder in foldersToRemove) {
                    solutionFolder.Items.Remove(folder);
                }
            }

            return removed;
        }

        private void AddReferencedProjects(Project project, int depth) {
            foreach (var projectReference in project.ProjectReferences) {
                if (!_projects.TryGetValue(projectReference, out var referencedProject)) {
                    // TODO Present to user?
                    _log.Warn("Project {0} references unknown project {1}", project.Name, projectReference);
                    continue;
                }

                if (_solutionRoot.ContainsProject(referencedProject)) {
                    continue;
                }

                var folder = GetSolutionFolder(referencedProject);
                if (!folder.ContainsProject(referencedProject)) {
                    folder.AddProject(referencedProject);

                    if (depth > 1) {
                        AddReferencedProjects(referencedProject, depth - 1);
                    }
                }
            }
        }

        private SolutionFolder GetSolutionFolder(Project project) {
            // get chain of folders from root to project
            var folderNames = new List<string>();
            var projectFolder = project.Parent;
            while (projectFolder.Parent != null) {
                folderNames.Add(projectFolder.Name);
                projectFolder = projectFolder.Parent;
            }
            folderNames.Reverse();

            var folder = GetOrCreateReferenceFolder();
            foreach (var folderName in folderNames) {
                folder = folder.GetOrCreateSubfolder(folderName);
            }
            return folder;
        }

        private SolutionFolder GetOrCreateReferenceFolder() {
            return _solutionRoot.GetOrCreateSubfolder(_settings.ReferenceFolderName);
        }

        public SolutionItem SelectedItem {
            get { return _selectedItem; }
            set {
                if (_selectedItem != value) {
                    _selectedItem = value;
                    NotifyOfPropertyChange(() => SelectedItem);
                }
            }
        }

        public bool ShowLaunchElevatedButton => _settings.ShowLaunchElevatedButton;

        public void RemoveSolutionItem() {
            if (_selectedItem != null) {
                var parentFolder = _selectedItem.Parent;

                var index = parentFolder.Items.IndexOf(_selectedItem);
                parentFolder.Items.Remove(_selectedItem);

                if (index >= 0) {
                    if (index >= parentFolder.Items.Count) {
                        index--;
                    }
                    SelectedItem = index >= 0 ? parentFolder.Items[index] : parentFolder;
                }

                var projectCount = SolutionItems.Flatten<SolutionItem, SolutionProject, SolutionFolder>(p => p.Items).Count();
                _statusMessenger.Show($"{projectCount} projects in the solution.");

                Refresh();
            }
        }
    }
}