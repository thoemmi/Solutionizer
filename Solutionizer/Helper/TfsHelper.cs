using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;

namespace Solutionizer.Helper {
    public static class TfsHelper {
        private static readonly Assembly _assemblyTfsClient;
        private static readonly Assembly _assemblyTfsCommon;
        private static readonly Assembly _assemblyTfsVcClient;

        static TfsHelper() {
            _assemblyTfsClient = Assembly.Load("Microsoft.TeamFoundation.Client, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL");
            _assemblyTfsCommon = Assembly.Load("Microsoft.TeamFoundation.Common, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL");
            _assemblyTfsVcClient = Assembly.Load("Microsoft.TeamFoundation.VersionControl.Client, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL");
        }

        public static bool TryGetTeamProjectCollection(string localPath, ref Uri tfsName, out string tfsFolder) {
            tfsFolder = null;

            if (_assemblyTfsClient == null || _assemblyTfsCommon == null || _assemblyTfsVcClient == null) {
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
            var registeredTfsConnectionsType = _assemblyTfsClient.GetType("Microsoft.TeamFoundation.Client.RegisteredTfsConnections");
            return registeredTfsConnectionsType.InvokeMember("GetProjectCollections", BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, null);
        }

        private static dynamic CreateTeamProjectPicker() {
            var teamProjectPickerType = _assemblyTfsClient.GetType("Microsoft.TeamFoundation.Client.TeamProjectPicker");
            var teamProjectPickerModeType = _assemblyTfsClient.GetType("Microsoft.TeamFoundation.Client.TeamProjectPickerMode");
            var noProject = Enum.Parse(teamProjectPickerModeType, "NoProject");
            return Activator.CreateInstance(teamProjectPickerType, new[] { noProject, false});
        }

        private static dynamic GetTeamProjectCollection(Uri tfsName, object credentialsProvider) {
            var tfsTeamProjectCollectionFactoryType = _assemblyTfsClient.GetType("Microsoft.TeamFoundation.Client.TfsTeamProjectCollectionFactory");
            return tfsTeamProjectCollectionFactoryType.InvokeMember("GetTeamProjectCollection",
                                                                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod,
                                                                    null, null, new[] {
                                                                        tfsName, credentialsProvider
                                                                    });
        }

        private static dynamic GetWorkspace(Uri tfsName, string localPath) {
            var versionControlServerType = _assemblyTfsVcClient.GetType("Microsoft.TeamFoundation.VersionControl.Client.VersionControlServer");
            dynamic fallbackCredentialsProvider = Activator.CreateInstance(_assemblyTfsClient.GetType("Microsoft.TeamFoundation.Client.UICredentialsProvider"));
            dynamic workspaceArray;
            try {
                var projectCollection = GetTeamProjectCollection(tfsName, fallbackCredentialsProvider);
                var versionControlServer = projectCollection.GetService(versionControlServerType);
                workspaceArray = versionControlServer.QueryWorkspaces(null, versionControlServer.AuthorizedUser, Environment.MachineName);
            } catch (Exception ex) {
                if (ex.GetType().Name == "TeamFoundationServerUnauthorizedException") {
                    fallbackCredentialsProvider.GetCredentials(tfsName, null);
                    var projectCollection = GetTeamProjectCollection(tfsName, fallbackCredentialsProvider);
                    var versionControlServer = projectCollection.GetService(versionControlServerType);
                    workspaceArray = versionControlServer.QueryWorkspaces(null, versionControlServer.AuthorizedUser, Environment.MachineName);
                } else {
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