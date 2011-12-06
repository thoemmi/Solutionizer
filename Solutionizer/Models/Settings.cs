using System;
using System.IO;
using System.Reflection;
using System.Windows.Markup;
using System.Xml;
using GalaSoft.MvvmLight;

namespace Solutionizer.Models {
    public class Settings : ViewModelBase {
        private static Settings _instance;
        private bool _scanOnStartup = true;
        private bool _isFlatMode;
        private bool _hideRootNode;
        private bool _isDirty;
        private WindowSettings _windowSettings;
        private bool _includeReferencedProjects = true;
        private int _referenceTreeDepth = 6;
        private Uri _tfsName;
        private VisualStudioVersion _visualStudioVersion = VisualStudioVersion.Vs2010;

        private string _rootPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Visual Studio 2010",
            "Projects");

        private static string SettingsPath {
            get {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    Assembly.GetEntryAssembly().GetName().Name,
                    "settings.xml");
            }
        }

        public static Settings Instance {
            set { _instance = value; }
            get {
                if (_instance == null) {
                    if (File.Exists(SettingsPath)) {
                        using (var stream = File.OpenRead(SettingsPath)) {
                            _instance = (Settings) XamlReader.Load(stream);
                        }
                    } else {
                        _instance = new Settings();
                    }
                    _instance.IsDirty = false;
                }
                return _instance;
            }
        }

        public void Save() {
            if (!IsDirty) {
                return;
            }

            var path = Path.GetDirectoryName(SettingsPath);
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }

            using (var stream = XmlWriter.Create(SettingsPath, new XmlWriterSettings {
                Indent = true, NewLineOnAttributes = true
            })) {
                XamlWriter.Save(this, stream);
            }

            IsDirty = false;
        }

        public bool IsFlatMode {
            get { return _isFlatMode; }
            set {
                if (_isFlatMode != value) {
                    _isFlatMode = value;
                    RaisePropertyChanged(() => IsFlatMode);
                    IsDirty = true;
                }
            }
        }

        public bool HideRootNode {
            get { return _hideRootNode; }
            set {
                if (_hideRootNode != value) {
                    _hideRootNode = value;
                    RaisePropertyChanged(() => HideRootNode);
                    IsDirty = true;
                }
            }
        }

        public string RootPath {
            get { return _rootPath; }
            set {
                if (_rootPath != value) {
                    _rootPath = value;
                    RaisePropertyChanged(() => RootPath);
                    IsDirty = true;
                }
            }
        }

        public bool IsDirty {
            get { return _isDirty; }
            private set {
                if (_isDirty != value) {
                    _isDirty = value;
                    RaisePropertyChanged(() => IsDirty);
                }
            }
        }

        public bool ScanOnStartup {
            get { return _scanOnStartup; }
            set {
                if (_scanOnStartup != value) {
                    _scanOnStartup = value;
                    RaisePropertyChanged(() => ScanOnStartup);
                    IsDirty = true;
                }
            }
        }

        public WindowSettings WindowSettings {
            get { return _windowSettings; }
            set {
                if (_windowSettings != value) {
                    _windowSettings = value;
                    RaisePropertyChanged(() => WindowSettings);
                    IsDirty = true;
                }
            }
        }

        public bool IncludeReferencedProjects {
            get { return _includeReferencedProjects; }
            set {
                if (_includeReferencedProjects != value) {
                    _includeReferencedProjects = value;
                    RaisePropertyChanged(() => IncludeReferencedProjects);
                    IsDirty = true;
                }
            }
        }

        public int ReferenceTreeDepth {
            get { return _referenceTreeDepth; }
            set {
                if (_referenceTreeDepth != value) {
                    _referenceTreeDepth = value;
                    RaisePropertyChanged(() => ReferenceTreeDepth);
                    IsDirty = true;
                }
            }
        }

        public Uri TFSName {
            get { return _tfsName; }
            set {
                if (_tfsName != value) {
                    _tfsName = value;
                    RaisePropertyChanged(() => TFSName);
                    IsDirty = true;
                }
            }
        }

        public VisualStudioVersion VisualStudioVersion {
            get { return _visualStudioVersion; }
            set {
                if (_visualStudioVersion != value) {
                    _visualStudioVersion = value;
                    RaisePropertyChanged(() => VisualStudioVersion);
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
        Vs2010,
        Vs2011
    }
}