using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using NLog;

namespace Solutionizer.Helper {
    public static class TfsHelper {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private static readonly bool _tfsAssembliesAvailable;
        private static readonly Type _registeredTfsConnectionsType;
        private static readonly Type _teamProjectPickerType;
        private static readonly Type _teamProjectPickerModeType;
        private static readonly Type _tfsTeamProjectCollectionFactoryType;
        private static readonly Type _versionControlServerType;
        private static readonly Type _uiCredentialsProviderType;

        static TfsHelper() {
            try {
                var assemblyTfsClient = Assembly.Load("Microsoft.TeamFoundation.Client, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL");
                var assemblyTfsCommon = Assembly.Load("Microsoft.TeamFoundation.Common, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL");
                var assemblyTfsVcClient = Assembly.Load("Microsoft.TeamFoundation.VersionControl.Client, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL");

                if (assemblyTfsClient == null || assemblyTfsCommon == null || assemblyTfsVcClient == null) {
                    _tfsAssembliesAvailable = false;
                    _log.Info("No TFS assemblies available");
                } else {
                    _registeredTfsConnectionsType = assemblyTfsClient.GetType("Microsoft.TeamFoundation.Client.RegisteredTfsConnections");
                    _teamProjectPickerType = assemblyTfsClient.GetType("Microsoft.TeamFoundation.Client.TeamProjectPicker");
                    _teamProjectPickerModeType = assemblyTfsClient.GetType("Microsoft.TeamFoundation.Client.TeamProjectPickerMode");
                    _tfsTeamProjectCollectionFactoryType = assemblyTfsClient.GetType("Microsoft.TeamFoundation.Client.TfsTeamProjectCollectionFactory");
                    _versionControlServerType = assemblyTfsVcClient.GetType("Microsoft.TeamFoundation.VersionControl.Client.VersionControlServer");
                    _uiCredentialsProviderType = assemblyTfsClient.GetType("Microsoft.TeamFoundation.Client.UICredentialsProvider");

                    _tfsAssembliesAvailable = true;
                    _log.Info("Loaded TFS assemblies and types");
                }
            } catch (Exception ex) {
                _log.WarnException("Exception while loading TFS assemblies", ex);
                _tfsAssembliesAvailable = false;
            }
        }

        public static bool TryGetTeamProjectCollection(string localPath, ref Uri tfsName, out string tfsFolder) {
            tfsFolder = null;

            if (!_tfsAssembliesAvailable) {
                return false;
            }

            if (tfsName == null) {
                var projectCollections = GetProjectCollections();
                if (projectCollections != null && projectCollections.Length == 1) {
                    tfsName = projectCollections[0].Uri;
                } else {
                    using (var dlg = CreateTeamProjectPicker()) {
                        dlg.AcceptButtonText = "Select";
                        if (dlg.ShowDialog() != DialogResult.OK) {
                            return false;
                        }
                        tfsName = dlg.SelectedTeamProjectCollection.Uri;
                    }
                }
            }

            var workspace = GetWorkspace(tfsName, localPath);
            if (workspace == null || !workspace.IsLocalPathMapped(localPath)) {
                return false;
            }

            tfsFolder = workspace.GetServerItemForLocalItem(localPath);
            return true;
        }

        private static dynamic GetProjectCollections() {
            return _registeredTfsConnectionsType.InvokeMember("GetProjectCollections", BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, null);
        }

        private static dynamic CreateTeamProjectPicker() {
            var noProject = Enum.Parse(_teamProjectPickerModeType, "NoProject");
            return Activator.CreateInstance(_teamProjectPickerType, new[] { noProject, false});
        }

        private static dynamic GetTeamProjectCollection(Uri tfsName, object credentialsProvider) {
            return _tfsTeamProjectCollectionFactoryType.InvokeMember("GetTeamProjectCollection",
                                                                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod,
                                                                    null, null, new[] {
                                                                        tfsName, credentialsProvider
                                                                    });
        }

        private static dynamic GetWorkspace(Uri tfsName, string localPath) {
            dynamic fallbackCredentialsProvider = Activator.CreateInstance(_uiCredentialsProviderType);
            dynamic workspaceArray;
            try {
                var projectCollection = GetTeamProjectCollection(tfsName, fallbackCredentialsProvider);
                var versionControlServer = projectCollection.GetService(_versionControlServerType);
                workspaceArray = versionControlServer.QueryWorkspaces(null, versionControlServer.AuthorizedUser, Environment.MachineName);
            } catch (Exception ex) {
                switch (ex.GetType().Name) {
                    case "TeamFoundationServiceUnavailableException":
                        _log.InfoException("TFS is not available", ex);
                        workspaceArray = null;
                        break;
                    case "TeamFoundationServerUnauthorizedException": 
                        fallbackCredentialsProvider.GetCredentials(tfsName, null);
                        var projectCollection = GetTeamProjectCollection(tfsName, fallbackCredentialsProvider);
                        var versionControlServer = projectCollection.GetService(_versionControlServerType);
                        workspaceArray = versionControlServer.QueryWorkspaces(null, versionControlServer.AuthorizedUser, Environment.MachineName);
                        break;
                    default:
                        _log.ErrorException("Querying workspaces failed", ex);
                        throw;
                }
            }

            if (workspaceArray == null || workspaceArray.Length == 0) {
                return null;
            }

            if (workspaceArray.Length == 1) {
                return workspaceArray[0];
            }

            foreach (var workspace in workspaceArray) {
                foreach (var workingFolder in workspace.Folders) {
                    if (!workingFolder.IsCloaked && workspace.IsLocalPathMapped(localPath) && localPath.StartsWith(workingFolder.LocalItem, true, CultureInfo.InvariantCulture)) {
                        return workspace;
                    }
                }
            }

            return null;
        }
    }
}