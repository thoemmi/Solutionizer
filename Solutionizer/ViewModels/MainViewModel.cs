using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using Ookii.Dialogs.Wpf;
using Solutionizer.Commands;

namespace Solutionizer.ViewModels {
    public class MainViewModel : ViewModelBase {
        private bool _isBusy;
        private string _busyMessage;
        private SolutionViewModel _solution = new SolutionViewModel();
        private readonly ProjectShelfViewModel _projectShelfViewModel;
        private readonly ICommand _onLoadedCommand;
        private readonly ICommand _selectRootPathCommand;
        private readonly ICommand _launchCommand;
        private readonly ICommand _saveCommand;

        public MainViewModel() {
            _projectShelfViewModel = new ProjectShelfViewModel(this);
            _onLoadedCommand = new FixedRelayCommand(OnLoaded);
            _selectRootPathCommand = new FixedRelayCommand(OnSelectRootPath);
            _launchCommand = new FixedRelayCommand(OnLaunch, () => _solution.SolutionHasItems);
            _saveCommand = new FixedRelayCommand(OnSave, () => _solution.SolutionHasItems);
        }

        private void OnSelectRootPath() {
            var dlg = new VistaFolderBrowserDialog {
                SelectedPath = _projectShelfViewModel.RootPath
            };
            if (dlg.ShowDialog(Application.Current.MainWindow) == true) {
                _projectShelfViewModel.RootPath = dlg.SelectedPath;
            }
        }

        private void OnLoaded() {
            _projectShelfViewModel.RootPath = @"d:\dev\xtplus\main\main";
        }

        private void OnLaunch() {
            var newFilename = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyy-MM-dd_HHmmss")) + ".sln";
            new SaveSolutionCommand(newFilename, _solution).Execute();
            System.Diagnostics.Process.Start(newFilename);
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

        public bool IsBusy {
            get { return _isBusy; }
            set {
                if (_isBusy != value) {
                    _isBusy = value;
                    RaisePropertyChanged(() => IsBusy);
                }
            }
        }

        public string BusyMessage {
            get { return _busyMessage; }
            set {
                if (_busyMessage != value) {
                    _busyMessage = value;
                    RaisePropertyChanged(() => BusyMessage);
                }
            }
        }

        public ProjectShelfViewModel ProjectShelf {
            get { return _projectShelfViewModel; }
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
    }
}