using System.Collections.Generic;
using Solutionizer.Models;

namespace Solutionizer.ViewModels {
    public class SolutionProject : SolutionItem {
        public SolutionProject(Project project, SolutionFolder parent) : base(parent) {
            Guid = project.Guid;
            Name = project.Name;
            Filepath = project.Filepath;
            Configurations = project.Configurations;
        }

        public List<string> Configurations { get; private set; }

        public string Filepath { get; private set; }
    }
}