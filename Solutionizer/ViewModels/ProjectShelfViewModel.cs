using System;
using System.Collections;
using System.Linq;
using Solutionizer.Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using Solutionizer.Scanner;

namespace Solutionizer.ViewModels {
    public class ProjectShelfViewModel : ViewModelBase {
        private readonly MainViewModel _mainViewModel;
        private string _rootPath;
        private DirectoryNode _rootNode;
        private IList _rootNodes;
        private bool _isFlatMode;

        public ProjectShelfViewModel(MainViewModel mainViewModel) {
            _mainViewModel = mainViewModel;
        }

        private void RefreshFileTree() {
            _mainViewModel.IsBusy = true;

            var worker = new BackgroundWorker();
            worker.DoWork += (o, ea) => {
                var scanner = new ProjectScanner();
                ea.Result = scanner.Scan(RootPath);
            };
            worker.RunWorkerCompleted += (o, ea) => {
                _mainViewModel.IsBusy = false;
                _rootNode = (DirectoryNode)ea.Result;
                TransformNodes();
            };
            worker.RunWorkerAsync();
        }

        private void TransformNodes() {
            if (_rootNode == null) {
                RootNodes = new object[0];
                return;
            }

            if (_isFlatMode) {
                var root = new DirectoryNode {
                    Name = _rootNode.Name,
                    Path = _rootNode.Path,
                    Files = new[] { _rootNode }.Flatten(d => d.Files, d => d.Subdirectories).ToList()
                };
                root.Files.Sort((f1, f2) => String.Compare(f1.Name, f2.Name, StringComparison.InvariantCultureIgnoreCase));

                RootNodes = new[] { root };
            } else {
                RootNodes = new[] { _rootNode };
            }
        }

        public IList RootNodes {
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

        public bool IsFlatMode {
            get { return _isFlatMode; }
            set {
                if (_isFlatMode != value) {
                    _isFlatMode = value;
                    TransformNodes();
                    RaisePropertyChanged(() => IsFlatMode);
                }
            }
        }
    }
}