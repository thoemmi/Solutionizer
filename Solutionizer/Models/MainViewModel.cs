using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using Ookii.Dialogs.Wpf;
using Solutionizer.Commands;
using Solutionizer.Infrastructure;

namespace Solutionizer.Models {
    public class MainViewModel : ViewModelBase {
        private SolutionViewModel _solution = new SolutionViewModel();
        private readonly ICommand _onLoadedCommand;
        private readonly ICommand _selectRootPathCommand;
        private readonly ICommand _launchCommand;
        private readonly ICommand _saveCommand;
        private readonly ICommand _toggleFlatModeCommand;
        private readonly ICommand _toggleHideRootNodeCommand;
        private bool _isFlatMode;
        private bool _hideRootNode;
        private string _rootPath;

        public MainViewModel() {
            _onLoadedCommand = new FixedRelayCommand(OnLoaded);
            _selectRootPathCommand = new FixedRelayCommand(OnSelectRootPath);
            _launchCommand = new FixedRelayCommand(OnLaunch, () => _solution.SolutionHasItems);
            _saveCommand = new FixedRelayCommand(OnSave, () => _solution.SolutionHasItems);
            _toggleFlatModeCommand = new FixedRelayCommand(() => IsFlatMode = !IsFlatMode);
            _toggleHideRootNodeCommand = new FixedRelayCommand(() => HideRootNode = !HideRootNode, () => !IsFlatMode);
        }

        private void OnSelectRootPath() {
            var dlg = new VistaFolderBrowserDialog {
                SelectedPath = RootPath
            };
            if (dlg.ShowDialog(Application.Current.MainWindow) == true) {
                RootPath = dlg.SelectedPath;
                _solution.CreateSolution(dlg.SelectedPath);
            }
        }

        private void OnLoaded() {
            RootPath = @"d:\dev\xtplus\main\main";
            _solution.CreateSolution(RootPath);
        }

        private void OnLaunch() {
            var newFilename = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyy-MM-dd_HHmmss")) + ".sln";
            new SaveSolutionCommand(newFilename, _solution).Execute();
            Process.Start(newFilename);
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void OnSave() {
            var dlg = new VistaSaveFileDialog {
                Filter = "Solution File (*.sln)|*.sln"
            };
            if (dlg.ShowDialog() == true) {
                new SaveSolutionCommand(dlg.FileName, _solution).Execute();
            }
        }

        public ICommand OnLoadedCommand {
            get { return _onLoadedCommand; }
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

        public ICommand ToggleFlatModeCommand {
            get { return _toggleFlatModeCommand; }
        }

        public ICommand ToggleHideRootNodeCommand {
            get { return _toggleHideRootNodeCommand; }
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

        public bool IsFlatMode {
            get { return _isFlatMode; }
            set {
                if (_isFlatMode != value) {
                    _isFlatMode = value;
                    RaisePropertyChanged(() => IsFlatMode);
                }
            }
        }

        public bool HideRootNode {
            get { return _hideRootNode; }
            set {
                if (_hideRootNode != value) {
                    _hideRootNode = value;
                    RaisePropertyChanged(() => HideRootNode);
                }
            }
        }

        public string RootPath {
            get { return _rootPath; }
            set {
                if (_rootPath != value) {
                    _rootPath = value;
                    RaisePropertyChanged(() => RootPath);
                }
            }
        }
    }
}