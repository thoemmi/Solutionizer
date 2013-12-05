using System.Windows;

namespace Solutionizer.Framework {
    public class WindowManager {
        public Window ShowWindow<TViewModel>() {
            var view = (Window)ViewLocator.GetViewForViewModel<TViewModel>();
            view.Show();
            return view;
        }
    }
}