using System.Windows;
using Solutionizer.Models;

namespace Solutionizer.Infrastructure {
    public class WindowStatePersistence {
        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.RegisterAttached("Settings", typeof (Settings), typeof (WindowStatePersistence), new PropertyMetadata(default(Settings), OnChanged));

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var window = d as Window;
            if (window == null) {
                return;
            }

            var setting = e.NewValue as Settings;
            if (setting != null && setting.WindowSettings != null) {
                window.Top = setting.WindowSettings.Top;
                window.Left = setting.WindowSettings.Left;
                window.Width = setting.WindowSettings.Width;
                window.Height = setting.WindowSettings.Height;
                if (setting.WindowSettings.Maximized) {
                    window.WindowState = WindowState.Maximized;
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

        public static void SetSettings(Window element, Settings value) {
            element.SetValue(SettingsProperty, value);
        }

        public static Settings GetSettings(Window element) {
            return (Settings) element.GetValue(SettingsProperty);
        }
    }
}