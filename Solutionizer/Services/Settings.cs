using System;
using System.IO;
using Caliburn.Micro;
using Microsoft.Win32;
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
        private VisualStudioVersion _visualStudioVersion;
        private string _referenceFolderName = "_References";
        private string _rootPath;

        public Settings() {
            _visualStudioVersion = DetectVisualStudioVersion();
            _rootPath = GetDefaultProjectsLocation(_visualStudioVersion);
        }

        private static VisualStudioVersion DetectVisualStudioVersion() {
            using (var key = Registry.ClassesRoot.OpenSubKey("VisualStudio.DTE.11.0")) {
                if (key != null) {
                    return VisualStudioVersion.VS2012;
                }
            }
            return VisualStudioVersion.VS2010;
        }
        
        private static string GetDefaultProjectsLocation(VisualStudioVersion visualStudioVersion) {
            RegistryKey key = null;
            string location = null;
            try {
                if (visualStudioVersion == VisualStudioVersion.VS2012) {
                    key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\VisualStudio\11.0");
                }
                if (key == null) {
                    key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\VisualStudio\10.0");
                }
                if (key != null) {
                    location = key.GetValue("DefaultNewProjectLocation") as string;
                }
                if (String.IsNullOrEmpty(location)) {
                    location = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "Visual Studio 2010",
                        "Projects");
                }
                return location;
            } finally {
                if (key != null) {
                    key.Close();
                }
            }
        }

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
                value = Math.Max(1, value);
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