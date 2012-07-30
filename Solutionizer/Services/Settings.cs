using System;
using System.IO;
using Caliburn.Micro;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Solutionizer.Services {
    public class Settings : PropertyChangedBase, ISettings {
        private bool _scanOnStartup = true;
        private bool _isFlatMode;
        private bool _isDirty;
        private WindowSettings _windowSettings;
        private bool _includeReferencedProjects = true;
        private int _referenceTreeDepth = 6;
        private bool _simplifyProjectTree;
        private Uri _tfsName;
        private VisualStudioVersion _visualStudioVersion = VisualStudioVersion.VS2010;
        private string _referenceFolderName = "_References";

        private string _rootPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Visual Studio 2010",
            "Projects");

        public bool IsFlatMode {
            get { return _isFlatMode; }
            set {
                if (_isFlatMode != value) {
                    _isFlatMode = value;
                    NotifyOfPropertyChange(() => IsFlatMode);
                    IsDirty = true;
                }
            }
        }

        public bool SimplifyProjectTree {
            get { return _simplifyProjectTree; }
            set {
                if (_simplifyProjectTree != value) {
                    _simplifyProjectTree = value;
                    NotifyOfPropertyChange(() => SimplifyProjectTree);
                    IsDirty = true;
                }
            }
        }

        public string RootPath {
            get { return _rootPath; }
            set {
                if (_rootPath != value) {
                    _rootPath = value;
                    NotifyOfPropertyChange(() => RootPath);
                    IsDirty = true;
                }
            }
        }

        [JsonIgnore]
        public bool IsDirty {
            get { return _isDirty; }
            set {
                if (_isDirty != value) {
                    _isDirty = value;
                    NotifyOfPropertyChange(() => IsDirty);
                }
            }
        }

        public bool ScanOnStartup {
            get { return _scanOnStartup; }
            set {
                if (_scanOnStartup != value) {
                    _scanOnStartup = value;
                    NotifyOfPropertyChange(() => ScanOnStartup);
                    IsDirty = true;
                }
            }
        }

        public WindowSettings WindowSettings {
            get { return _windowSettings; }
            set {
                if (_windowSettings != value) {
                    _windowSettings = value;
                    NotifyOfPropertyChange(() => WindowSettings);
                    IsDirty = true;
                }
            }
        }

        public bool IncludeReferencedProjects {
            get { return _includeReferencedProjects; }
            set {
                if (_includeReferencedProjects != value) {
                    _includeReferencedProjects = value;
                    NotifyOfPropertyChange(() => IncludeReferencedProjects);
                    IsDirty = true;
                }
            }
        }

        public int ReferenceTreeDepth {
            get { return _referenceTreeDepth; }
            set {
                if (_referenceTreeDepth != value) {
                    _referenceTreeDepth = value;
                    NotifyOfPropertyChange(() => ReferenceTreeDepth);
                    IsDirty = true;
                }
            }
        }

        public string ReferenceFolderName {
            get { return _referenceFolderName; }
            set {
                if (_referenceFolderName != value) {
                    _referenceFolderName = value;
                    NotifyOfPropertyChange(() => ReferenceFolderName);
                    IsDirty = true;
                }
            }
        }

        public Uri TfsName {
            get { return _tfsName; }
            set {
                if (_tfsName != value) {
                    _tfsName = value;
                    NotifyOfPropertyChange(() => TfsName);
                    IsDirty = true;
                }
            }
        }

        [JsonConverter(typeof (StringEnumConverter))]
        public VisualStudioVersion VisualStudioVersion {
            get { return _visualStudioVersion; }
            set {
                if (_visualStudioVersion != value) {
                    _visualStudioVersion = value;
                    NotifyOfPropertyChange(() => VisualStudioVersion);
                    IsDirty = true;
                }
            }
        }
    }

    public class WindowSettings {
        public double Top { get; set; }
        public double Left { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool Maximized { get; set; }
    }

    public enum VisualStudioVersion {
        VS2010,
        VS2012,
    }
}