using Caliburn.Micro;

namespace Solutionizer.Infrastructure {
    public interface IDialogManager {
        void ShowDialog(IScreen dialogModel);
    }
}