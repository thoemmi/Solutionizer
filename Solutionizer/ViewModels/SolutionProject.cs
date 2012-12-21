using System.Collections.Generic;

namespace Solutionizer.ViewModels {
    public class SolutionProject : SolutionItem {
        //public Project Project { get; set; }
        public SolutionProject(SolutionFolder parent) : base(parent) {
        }

        public string Filepath { get; set; }

        public IList<string> Configurations { get; set; }
    }
}