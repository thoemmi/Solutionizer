using System;
using GalaSoft.MvvmLight;

namespace Solutionizer.VisualStudio {
    public abstract class SolutionItem : ViewModelBase {
        private string _name;

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