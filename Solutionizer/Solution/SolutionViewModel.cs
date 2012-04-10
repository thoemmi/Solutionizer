using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using Solutionizer.Infrastructure;
using Solutionizer.Models;
using Solutionizer.ViewModels;
using Solutionizer.VisualStudio;

namespace Solutionizer.Solution {
    public class SolutionViewModel : PropertyChangedBase {
        private readonly ICommand _dropCommand;
        private bool _isSccBound;
        private readonly SolutionFolder _solutionRoot = new SolutionFolder(null);
        private SolutionItem _selectedItem;
        private readonly Services.Settings _settings = Services.Settings.Instance;

        public SolutionViewModel() {
            _dropCommand = new FixedRelayCommand<object>(OnDrop, obj => obj is ProjectViewModel);
        }

        private void OnDrop(object node) {
            var project = ((ProjectViewModel) node).Project;
            project.Load();
            AddProject(project);
        }

        public ICommand DropCommand {
            get { return _dropCommand; }
        }

        public IList SolutionItems {
            get { return _solutionRoot.Items; }
        } 

        public void AddProject(Project project) {
            _isSccBound |= project.IsSccBound;

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
                var referencedProject = Infrastructure.ProjectRepository.Instance.GetProject(projectReference);
                if (referencedProject == null) {
                    // TODO log unknown project
                    continue;
                }

                if (_solutionRoot.ContainsProject(referencedProject)) {
                    continue;
                }

                var folder = GetSolutionFolder(referencedProject);
                if (!folder.ContainsProject(referencedProject)) {
                    folder.AddProject(referencedProject);

                    if (depth > 0) {
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
            }
        }
    }
}