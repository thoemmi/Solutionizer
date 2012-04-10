using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Solutionizer.Infrastructure;
using Solutionizer.Models;
using Solutionizer.Services;
using Solutionizer.ViewModels;
using Solutionizer.VisualStudio;

namespace Solutionizer.Solution {
    public class SolutionViewModel {
        private readonly ICommand _dropCommand;
        private bool _isSccBound;
        private readonly SolutionFolder _solutionRoot = new SolutionFolder();

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

            if (Settings.Instance.IncludeReferencedProjects) {
                AddReferencedProjects(project, Settings.Instance.ReferenceTreeDepth);
            }
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
            return _solutionRoot.GetOrCreateSubfolder(Settings.Instance.ReferenceFolderName);
        }
    }
}