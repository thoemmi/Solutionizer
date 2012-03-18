﻿using System;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Solutionizer.Infrastructure;
using Solutionizer.ViewModels;

namespace Solutionizer.Helper {
    public static class TfsHelper {
        public static bool TryGetTeamProjectCollection(string localPath, out Uri tfsName, out string tfsFolder) {
            tfsName = Settings.Instance.TFSName;
            tfsFolder = null;
            if (tfsName == null) {
                using (var dlg = new TeamProjectPicker(TeamProjectPickerMode.NoProject, false)) {
                    dlg.AcceptButtonText = "Select";
                    if (dlg.ShowDialog() != DialogResult.OK) {
                        return false;
                    }
                    tfsName = dlg.SelectedTeamProjectCollection.Uri;
                    Settings.Instance.TFSName = tfsName;
                }
            }

            var workspace = GetWorkspace(tfsName, localPath);
            if (workspace == null || !workspace.IsLocalPathMapped(localPath)) {
                return false;
            }

            tfsFolder = workspace.GetServerItemForLocalItem(localPath);
            return true;
        }

        private static Workspace GetWorkspace(Uri tfsName, string localPath) {
            var fallbackCredentialsProvider = (ICredentialsProvider) new UICredentialsProvider();
            Workspace[] workspaceArray;
            try {
                var projectCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(tfsName, fallbackCredentialsProvider);
                var versionControlServer = projectCollection.GetService<VersionControlServer>();
                workspaceArray = versionControlServer.QueryWorkspaces(null, versionControlServer.AuthorizedUser, Environment.MachineName);
            } catch (TeamFoundationServerUnauthorizedException) {
                fallbackCredentialsProvider.GetCredentials(tfsName, null);
                var projectCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(tfsName, fallbackCredentialsProvider);
                var versionControlServer = projectCollection.GetService<VersionControlServer>();
                workspaceArray = versionControlServer.QueryWorkspaces(null, versionControlServer.AuthorizedUser, Environment.MachineName);
            }

            if (workspaceArray.Length == 0) {
                return null;
            }

            if (workspaceArray.Length == 1) {
                return workspaceArray[0];
            }

            foreach (var workspace in workspaceArray) {
                foreach (var workingFolder in workspace.Folders) {
                    if (!workingFolder.IsCloaked && localPath.StartsWith(workingFolder.LocalItem, true, CultureInfo.InvariantCulture) &&
                        workspace.IsLocalPathMapped(localPath)) {
                        return workspace;
                    }
                }
            }
            //WorkspaceSelectionDlg workspaceSelectionDlg = new WorkspaceSelectionDlg((IEnumerable<Workspace>)workspaceArray, Settings.Default.TFSWorkspace);
            //if (workspaceSelectionDlg.ShowDialog() == DialogResult.Cancel)
            //    return (Workspace)null;
            //Workspace selectedWorkspace = workspaceSelectionDlg.SelectedWorkspace;
            //Settings.Default.TFSWorkspace = selectedWorkspace.Name;
            return null;
        }
    }
}