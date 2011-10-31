using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using SolutionPicker.Scanner;

namespace SolutionPicker.ViewModels {
    public class SolutionViewModel : ViewModelBase {
        private readonly ObservableCollection<Project> _projects = new ObservableCollection<Project>();
        private readonly ObservableCollection<Project> _referencedProjects = new ObservableCollection<Project>();
        private readonly ICommand _dropCommand;

        private bool _isSccBound;

        public SolutionViewModel() {
            _dropCommand = new RelayCommand<FileNode>(OnDrop);
        }

        private void OnDrop(FileNode node) {
            var project = Project.Load(node.Path);
            AddProject(project);
        }

        public void AddProject(Project project) {
            _isSccBound |= project.IsSccBound;

            if (_projects.Contains(project)) {
                return;
            }
            _projects.Add(project);
            if (_referencedProjects.Contains(project)) {
                _referencedProjects.Remove(project);
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