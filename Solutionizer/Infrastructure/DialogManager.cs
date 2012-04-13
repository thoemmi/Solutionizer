using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using Solutionizer.Extensions;

namespace Solutionizer.Infrastructure {
    [Export]
    public class DialogManager {
        public void ShowDialog(object model) {
            var window = GetAppWindow();
            var panel = window.FindVisualChild<Panel>("DialogPlaceHolder");

            var uiElement = ViewLocator.LocateForModel(model, null, null);

            var screen = model as Screen;
            if (screen != null) {
            }

            var dialogView = new DialogView();
            dialogView.Content = uiElement;
            panel.Children.Add(dialogView);
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