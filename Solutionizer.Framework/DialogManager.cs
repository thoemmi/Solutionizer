using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Solutionizer.Framework {
    public interface IDialogManager {
        Task<TResult> ShowDialog<TResult>(DialogViewModel<TResult> viewModel);
    }

    public class DialogManager : ObservableCollection<object>, IDialogManager {
        public Task<TResult> ShowDialog<TResult>(DialogViewModel<TResult> viewModel) {
            Add(viewModel);
            return viewModel.Task;
        }
    }
}