using System;
using TinyLittleMvvm;

namespace Solutionizer.ViewModels {
    public abstract class SolutionItem : PropertyChangedBase {
        private string _name;

        protected SolutionItem(SolutionFolder parent) {
            Parent = parent;
        }

        public SolutionFolder Parent { get; }

        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    _name = value;
                    NotifyOfPropertyChange(() => Name);
                }
            }
        }

        public Guid Guid { get; set; }
    }
}