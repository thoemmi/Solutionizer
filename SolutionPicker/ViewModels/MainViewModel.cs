using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using SolutionPicker.Scanner;

namespace SolutionPicker.ViewModels {
    public class MainViewModel : ViewModelBase {
        private bool _isBusy;
        private string _busyMessage;
        private string _rootPath = @"d:\dev\xtplus\main\main";
        private readonly ICommand _onLoadedCommand;
        private IList<DirectoryNode> _rootNodes;
        private SolutionViewModel _solution = new SolutionViewModel();

        public MainViewModel() {
            _onLoadedCommand = new RelayCommand(OnLoaded);
        }

        private void OnLoaded() {
            IsBusy = true;

            var worker = new BackgroundWorker();
            worker.DoWork += (o, ea) => {
                var scanner = new ProjectScanner();
                ea.Result = new[] {
                    scanner.Scan(RootPath)
                };
            };
            worker.RunWorkerCompleted += (o, ea) => {
                IsBusy = false;
                RootNodes = (IList<DirectoryNode>) ea.Result;
            };
            worker.RunWorkerAsync();
        }

        public ICommand OnLoadedCommand {
            get { return _onLoadedCommand; }
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

        public string RootPath {
            get { return _rootPath; }
            set {
                if (_rootPath != value) {
                    _rootPath = value;
                    RaisePropertyChanged(() => RootPath);
                }
            }
        }

        public IList<DirectoryNode> RootNodes {
            get { return _rootNodes; }
            set {
                if (_rootNodes != value) {
                    _rootNodes = value;
                    RaisePropertyChanged(() => RootNodes);
                }
            }
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