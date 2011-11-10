using System.Collections.Generic;
using System.ComponentModel;
using Solutionizer.Scanner;

namespace Solutionizer.ViewModels {
    public class ProjectShelfViewModel : ViewModelBase {
        private readonly MainViewModel _mainViewModel;
        private string _rootPath;
        private IList<DirectoryNode> _rootNodes;

        public ProjectShelfViewModel(MainViewModel mainViewModel) {
            _mainViewModel = mainViewModel;
        }

        private void RefreshFileTree() {
            _mainViewModel.IsBusy = true;

            var worker = new BackgroundWorker();
            worker.DoWork += (o, ea) => {
                var scanner = new ProjectScanner();
                ea.Result = new[] {
                    scanner.Scan(RootPath)
                };
            };
            worker.RunWorkerCompleted += (o, ea) => {
                _mainViewModel.IsBusy = false;
                RootNodes = (IList<DirectoryNode>) ea.Result;
            };
            worker.RunWorkerAsync();
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

        public string RootPath {
            get { return _rootPath; }
            set {
                if (_rootPath != value) {
                    _rootPath = value;
                    RaisePropertyChanged(() => RootPath);
                    RefreshFileTree();
                }
            }
        }
    }
}