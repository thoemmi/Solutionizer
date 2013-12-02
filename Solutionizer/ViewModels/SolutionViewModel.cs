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
using Solutionizer.Framework;
using Solutionizer.Helper;
using Solutionizer.Models;
using Solutionizer.Services;

namespace Solutionizer.ViewModels {
    public class SolutionViewModel : PropertyChangedBase {
        public delegate SolutionViewModel Factory(string rootPath, IDictionary<string, Project> projects);

        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly string _rootPath;
        private readonly IDictionary<string, Project> _projects;
        private readonly ICommand _dropCommand;
        private readonly ICommand _removeSelectedItemCommand;
        private readonly ICommand _launchCommand;
        private readonly ICommand _saveCommand;
        private readonly ICommand _clearCommand;

        private bool _isSccBound;
        private bool _isDirty;
        private readonly SolutionFolder _solutionRoot = new SolutionFolder(null);
        private SolutionItem _selectedItem;
        private readonly ISettings _settings;
        private string _fileName;

        public SolutionViewModel(ISettings settings, string rootPath, IDictionary<string, Project> projects) {
            _rootPath = rootPath;
            _projects = projects;
            _settings = settings;
            _dropCommand = new RelayCommand<object>(OnDrop, obj => obj is ProjectViewModel);
            _removeSelectedItemCommand = new RelayCommand(RemoveSolutionItem);
            _settings.PropertyChanged += (sender, args) => {
                if (args.PropertyName == "ShowLaunchElevatedButton") {
                    NotifyOfPropertyChange(() => ShowLaunchElevatedButton);
                }
                if (args.PropertyName == "ShowProjectCount") {
                    NotifyOfPropertyChange(() => ShowProjectCount);
                }
            };
            _launchCommand = new RelayCommand<bool>(Launch, _ => _solutionRoot.Items.Any());
            _saveCommand = new RelayCommand(Save, () => _solutionRoot.Items.Any());
            _clearCommand = new RelayCommand(Clear, () => _solutionRoot.Items.Any());
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
            var exePath = VisualStudioHelper.GetVisualStudioExecutable(_settings.VisualStudioVersion);
            var psi = new ProcessStartInfo(exePath, FileName);
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
                FileName = Path.Combine(targetFolder, DateTime.Now.ToString("yyyy-MM-dd_HHmmss")) + ".sln";
            }

            new SaveSolutionCommand(_settings, FileName, _settings.VisualStudioVersion, this).Execute();
            IsDirty = false;
        }

        private void Clear() {
            _solutionRoot.Items.Clear();
            SelectedItem = null;
            FileName = null;
            IsDirty = false;
            Refresh();
        }

        public ICommand DropCommand {
            get { return _dropCommand; }
        }

        public ICommand LaunchCommand {
            get { return _launchCommand; }
        }

        public ICommand SaveCommand {
            get { return _saveCommand; }
        }

        public ICommand ClearCommand {
            get { return _clearCommand; }
        }

        public ICommand RemoveSelectedItemCommand {
            get { return _removeSelectedItemCommand; }
        }

        public IList<SolutionItem> SolutionItems {
            get { return _solutionRoot.Items; }
        }

        public bool IsDirty {
            get { return _isDirty; }
            set {
                if (_isDirty != value) {
                    _isDirty = value;
                    NotifyOfPropertyChange(() => IsDirty);
                }
            }
        }

        public string RootPath {
            get { return _rootPath; }
        }

        public bool IsSccBound {
            get { return _isSccBound; }
            set {
                if (_isSccBound != value) {
                    _isSccBound = value;
                    NotifyOfPropertyChange(() => IsSccBound);
                }
            }
        }

        public bool ShowProjectCount {
            get { return _settings.ShowProjectCount; }
        }

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

            Refresh();
        }

        private static bool RemoveProject(SolutionFolder solutionFolder, Project project) {
            bool removed = false;
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
                Project referencedProject;
                if (!_projects.TryGetValue(projectReference, out referencedProject)) {
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

        public bool ShowLaunchElevatedButton {
            get { return _settings.ShowLaunchElevatedButton; }
        }

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

                Refresh();
            }
        }
    }
}