using System;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using Solutionizer.Framework;
using Solutionizer.Services;

namespace Solutionizer.ViewModels {
    public sealed class SettingsViewModel : DialogViewModel, IOnLoadedHandler, IWithTitle {
        private readonly ISettings _settings;
        private bool _scanOnStartup;
        private bool _simplifyProjectTree;
        private bool _includeReferencedProjects;
        private bool _isFlatMode;
        private string _referenceFolderName;
        private int _referenceTreeDepth;
        private bool _dontBuildReferencedProjects;
        private VisualStudioVersion _visualStudioVersion;
        private bool _showLaunchElevatedButton;
        private bool _showProjectCount;
        private bool _includePrereleaseUpdates;
        private SolutionTargetLocation _solutionTargetLocation;
        private string _customTargetFolder;
        private string _customTargetSubfolder;
        private readonly ICommand _okCommand;
        private readonly ICommand _cancelCommand;
        private readonly ICommand _selectSolutionTargetFolderCommand;

        public SettingsViewModel(ISettings settings) {
            _settings = settings;
            _okCommand = new RelayCommand(Ok, () => CanOk);
            _cancelCommand = new RelayCommand(Close);
            _selectSolutionTargetFolderCommand = new RelayCommand(SelectSolutionTargetFolder);
        }

        public string Title {
            get { return "Settings"; }
        }

        public void OnLoaded() {
            ScanOnStartup = _settings.ScanOnStartup;
            SimplifyProjectTree = _settings.SimplifyProjectTree;
            IncludeReferencedProjects = _settings.IncludeReferencedProjects;
            ReferenceFolderName = _settings.ReferenceFolderName;
            ReferenceTreeDepth = _settings.ReferenceTreeDepth;
            DontBuildReferencedProjects = _settings.DontBuildReferencedProjects;
            IsFlatMode = _settings.IsFlatMode;
            VisualStudioVersion = _settings.VisualStudioVersion;
            ShowLaunchElevatedButton = _settings.ShowLaunchElevatedButton;
            ShowProjectCount = _settings.ShowProjectCount;
            IncludePrereleaseUpdates = _settings.IncludePrereleaseUpdates;
            SolutionTargetLocation = _settings.SolutionTargetLocation;
            CustomTargetFolder = _settings.CustomTargetFolder;
            CustomTargetSubfolder = _settings.CustomTargetSubfolder;
        }

        public bool ScanOnStartup {
            get { return _scanOnStartup; }
            set {
                if (_scanOnStartup != value) {
                    _scanOnStartup = value;
                    NotifyOfPropertyChange(() => ScanOnStartup);
                }
            }
        }

        public bool SimplifyProjectTree {
            get { return _simplifyProjectTree; }
            set {
                if (_simplifyProjectTree != value) {
                    _simplifyProjectTree = value;
                    NotifyOfPropertyChange(() => SimplifyProjectTree);
                }
            }
        }

        public bool IncludeReferencedProjects {
            get { return _includeReferencedProjects; }
            set {
                if (_includeReferencedProjects != value) {
                    _includeReferencedProjects = value;
                    NotifyOfPropertyChange(() => IncludeReferencedProjects);
                }
            }
        }

        public string ReferenceFolderName {
            get { return _referenceFolderName; }
            set {
                if (_referenceFolderName != value) {
                    _referenceFolderName = value;
                    NotifyOfPropertyChange(() => ReferenceFolderName);
                }
            }
        }

        public int ReferenceTreeDepth {
            get { return _referenceTreeDepth; }
            set {
                if (_referenceTreeDepth != value) {
                    _referenceTreeDepth = value;
                    NotifyOfPropertyChange(() => ReferenceTreeDepth);
                }
            }
        }

        public bool DontBuildReferencedProjects {
            get { return _dontBuildReferencedProjects; }
            set {
                if (_dontBuildReferencedProjects != value) {
                    _dontBuildReferencedProjects = value;
                    NotifyOfPropertyChange(() => DontBuildReferencedProjects);
                }
            }
        }

        public bool IsFlatMode {
            get { return _isFlatMode; }
            set {
                if (_isFlatMode != value) {
                    _isFlatMode = value;
                    NotifyOfPropertyChange(() => IsFlatMode);
                }
            }
        }

        public VisualStudioVersion VisualStudioVersion {
            get { return _visualStudioVersion; }
            set {
                if (_visualStudioVersion != value) {
                    _visualStudioVersion = value;
                    NotifyOfPropertyChange(() => VisualStudioVersion);
                }
            }
        }

