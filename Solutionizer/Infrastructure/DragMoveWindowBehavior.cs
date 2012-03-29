using System.Windows;
using System.Linq;
using System.Windows.Interactivity;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;

namespace Solutionizer.Infrastructure {
    public class DragMoveWindowBehavior : Behavior<FrameworkElement> {
        private Window _window;

        protected override void OnAttached() {
            base.OnAttached();

            if (!AssociatedObject.IsLoaded) {
                RoutedEventHandler eventHandler = null;
                eventHandler = (sender, args) => {
                    AssociatedObject.Loaded -= eventHandler;
                    InternalAttach();
                };
                AssociatedObject.Loaded += eventHandler;
            } else {
                InternalAttach();
            }
        }

        private void InternalAttach() {
            _window = Ancestors(AssociatedObject)
                .OfType<Window>()
                .FirstOrDefault();

            if (_window == null) {
                return;
            }

            AssociatedObject.MouseDown += AssociatedObjectOnMouseDown;
            AssociatedObject.MouseMove += AssociatedObjectOnMouseMove;
        }

        private void AssociatedObjectOnMouseDown(object sender, MouseButtonEventArgs e) {
            if (e.RightButton != MouseButtonState.Pressed && e.MiddleButton != MouseButtonState.Pressed &&
                e.LeftButton == MouseButtonState.Pressed) {
                _window.DragMove();
            }

            if (e.ClickCount == 2) {
                if (_window.WindowState == WindowState.Maximized) {
                    _window.WindowState = WindowState.Normal;
                } else {
                    _window.WindowState = WindowState.Maximized;
                }
            }
        }

        private void AssociatedObjectOnMouseMove(object sender, MouseEventArgs e) {
            if (e.RightButton != MouseButtonState.Pressed && e.MiddleButton != MouseButtonState.Pressed
                && e.LeftButton == MouseButtonState.Pressed && _window.WindowState == WindowState.Maximized) {
                // Calcualting correct left coordinate for multi-screen system.
                double mouseX = _window.PointToScreen(Mouse.GetPosition(_window)).X;
                double width = _window.RestoreBounds.Width;
                double left = mouseX - width / 2;

                // Aligning window's position to fit the screen.
                double virtualScreenWidth = SystemParameters.VirtualScreenWidth;
                left = left < 0 ? 0 : left;
                left = left + width > virtualScreenWidth ? virtualScreenWidth - width : left;

                _window.Top = 0;
                _window.Left = left;

                // Restore window to normal state.
                _window.WindowState = WindowState.Normal;

                _window.DragMove();
            }
        }

        protected override void OnDetaching() {
            if (_window == null) {
                return;
            }
            _window = null;
            AssociatedObject.MouseDown -= AssociatedObjectOnMouseDown;
            AssociatedObject.MouseMove -= AssociatedObjectOnMouseMove;
            base.OnDetaching();
        }

        private static IEnumerable<DependencyObject> Ancestors(DependencyObject obj) {
            var parent = VisualTreeHelper.GetParent(obj);
            while (parent != null) {
                yield return parent;
                obj = parent;
                parent = VisualTreeHelper.GetParent(obj);
            }
        }
    }
}