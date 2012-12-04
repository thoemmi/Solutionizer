using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Solutionizer.Models;
using Solutionizer.Services;

namespace Solutionizer.ViewModels {
    public class DirectoryViewModel : ItemViewModel {
        private readonly ISettings _settings;

        private readonly ProjectFolder _projectFolder;
        private readonly List<DirectoryViewModel> _directories = new List<DirectoryViewModel>();
        private readonly List<ProjectViewModel> _projects = new List<ProjectViewModel>();
        private readonly Func<ItemViewModel, bool> _filter;		

        public DirectoryViewModel(ISettings settings, DirectoryViewModel parent, ProjectFolder projectFolder, Func<ItemViewModel, bool> filter) : base(parent) {
            _settings = settings;
            _projectFolder = projectFolder;
            _filter = filter;
        }

        public override string Name {
            get { return _projectFolder.Name; }
        }

        public override string Path {
            get { return _projectFolder.FullPath; }
        }

        public ProjectFolder ProjectFolder {
            get { return _projectFolder; }
        }

        public List<DirectoryViewModel> Directories {
            get { return _directories; }
        }

        public List<ProjectViewModel> Projects {
            get { return _projects; }
        }

        public ICollectionView Children {
            get {
                var children = _directories
                    .Where(d => !_settings.SimplifyProjectTree || d.Children.Cast<ItemViewModel>().Any())
                    .Cast<ItemViewModel>()
                    .OrderBy(d => d.Name)
                    .Concat(_projects.OrderBy(p => p.Name));

                var childrenView = CollectionViewSource.GetDefaultView(children);
                childrenView.Filter = item => !(item is ProjectViewModel) || _filter((ItemViewModel)item);
                return childrenView;
            }
        }
    }
}