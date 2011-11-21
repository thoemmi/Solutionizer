using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using Solutionizer.Scanner;

namespace Solutionizer.ViewModels {
    public class SolutionViewModel : ViewModelBase {
        private readonly SolutionFolder _solutionFolder = new SolutionFolder();
        private readonly ICommand _dropCommand;
        private bool _solutionHasItems;
        private bool _isDirty;

        private bool _isSccBound;

        public SolutionViewModel() {
            _dropCommand = new FixedRelayCommand<object>(OnDrop, obj => obj is FileNode);
            _solutionFolder.Items.CollectionChanged += (sender, args) => {
                SolutionHasItems = _solutionFolder.Items.Count > 0;
                IsDirty = true;
            };
        }

        private void OnDrop(object node) {
            var project = Project.Load(((FileNode) node).Path);
            AddProject(project);
        }

        public SolutionFolder SolutionFolder {
            get { return _solutionFolder; }
        }

        public bool SolutionHasItems {
            get { return _solutionHasItems; }
            set {
                if (_solutionHasItems != value) {
                    _solutionHasItems = value;
                    RaisePropertyChanged(() => SolutionHasItems);
                }
            }
        }

        public bool IsDirty {
            get { return _isDirty; }
            set {
                if (_isDirty != value) {
                    _isDirty = value;
                    RaisePropertyChanged(() => IsDirty);
                }
            }
        }

        public void AddProject(Project project) {
            _isSccBound |= project.IsSccBound;

            if (_solutionFolder.ContainsProject(project)) {
                return;
            }

            _solutionFolder.AddProject(project);

            var referenceFolder = _solutionFolder.Items.OfType<SolutionFolder>().SingleOrDefault();
            if (referenceFolder != null) {
                RemoveProject(referenceFolder, project);
            }

            AddReferencedProjects(project, 6);
        }

        private static void RemoveProject(SolutionFolder solutionFolder, Project project) {
            var item = solutionFolder.Items.SingleOrDefault(p => p.Guid == project.Guid);
            if (item != null) {
                solutionFolder.Items.Remove(item);
                return;
            }

            foreach (var subfolder in solutionFolder.Items.OfType<SolutionFolder>()) {
                RemoveProject(subfolder, project);
            }
        }

        private void AddReferencedProjects(Project project, int depth) {
            var referenceFolder = GetOrCreateReferenceFolder();

            foreach (var projectReference in project.ProjectReferences) {
                var referencedProject = Project.Load(projectReference);

                if (_solutionFolder.ContainsProject(referencedProject)) {
                    continue;
                }

                if (referenceFolder.ContainsProject(referencedProject)) {
                    continue;
                }

                referenceFolder.AddProject(referencedProject);

                if (depth > 0) {
                    AddReferencedProjects(referencedProject, depth - 1);
                }
            }
        }

        private SolutionFolder GetOrCreateReferenceFolder() {
            return _solutionFolder.GetOrCreateSubfolder("References");
        }

        public bool IsSccBound {
            get { return _isSccBound; }
            set {
                if (_isSccBound != value) {
                    _isSccBound = value;
                    RaisePropertyChanged(() => IsSccBound);
                }
            }
        }

        public ICommand DropCommand {
            get { return _dropCommand; }
        }
    }

    public abstract class SolutionItem : ViewModelBase {
        private string _name;

        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    _name = value;
                    RaisePropertyChanged(() => Name);
                }
            }
        }

        public Guid Guid { get; set; }
    }

    public class SolutionItemComparer : IComparer<SolutionItem> {
        public int Compare(SolutionItem x, SolutionItem y) {
            var xIsFolder = x is SolutionFolder;
            var yIsFolder = y is SolutionFolder;
            if (xIsFolder && !yIsFolder) {
                return -1;
            }
            if (!xIsFolder && yIsFolder) {
                return +1;
            }
            return StringComparer.InvariantCultureIgnoreCase.Compare(x.Name, y.Name);
        }
    }

    public class SolutionFolder : SolutionItem {
        private readonly SortedObservableCollection<SolutionItem> _items =
            new SortedObservableCollection<SolutionItem>(new SolutionItemComparer());

        public ObservableCollection<SolutionItem> Items {
            get { return _items; }
        }

        public bool ContainsProject(Project project) {
            return _items.OfType<SolutionProject>().Any(p => p.Guid == project.Guid);
        }

        public SolutionFolder GetOrCreateSubfolder(string folderName) {
            var folder = _items.OfType<SolutionFolder>().SingleOrDefault(p => p.Name == folderName);
            if (folder == null) {
                folder = new SolutionFolder {
                    Guid = Guid.NewGuid(),
                    Name = folderName
                };
                _items.Add(folder);
            }
            return folder;
        }

        public void AddProject(Project project) {
            _items.Add(new SolutionProject {
                Guid = project.Guid,
                Name = project.Name,
                Project = project
            });
        }
    }

    public class SolutionProject : SolutionItem {
        public Project Project { get; set; }
    }
}