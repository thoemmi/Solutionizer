using System.Collections.Generic;
using Solutionizer.Models;

namespace Solutionizer.ViewModels {
    public class SolutionProject : SolutionItem {
        private readonly Project _project;

        public SolutionProject(Project project, SolutionFolder parent) : base(parent) {
            _project = project;
        }

        public string Filepath { get; set; }

        public IList<string> Configurations { get { return _project.Configurations; } }
    }
}