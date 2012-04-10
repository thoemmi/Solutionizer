using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;

namespace Solutionizer.Infrastructure {
    [Export]
    public class DialogManager {
        public void ShowDialog(object model) {
            var window = GetAppWindow();
        }

        protected virtual Window GetAppWindow() {
            if (Application.Current == null) {
                return null;
            }
            var window = (Application.Current.Windows).OfType<Window>().Where((x => x.IsActive)).FirstOrDefault() ??
                            Application.Current.MainWindow;
            return window;
        }
    }
}