using System.Windows.Input;
using Solutionizer.Framework;

namespace Solutionizer.ViewModels {
    public class AboutViewModel : DialogViewModel, IWithTitle {
        private readonly ICommand _closeCommand;

        public AboutViewModel() {
            _closeCommand = new RelayCommand(Close);
        }

        public string Title {
            get { return "About Solutionizer"; }
        }

        public ICommand CloseCommand {
            get { return _closeCommand; }
        }
    }
}