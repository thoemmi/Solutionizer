using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Solutionizer.Helper;
using TinyLittleMvvm;

namespace Solutionizer.Services {
    public class Settings : PropertyChangedBase, ISettings {
        private bool _scanOnStartup = true;
        private bool _isFlatMode;
        private bool _isDirty;
        private WindowSettings _windowSettings;
        private bool _includeReferencedProjects = true;
        private int _referenceTreeDepth = 6;
        private bool _dontBuildReferencedProjects;
        private bool _simplifyProjectTree;
        private Uri _tfsName;
        private VisualStudioVersion _visualStudioVersion;
        private string _referenceFolderName = "_References";
        private string _rootPath;
        private bool _showLaunchElevatedButton;
        private bool _showProjectCount;
        private string _lastUpdateCheckETag;
        private bool _autoUpdateCheck = true;
        private bool _includePrereleaseUpdates;
        private string _someOtherProperty;
        private SolutionTargetLocation _solutionTargetLocation;
        private string _customTargetFolder;
        private string _customTargetSubfolder;

        public Settings() {
            _visualStudioVersion = VisualStudioHelper.DetectVersion();
            _rootPath = VisualStudioHelper.GetDefaultProjectsLocation(_visualStudioVersion);
            _customTargetFolder = _rootPath;
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

        public bool DontBuildReferencedProjects {
            get { return _dontBuildReferencedProjects; }
            set {
                if (_dontBuildReferencedProjects != value) {
                    _dontBuildReferencedProjects = value;
                    NotifyOfPropertyChange(() => DontBuildReferencedProjects);
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

        public bool ShowLaunchElevatedButton {
            get { return _showLaunchElevatedButton; }
            set {
                if (_showLaunchElevatedButton != value) {
                    _showLaunchElevatedButton = value;
                    NotifyOfPropertyChange(() => ShowLaunchElevatedButton);
                    IsDirty = true;
                }
            }
        }

        public bool ShowProjectCount {
            get { return _showProjectCount; }
            set {
                if (_showProjectCount != value) {
                    _showProjectCount = value;
                    NotifyOfPropertyChange(() => ShowProjectCount);
                    IsDirty = true;
                }
            }
        }

        public string LastUpdateCheckETag {
            get { return _lastUpdateCheckETag; }
            set {
                if (value != _lastUpdateCheckETag) {
                    _lastUpdateCheckETag = value;
                    NotifyOfPropertyChange(() => LastUpdateCheckETag);
                    IsDirty = true;
                }
            }
        }

        public string SomeOtherProperty {
            get { return _someOtherProperty; }
            set {
                if (value != _someOtherProperty) {
                    _someOtherProperty = value;
                    NotifyOfPropertyChange(() => SomeOtherProperty);
                    IsDirty = true;
                }
            }
        }

        public bool AutoUpdateCheck {
            get { return _autoUpdateCheck; }
            set {
                if (_autoUpdateCheck != value) {
                    _autoUpdateCheck = value;
                    NotifyOfPropertyChange(() => AutoUpdateCheck);
                    IsDirty = true;
                }
            }
        }

        public bool IncludePrereleaseUpdates {
            get { return _includePrereleaseUpdates; }
            set {
                if (_includePrereleaseUpdates != value) {
                    _includePrereleaseUpdates = value;
                    NotifyOfPropertyChange(() => IncludePrereleaseUpdates);
                    IsDirty = true;
                }
            }
        }

        public SolutionTargetLocation SolutionTargetLocation {
            get { return _solutionTargetLocation; }
            set {
                if (_solutionTargetLocation != value) {
                    _solutionTargetLocation = value;
                    NotifyOfPropertyChange(() => SolutionTargetLocation);
                    IsDirty = true;
                }
            }
        }

        public string CustomTargetFolder {
            get { return _customTargetFolder; }
            set {
                if (_customTargetFolder != value) {
                    _customTargetFolder = value;
                    NotifyOfPropertyChange(() => CustomTargetFolder);
                    IsDirty = true;
                }
            }
        }

        public string CustomTargetSubfolder {
            get { return _customTargetSubfolder; }
            set {
                if (_customTargetSubfolder != value) {
                    _customTargetSubfolder = value;
                    NotifyOfPropertyChange(() => CustomTargetSubfolder);
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

    // ReSharper disable InconsistentNaming
    public enum VisualStudioVersion {
        VS2010,
        VS2012,
        VS2013,
    }
    // ReSharper restore InconsistentNaming

    public enum SolutionTargetLocation {
        TempFolder,
        CustomFolder,
        BelowRootPath
    }
}