        public bool ShowLaunchElevatedButton {
            get { return _showLaunchElevatedButton; }
            set {
                if (_showLaunchElevatedButton != value) {
                    _showLaunchElevatedButton = value;
                    NotifyOfPropertyChange(() => ShowLaunchElevatedButton);
                }
            }
        }

        public bool ShowProjectCount {
            get { return _showProjectCount; }
            set {
                if (_showProjectCount != value) {
                    _showProjectCount = value;
                    NotifyOfPropertyChange(() => ShowProjectCount);
                }
            }
        }

        public bool IncludePrereleaseUpdates {
            get { return _includePrereleaseUpdates; }
            set {
                if (_includePrereleaseUpdates != value) {
                    _includePrereleaseUpdates = value;
                    NotifyOfPropertyChange(() => IncludePrereleaseUpdates);
                }
            }
        }

        public SolutionTargetLocation SolutionTargetLocation {
            get { return _solutionTargetLocation; }
            set {
                if (_solutionTargetLocation != value) {
                    _solutionTargetLocation = value;
                    NotifyOfPropertyChange(() => SolutionTargetLocation);
                }
            }
        }

        public string CustomTargetFolder {
            get { return _customTargetFolder; }
            set {
                if (_customTargetFolder != value) {
                    _customTargetFolder = value;
                    NotifyOfPropertyChange(() => CustomTargetFolder);
                }
            }
        }

        public ICommand SelectSolutionTargetFolderCommand {
            get { return _selectSolutionTargetFolderCommand; }
        }

        private void SelectSolutionTargetFolder() {
            var dlg = new VistaFolderBrowserDialog {
                SelectedPath = CustomTargetFolder
            };
            if (dlg.ShowDialog(Application.Current.MainWindow) == true) {
                CustomTargetFolder = dlg.SelectedPath;
            }
        }

        public string CustomTargetSubfolder {
            get { return _customTargetSubfolder; }
            set {
                if (_customTargetSubfolder != value) {
                    _customTargetSubfolder = value;
                    NotifyOfPropertyChange(() => CustomTargetSubfolder);
                }
            }
        }

        protected override void NotifyOfPropertyChange(string propertyName = null) {
            base.NotifyOfPropertyChange(propertyName);

            // if any property changed that is not CanOk, we want the UI to evaluate that property
            Expression<Func<bool>> property = () => CanOk;
            var canOkName = GetMemberInfo(property).Name;
            if (propertyName != canOkName) {
                NotifyOfPropertyChange(property);
            }
        }

        public ICommand OkCommand {
            get { return _okCommand; }
        }

        public ICommand CancelCommand {
            get { return _cancelCommand; }
        }

        public void Ok() {
            _settings.ScanOnStartup = ScanOnStartup;
            _settings.SimplifyProjectTree = SimplifyProjectTree;
            _settings.IncludeReferencedProjects = IncludeReferencedProjects;
            _settings.ReferenceFolderName = ReferenceFolderName;
            _settings.ReferenceTreeDepth = ReferenceTreeDepth;
            _settings.DontBuildReferencedProjects = DontBuildReferencedProjects;
            _settings.IsFlatMode = IsFlatMode;
            _settings.VisualStudioVersion = VisualStudioVersion;
            _settings.ShowLaunchElevatedButton = ShowLaunchElevatedButton;
            _settings.ShowProjectCount = ShowProjectCount;
            _settings.IncludePrereleaseUpdates = IncludePrereleaseUpdates;
            _settings.SolutionTargetLocation = SolutionTargetLocation;
            _settings.CustomTargetFolder = CustomTargetFolder;
            _settings.CustomTargetSubfolder = CustomTargetSubfolder;

            Close();
        }

        public bool CanOk {
            get {
                return
                    ScanOnStartup != _settings.ScanOnStartup ||
                    SimplifyProjectTree != _settings.SimplifyProjectTree ||
                    IncludeReferencedProjects != _settings.IncludeReferencedProjects ||
                    ReferenceFolderName != _settings.ReferenceFolderName ||
                    ReferenceTreeDepth != _settings.ReferenceTreeDepth ||
                    DontBuildReferencedProjects != _settings.DontBuildReferencedProjects ||
                    IsFlatMode != _settings.IsFlatMode ||
                    VisualStudioVersion != _settings.VisualStudioVersion ||
                    ShowLaunchElevatedButton != _settings.ShowLaunchElevatedButton ||
                    ShowProjectCount != _settings.ShowProjectCount ||
                    IncludePrereleaseUpdates != _settings.IncludePrereleaseUpdates ||
                    SolutionTargetLocation != _settings.SolutionTargetLocation ||
                    CustomTargetFolder != _settings.CustomTargetFolder ||
                    CustomTargetSubfolder != _settings.CustomTargetSubfolder;
            }
        }
    }
}