using GalaSoft.MvvmLight;

namespace Solutionizer.Models {
    public class Settings : ViewModelBase {
        private bool _isFlatMode;
        private bool _hideRootNode;
        private string _rootPath;


        public bool IsFlatMode {
            get { return _isFlatMode; }
            set {
                if (_isFlatMode != value) {
                    _isFlatMode = value;
                    RaisePropertyChanged(() => IsFlatMode);
                }
            }
        }

        public bool HideRootNode {
            get { return _hideRootNode; }
            set {
                if (_hideRootNode != value) {
                    _hideRootNode = value;
                    RaisePropertyChanged(() => HideRootNode);
                }
            }
        }

        public string RootPath {
            get { return _rootPath; }
            set {
                if (_rootPath != value) {
                    _rootPath = value;
                    RaisePropertyChanged(() => RootPath);
                }
            }
        }
    }
}