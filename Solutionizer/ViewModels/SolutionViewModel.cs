using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Solutionizer.Scanner;

namespace Solutionizer.ViewModels {
    public class SolutionViewModel : ViewModelBase {
        private readonly ObservableCollection<Project> _projects = new ObservableCollection<Project>();
        private readonly ObservableCollection<Project> _referencedProjects = new ObservableCollection<Project>();
        private readonly ICommand _dropCommand;

        private readonly Dictionary<string,Project> _knownProjectsByPath = new Dictionary<string, Project>(StringComparer.InvariantCultureIgnoreCase);

        private bool _isSccBound;

        public SolutionViewModel() {
            _dropCommand = new RelayCommand<object>(OnDrop, obj => obj is FileNode);
        }

        private void OnDrop(object node) {
            var project = Project.Load(((FileNode)node).Path);
            AddProject(project);
        }

        public void AddProject(Project project) {
            _isSccBound |= project.IsSccBound;

            if (!_knownProjectsByPath.ContainsKey(project.Filepath)) {
                _knownProjectsByPath.Add(project.Filepath, project);
            }

            if (_projects.Contains(project)) {
                return;
            }
            _projects.Add(project);
            if (_referencedProjects.Contains(project)) {
                _referencedProjects.Remove(project);
            }
            AddReferencedProjects(project, 6);
        }

        private void AddReferencedProjects(Project project, int depth) {
            foreach (var projectReference in project.ProjectReferences) {
                Project referencedProject;
                if (!_knownProjectsByPath.TryGetValue(projectReference, out referencedProject)) {
                    referencedProject = Project.Load(projectReference);
                    _knownProjectsByPath.Add(referencedProject.Filepath, referencedProject);
                }
                if (!_referencedProjects.Contains(referencedProject) && !_projects.Contains(referencedProject)) {
                    _referencedProjects.Add(referencedProject);
                    if (depth > 0) {
                        AddReferencedProjects(referencedProject, depth - 1);
                    }
                }
            }
        }

        public ObservableCollection<Project> Projects {
            get { return _projects; }
        }

        public ObservableCollection<Project> ReferencedProjects {
            get { return _referencedProjects; }
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
}