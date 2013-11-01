using System.Collections.ObjectModel;
using System.Windows;

namespace Solutionizer.Framework {
    public interface IDialogManager {
        void ShowDialog(object viewModel);
    }

    public class DialogManager : ObservableCollection<object>, IDialogManager {
        public void ShowDialog(object viewModel) {
            Add(viewModel);
        }
    }
}