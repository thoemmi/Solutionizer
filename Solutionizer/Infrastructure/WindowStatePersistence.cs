using System.Windows;

namespace Solutionizer.Infrastructure {
    public class WindowStatePersistence {
        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.RegisterAttached("Settings", typeof (Services.Settings), typeof (WindowStatePersistence), new PropertyMetadata(null, OnChanged));

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var window = d as Window;
            if (window == null) {
                return;
            }

            var setting = e.NewValue as Services.Settings;
            if (setting != null) {
                if (setting.WindowSettings != null) {
                    var top = setting.WindowSettings.Top;
                    var left = setting.WindowSettings.Left;
                    var width = setting.WindowSettings.Width;
                    var height = setting.WindowSettings.Height;

                    // right
                    var delta = (left + width) - (SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth);
                    if (delta > 0) {
                        left -= delta;
                    }
                    // left
                    delta = left - SystemParameters.VirtualScreenLeft;
                    if (delta < 0) {
                        left -= delta;
                    }
                    // width
                    delta = (left + width) - (SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth);
                    if (delta > 0) {
                        width -= delta;
                    }
                    // bottom
                    delta = (top + height) - (SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight);
                    if (delta > 0) {
                        top -= delta;
                    }
                    // left
                    delta = top - SystemParameters.VirtualScreenTop;
                    if (delta < 0) {
                        top -= delta;
                    }
                    // width
                    delta = (top + height) - (SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight);
                    if (delta > 0) {
                        height -= delta;
                    }

                    window.Top = top;
                    window.Left = left;
                    window.Width = width;
                    window.Height = height;

                    if (setting.WindowSettings.Maximized) {
                        window.WindowState = WindowState.Maximized;
                    }
                }
                window.Closing += (sender, args) => {
                    if (window.WindowState == WindowState.Maximized) {
                        setting.WindowSettings = new WindowSettings {
                            Top = window.RestoreBounds.Top,
                            Left = window.RestoreBounds.Left,
                            Width = window.RestoreBounds.Width,
                            Height = window.RestoreBounds.Height,
                            Maximized = true
                        };
                    } else {
                        setting.WindowSettings = new WindowSettings {
                            Top = window.Top,
                            Left = window.Left,
                            Width = window.Width,
                            Height = window.Height,
                            Maximized = false
                        };
                    }
                };
            }
        }

        public static void SetSettings(Window element, Services.Settings value) {
            element.SetValue(SettingsProperty, value);
        }

        public static Services.Settings GetSettings(Window element) {
            return (Services.Settings)element.GetValue(SettingsProperty);
        }
    }
}