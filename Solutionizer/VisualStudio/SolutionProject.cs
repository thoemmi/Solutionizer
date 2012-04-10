namespace Solutionizer.VisualStudio {
    public class SolutionProject : SolutionItem {
        //public Project Project { get; set; }
        public SolutionProject(SolutionFolder parent) : base(parent) {
        }

        public string Filepath { get; set; }
    }
}