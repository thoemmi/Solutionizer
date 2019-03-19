using System;
using System.Linq;
using Solutionizer.Models;

namespace Solutionizer.ViewModels {
    public class ProjectViewModel : ItemViewModel {
        private bool _isVisible = true;

        public ProjectViewModel(DirectoryViewModel parent, Project project) : base(parent) {
            Project = project;
        }

        public Project Project { get; }

        public override string Name => Project.Name;

        public override string Path => Project.Filepath;

        public bool IsVisible {
            get { return _isVisible; }
            private set {
                if (_isVisible != value) {
                    _isVisible = value;
                    NotifyOfPropertyChange(() => IsVisible);
                }
            }
        }

        public override void Filter(string filter) {
            IsVisible = String.IsNullOrEmpty(filter) || Name.ToUpperInvariant().Contains(filter.ToUpperInvariant());
        }

        public bool HasIssues => HasErrors || HasBrokenProjectReferences;

        public bool HasErrors => Project.Errors.Any();

        public bool HasBrokenProjectReferences => Project.BrokenProjectReferences.Any();

        public string BrokenProjectReferences => String.Join(",\n", Project.BrokenProjectReferences);

        public string Errors => String.Join(",\n", Project.Errors);
    }
}