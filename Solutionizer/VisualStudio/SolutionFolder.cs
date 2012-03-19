using System;
using System.Collections.ObjectModel;
using System.Linq;
using Solutionizer.Infrastructure;
using Solutionizer.ViewModels;

namespace Solutionizer.VisualStudio {
    public class SolutionFolder : SolutionItem {
        private readonly SortedObservableCollection<SolutionItem> _items =
            new SortedObservableCollection<SolutionItem>(new SolutionItemComparer());

        public ObservableCollection<SolutionItem> Items {
            get { return _items; }
        }

        public bool ContainsProject(ProjectViewModel projectViewModel) {
            return _items.OfType<SolutionProject>().Any(p => p.Guid == projectViewModel.Guid);
        }

        public SolutionFolder GetOrCreateSubfolder(string folderName) {
            var folder = _items.OfType<SolutionFolder>().SingleOrDefault(p => p.Name == folderName);
            if (folder == null) {
                folder = new SolutionFolder {
                    Guid = Guid.NewGuid(),
                    Name = folderName
                };
                _items.Add(folder);
            }
            return folder;
        }

        public void AddProject(ProjectViewModel projectViewModel) {
            _items.Add(new SolutionProject {
                Guid = projectViewModel.Guid,
                Name = projectViewModel.Name,
                Filepath = projectViewModel.Filepath
            });
        }
    }
}