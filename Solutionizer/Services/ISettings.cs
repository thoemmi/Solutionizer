using System;
using System.ComponentModel;

namespace Solutionizer.Services {
    public interface ISettings : INotifyPropertyChanged {
        bool IsFlatMode { get; set; }
        bool SimplifyProjectTree { get; set; }
        string RootPath { get; set; }
        bool IsDirty { get; }
        bool ScanOnStartup { get; set; }
        WindowSettings WindowSettings { get; set; }
        bool IncludeReferencedProjects { get; set; }
        int ReferenceTreeDepth { get; set; }
        string ReferenceFolderName { get; set; }
        Uri TfsName { get; set; }
        string VisualStudioVersion { get; set; }
        bool ShowLaunchElevatedButton { get; set; }
        bool DontBuildReferencedProjects { get; set; }
        bool ShowProjectCount { get; set; }
        bool AutoUpdateCheck { get; set; }
        bool IncludePrereleaseUpdates { get; set; }
        string LastUpdateCheckETag { get; set; }
        SolutionTargetLocation SolutionTargetLocation { get; set; }
        string CustomTargetFolder { get; set; }
        string CustomTargetSubfolder { get; set; }
    }
}