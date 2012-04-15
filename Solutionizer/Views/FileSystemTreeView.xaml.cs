using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Solutionizer.Commands;
using Solutionizer.Infrastructure;
using Solutionizer.Models;
using Solutionizer.ViewModels;

namespace Solutionizer.Views {
    /// <summary>
    /// Interaction logic for FileSystemTreeView.xaml
    /// </summary>
    public partial class FileSystemTreeView {
        private ProjectFolder _rootNode;

        public FileSystemTreeView() {
            InitializeComponent();
            Loaded += (sender, args) => {
                if (ScanOnStartup) {
                    RefreshFileTree();
                }
            };
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

        public static readonly DependencyProperty RootNodesProperty =
            DependencyProperty.Register("RootNodes", typeof (IList), typeof (FileSystemTreeView), new PropertyMetadata(default(IList)));

        public IList RootNodes {
            get { return (IList) GetValue(RootNodesProperty); }
            set { SetValue(RootNodesProperty, value); }
        }

        public static readonly DependencyProperty ScanOnStartupProperty =
            DependencyProperty.Register("ScanOnStartup", typeof (bool), typeof (FileSystemTreeView), new PropertyMetadata(default(bool)));

        public bool ScanOnStartup {
            get { return (bool) GetValue(ScanOnStartupProperty); }
            set { SetValue(ScanOnStartupProperty, value); }
        }

        private void RefreshFileTree() {
            var rootPath = RootPath;
            //CommandExecutor
            //    .ExecuteAsync("Scanning projects", () =>  Solutionizer.Infrastructure.ProjectRepository.Instance.GetProjects(rootPath))
            //    .ContinueWith(result => {
            //        _rootNode = result.Result;
            //        TransformNodes();
            //    }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void TransformNodes() {
            if (_rootNode == null) {
                RootNodes = new object[0];
                return;
            }
            RootNodes = CreateDirectoryViewModel(_rootNode, null).Children.ToList();
        }

        public DirectoryViewModel CreateDirectoryViewModel(ProjectFolder projectFolder, DirectoryViewModel parent) {
            var viewModel = new DirectoryViewModel(projectFolder, parent);
            foreach (var folder in projectFolder.Folders) {
                viewModel.Directories.Add(CreateDirectoryViewModel(folder, viewModel));
            }
            foreach (var project in projectFolder.Projects) {
                viewModel.Projects.Add(CreateProjectViewModel(project, viewModel));
            }
            return viewModel;
        }

        private ProjectViewModel CreateProjectViewModel(Project project, DirectoryViewModel parent) {
            return new ProjectViewModel(project, parent);
        }
    }
}