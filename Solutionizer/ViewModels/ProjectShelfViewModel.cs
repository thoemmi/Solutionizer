using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight;
using Solutionizer.Extensions;
using Solutionizer.Scanner;

namespace Solutionizer.ViewModels {
    public class ProjectShelfViewModel : ViewModelBase {
        private readonly MainViewModel _mainViewModel;
        private string _rootPath;
        private DirectoryNode _rootNode;
        private IList _rootNodes;
        private bool _isFlatMode;
        private bool _hideRootNode;

        public ProjectShelfViewModel(MainViewModel mainViewModel) {
            _mainViewModel = mainViewModel;
        }

        private void RefreshFileTree() {
            _mainViewModel.IsBusy = true;

            var worker = new BackgroundWorker();
            worker.DoWork += (o, ea) => {
                ea.Result = ProjectScanner.Scan(RootPath);
            };
            worker.RunWorkerCompleted += (o, ea) => {
                _mainViewModel.IsBusy = false;
                _rootNode = (DirectoryNode) ea.Result;
                TransformNodes();
            };
            worker.RunWorkerAsync();
        }

        private void TransformNodes() {
            if (_rootNode == null) {
                RootNodes = new object[0];
                return;
            }

            DirectoryNode root;
            if (_isFlatMode) {
                root = new DirectoryNode {
                    Name = _rootNode.Name,
                    Path = _rootNode.Path,
                    Files = new[] {
                        _rootNode
                    }.Flatten(d => d.Files, d => d.Subdirectories).ToList()
                };
                root.Files.Sort((f1, f2) => String.Compare(f1.Name, f2.Name, StringComparison.InvariantCultureIgnoreCase));
            } else {
                root = _rootNode;
            }

            if (_hideRootNode) {
                RootNodes = root.Subdirectories.Cast<object>().Concat(root.Files).ToList();
            }else {
                RootNodes = new[] { root };
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

        public bool HideRootNode {
            get { return _hideRootNode; }
            set {
                if (_hideRootNode != value) {
                    _hideRootNode = value;
                    TransformNodes();
                    RaisePropertyChanged(() => HideRootNode);
                }
            }
        }
    }
}