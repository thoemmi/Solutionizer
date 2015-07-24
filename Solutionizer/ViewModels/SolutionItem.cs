using System;
using TinyLittleMvvm;

namespace Solutionizer.ViewModels {
    public abstract class SolutionItem : PropertyChangedBase {
        private string _name;
        private readonly SolutionFolder _parent;

        protected SolutionItem(SolutionFolder parent) {
            _parent = parent;
        }

        public SolutionFolder Parent {
            get { return _parent; }
        }

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