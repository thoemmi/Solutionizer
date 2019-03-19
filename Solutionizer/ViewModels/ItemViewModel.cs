using TinyLittleMvvm;

namespace Solutionizer.ViewModels {
    public abstract class ItemViewModel : PropertyChangedBase {
        protected ItemViewModel(DirectoryViewModel parent) {
            Parent = parent;
        }

        public DirectoryViewModel Parent { get; }

        public abstract string Name { get; }

        public abstract string Path { get; }

        public abstract void Filter(string filter);
    }
}