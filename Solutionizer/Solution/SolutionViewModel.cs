using System.Windows;
using System.Windows.Input;
using Solutionizer.Infrastructure;
using Solutionizer.ViewModels;

namespace Solutionizer.Solution {
    public class SolutionViewModel {
        private readonly ICommand _dropCommand;

        public SolutionViewModel() {
            _dropCommand = new FixedRelayCommand<object>(OnDrop, obj => obj is ProjectViewModel);
        }

        private void OnDrop(object node)
        {
            var project = ((ProjectViewModel)node).Project;
            project.Load();
            //AddProject(project);
        }

        public ICommand DropCommand {
            get { return _dropCommand; }
        }
    }
}