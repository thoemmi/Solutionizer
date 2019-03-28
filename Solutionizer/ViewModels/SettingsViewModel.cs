using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using Solutionizer.Services;
using TinyLittleMvvm;

namespace Solutionizer.ViewModels {
    public sealed class SettingsViewModel : DialogViewModel, IOnLoadedHandler {
        private readonly ISettings _settings;
        private readonly IVisualStudioInstallationsProvider _visualStudioInstallationsProvider;
        private bool _scanOnStartup;
        private bool _simplifyProjectTree;
        private bool _includeReferencedProjects;
        private bool _isFlatMode;
        private string _referenceFolderName;
        private int _referenceTreeDepth;
        private bool _dontBuildReferencedProjects;
        private string _visualStudioVersion;
        private bool _showLaunchElevatedButton;
        private bool _showProjectCount;
        private bool _autoUpdateCheck;
        private bool _includePrereleaseUpdates;
        private SolutionTargetLocation _solutionTargetLocation;
        private string _customTargetFolder;
        private string _customTargetSubfolder;
        private readonly ICommand _okCommand;
        private readonly ICommand _cancelCommand;
        private readonly ICommand _selectSolutionTargetFolderCommand;

        public SettingsViewModel(ISettings settings, IVisualStudioInstallationsProvider visualStudioInstallationsProvider) {
            _settings = settings;
            _visualStudioInstallationsProvider = visualStudioInstallationsProvider;
            _okCommand = new RelayCommand(Ok, () => CanOk);
            _cancelCommand = new RelayCommand(Close);
            _selectSolutionTargetFolderCommand = new RelayCommand(SelectSolutionTargetFolder);
        }

        public Task OnLoadedAsync() {
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
            AutoUpdateCheck = _settings.AutoUpdateCheck;
            return Task.FromResult(0);
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
                    ValidateReferenceFolderName();
                    NotifyOfPropertyChange(() => IncludeReferencedProjects);
                }
            }
        }

        public string ReferenceFolderName {
            get { return _referenceFolderName; }
            set {
                if (_referenceFolderName != value) {
                    _referenceFolderName = value;
                    ValidateReferenceFolderName();
                    NotifyOfPropertyChange(() => ReferenceFolderName);
                }
            }
        }

        private void ValidateReferenceFolderName() {
            Validate(
                !IncludeReferencedProjects || !String.IsNullOrEmpty(ReferenceFolderName), 
                () => ReferenceFolderName, 
                "Folder name must not be empty.");

            Validate(
                !IncludeReferencedProjects || IsValidFileName(ReferenceFolderName), 
                () => ReferenceFolderName, 
                "The folder name contains invalid characters.");
        }

        private bool IsValidFileName(string s) {
            if (String.IsNullOrEmpty(s)) {
                return false;
            }
            var invalidPathChars = System.IO.Path.GetInvalidFileNameChars();
            return !invalidPathChars.Any(s.Contains);
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

        public IReadOnlyList<VisualStudioInstallation> Installations => _visualStudioInstallationsProvider.Installations;


        public string VisualStudioVersion {
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

        public bool AutoUpdateCheck {
            get { return _autoUpdateCheck; }
            set {
                if (_autoUpdateCheck != value) {
                    _autoUpdateCheck = value;
                    NotifyOfPropertyChange(() => AutoUpdateCheck);
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
                    ValidateTargetFolder();
                    NotifyOfPropertyChange(() => SolutionTargetLocation);
                }
            }
        }

        public string CustomTargetFolder {
            get { return _customTargetFolder; }
            set {
                if (_customTargetFolder != value) {
                    _customTargetFolder = value;
                    ValidateTargetFolder();
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
                    ValidateTargetFolder();
                    NotifyOfPropertyChange(() => CustomTargetSubfolder);
                }
            }
        }

        private void ValidateTargetFolder() {
            Validate(
                SolutionTargetLocation != SolutionTargetLocation.CustomFolder || !String.IsNullOrEmpty(CustomTargetFolder),
                () => CustomTargetFolder,
                "You must specify a folder."
                );

            Validate(
                SolutionTargetLocation != SolutionTargetLocation.BelowRootPath || !String.IsNullOrEmpty(CustomTargetSubfolder),
                () => CustomTargetSubfolder,
                "Folder name must not be empty.");

            Validate(
                SolutionTargetLocation != SolutionTargetLocation.BelowRootPath || IsValidFileName(CustomTargetSubfolder),
                () => CustomTargetSubfolder,
                "The folder name contains invalid characters.");

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
            _settings.AutoUpdateCheck = AutoUpdateCheck;
            _settings.IncludePrereleaseUpdates = IncludePrereleaseUpdates;
            _settings.SolutionTargetLocation = SolutionTargetLocation;
            _settings.CustomTargetFolder = CustomTargetFolder;
            _settings.CustomTargetSubfolder = CustomTargetSubfolder;

            Close();
        }

        public bool CanOk {
            get {
                return
                    !HasErrors && (
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
                        AutoUpdateCheck != _settings.AutoUpdateCheck ||
                        IncludePrereleaseUpdates != _settings.IncludePrereleaseUpdates ||
                        SolutionTargetLocation != _settings.SolutionTargetLocation ||
                        CustomTargetFolder != _settings.CustomTargetFolder ||
                        CustomTargetSubfolder != _settings.CustomTargetSubfolder);
            }
        }
    }
}