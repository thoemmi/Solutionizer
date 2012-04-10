using System;
using GalaSoft.MvvmLight;

namespace Solutionizer.VisualStudio {
    public abstract class SolutionItem : ViewModelBase {
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
                    RaisePropertyChanged(() => Name);
                }
            }
        }

        public Guid Guid { get; set; }
    }
}