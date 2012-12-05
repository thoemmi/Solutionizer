using Caliburn.Micro;

namespace Solutionizer.ViewModels {
    public abstract class ItemViewModel : PropertyChangedBase {
        private readonly DirectoryViewModel _parent;

        protected ItemViewModel(DirectoryViewModel parent) {
            _parent = parent;
        }

        public DirectoryViewModel Parent {
            get { return _parent; }
        }

        public abstract string Name { get; }

        public abstract string Path { get; }

        public abstract void Filter(string filter);
    }
}