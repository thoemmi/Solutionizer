using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using Ookii.Dialogs.Wpf;
using Solutionizer.Commands;
using Solutionizer.Infrastructure;

namespace Solutionizer.ViewModels {
    public class MainViewModel : ViewModelBase {
        private readonly Services.Settings _settings = Services.Settings.Instance;
        private SolutionViewModel _solution = new SolutionViewModel(Services.Settings.Instance.RootPath);
        private readonly ICommand _onLoadedCommand;
        private readonly ICommand _onClosedCommand;
        private readonly ICommand _selectRootPathCommand;
        private readonly ICommand _launchCommand;
        private readonly ICommand _saveCommand;
        private readonly ICommand _newSolutionCommand;
        private readonly ICommand _toggleFlatModeCommand;
        private readonly ICommand _showSettingsCommand;
        private readonly ICommand _hideSettingsCommand;
        private bool _showSettings;

        public MainViewModel() {
            _onLoadedCommand = new FixedRelayCommand(OnLoaded);
            _onClosedCommand = new FixedRelayCommand(OnClosed);
            _selectRootPathCommand = new FixedRelayCommand(OnSelectRootPath);
            _launchCommand = new FixedRelayCommand(OnLaunch, () => Solution.SolutionHasItems);
            _saveCommand = new FixedRelayCommand(OnSave, () => Solution.SolutionHasItems);
            _newSolutionCommand = new FixedRelayCommand(() => Solution = new SolutionViewModel(_settings.RootPath));
            _toggleFlatModeCommand = new FixedRelayCommand(() => _settings.IsFlatMode = !_settings.IsFlatMode);
            _showSettingsCommand = new FixedRelayCommand(() => ShowSettings = true);
            _hideSettingsCommand = new FixedRelayCommand(() => ShowSettings = false);
        }

        private void OnSelectRootPath() {
            var dlg = new VistaFolderBrowserDialog {
                SelectedPath = _settings.RootPath
            };
            if (dlg.ShowDialog(Application.Current.MainWindow) == true) {
                _settings.RootPath = dlg.SelectedPath;
                Solution = new SolutionViewModel(dlg.SelectedPath);
            }
        }

        private void OnLoaded() {
        }

        private void OnClosed() {
            _settings.Save();
        }

        private void OnLaunch() {
            var newFilename = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyy-MM-dd_HHmmss")) + ".sln";
            new SaveSolutionCommand(newFilename, _settings.VisualStudioVersion, Solution).Execute();
            Process.Start(newFilename);
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void OnSave() {
            var dlg = new VistaSaveFileDialog {
                Filter = "Solution File (*.sln)|*.sln",
                AddExtension = true,
                DefaultExt = ".sln"
            };
            if (dlg.ShowDialog() == true) {
                new SaveSolutionCommand(dlg.FileName, _settings.VisualStudioVersion, Solution).Execute();
            }
        }

        public ICommand OnLoadedCommand {
            get { return _onLoadedCommand; }
        }

        public ICommand OnClosedCommand {
            get { return _onClosedCommand; }
        }

        public ICommand SelectRootPathCommand {
            get { return _selectRootPathCommand; }
        }

        public ICommand LaunchCommand {
            get { return _launchCommand; }
        }

        public ICommand SaveCommand {
            get { return _saveCommand; }
        }

        public ICommand NewSolutionCommand {
            get { return _newSolutionCommand; }
        }

        public ICommand ToggleFlatModeCommand {
            get { return _toggleFlatModeCommand; }
        }

        public Services.Settings Settings {
            get { return _settings; }
        }

        public SolutionViewModel Solution {
            get { return _solution; }
            set {
                if (_solution != value) {
                    _solution = value;
                    RaisePropertyChanged(() => Solution);
                }
            }
        }

        public bool ShowSettings {
            get { return _showSettings; }
            set {
                if (_showSettings != value) {
                    _showSettings = value;
                    RaisePropertyChanged(() => ShowSettings);
                }
            }
        }

        public ICommand ShowSettingsCommand {
            get { return _showSettingsCommand; }
        }

        public ICommand HideSettingsCommand {
            get { return _hideSettingsCommand; }
        }
    }
}