using System;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using NLog;

namespace Solutionizer.Helper {
    public static class TfsHelper {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public static bool TryGetTeamProjectCollection(string localPath, ref Uri tfsName, out string tfsFolder) {
            tfsFolder = null;

            if (tfsName == null) {
                var projectCollections = RegisteredTfsConnections.GetProjectCollections();
                if (projectCollections != null && projectCollections.Length == 1) {
                    tfsName = projectCollections[0].Uri;
                } else {
                    using (var dlg = new TeamProjectPicker(TeamProjectPickerMode.NoProject, false)) {
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

        private static TfsTeamProjectCollection GetTeamProjectCollection(Uri tfsName, ICredentialsProvider credentialsProvider) {
            return TfsTeamProjectCollectionFactory.GetTeamProjectCollection(tfsName, credentialsProvider);
        }

        private static Workspace GetWorkspace(Uri tfsName, string localPath) {
            ICredentialsProvider fallbackCredentialsProvider = new UICredentialsProvider();
            Workspace[] workspaceArray;
            try {
                var projectCollection = GetTeamProjectCollection(tfsName, fallbackCredentialsProvider);
                var versionControlServer = projectCollection.GetService<VersionControlServer>();
                workspaceArray = versionControlServer.QueryWorkspaces(null, versionControlServer.AuthorizedUser, Environment.MachineName);
            } catch (TeamFoundationServiceUnavailableException ex) {
                _log.Info(ex, "TFS is not available");
                workspaceArray = null;
            } catch (TeamFoundationServerUnauthorizedException ex) {
                fallbackCredentialsProvider.GetCredentials(tfsName, null);
                var projectCollection = GetTeamProjectCollection(tfsName, fallbackCredentialsProvider);
                var versionControlServer = projectCollection.GetService<VersionControlServer>();
                workspaceArray = versionControlServer.QueryWorkspaces(null, versionControlServer.AuthorizedUser, Environment.MachineName);
            } catch (Exception ex) {
                _log.Error(ex, "Querying workspaces failed");
                throw;
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