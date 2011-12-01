using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Solutionizer.Commands;
using Solutionizer.Extensions;
using Solutionizer.Scanner;

namespace Solutionizer.Controls {
    /// <summary>
    /// Interaction logic for FileSystemTreeView.xaml
    /// </summary>
    public partial class FileSystemTreeView : UserControl {
        private DirectoryNode _rootNode;

        public FileSystemTreeView() {
            InitializeComponent();
            Loaded += (sender, args) => RefreshFileTree();
        }

        public static readonly DependencyProperty RootPathProperty =
            DependencyProperty.Register(
                "RootPath", 
                typeof (string), 
                typeof (FileSystemTreeView),
                new PropertyMetadata(default(string), (o, args) => ((FileSystemTreeView) o).RefreshFileTree()));

        public string RootPath {
            get { return (string) GetValue(RootPathProperty); }
            set { SetValue(RootPathProperty, value); }
        }

        public static readonly DependencyProperty IsFlatModeProperty =
            DependencyProperty.Register(
                "IsFlatMode", 
                typeof(bool), 
                typeof(FileSystemTreeView), 
                new PropertyMetadata(default(bool), (o, args) => ((FileSystemTreeView)o).TransformNodes()));

        public bool IsFlatMode {
            get { return (bool) GetValue(IsFlatModeProperty); }
            set { SetValue(IsFlatModeProperty, value); }
        }

        public static readonly DependencyProperty HideRootNodeProperty =
            DependencyProperty.Register(
                "HideRootNode", 
                typeof (bool), 
                typeof (FileSystemTreeView),
                new PropertyMetadata(default(bool), (o, args) => ((FileSystemTreeView)o).TransformNodes()));

        public bool HideRootNode {
            get { return (bool) GetValue(HideRootNodeProperty); }
            set { SetValue(HideRootNodeProperty, value); }
        }

        public static readonly DependencyProperty RootNodesProperty =
            DependencyProperty.Register("RootNodes", typeof (IList), typeof (FileSystemTreeView), new PropertyMetadata(default(IList)));

        public IList RootNodes {
            get { return (IList) GetValue(RootNodesProperty); }
            set { SetValue(RootNodesProperty, value); }
        }

        private void RefreshFileTree() {
            var rootPath = RootPath;
            CommandExecutor.ExecuteAsync("Scanning projects", () => ProjectScanner.Scan(rootPath), result => {
                _rootNode = result;
                TransformNodes();
            });
        }

        private void TransformNodes() {
            if (_rootNode == null) {
                RootNodes = new object[0];
                return;
            }

            DirectoryNode root;
            if (IsFlatMode) {
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

            if (HideRootNode || IsFlatMode) {
                RootNodes = root.Subdirectories.Cast<object>().Concat(root.Files).ToList();
            } else {
                RootNodes = new[] {
                    root
                };
            }
        }
    }
}