using System;
using System.IO;
using Microsoft.Win32;
using Solutionizer.Services;

namespace Solutionizer.Helper {
    public static class VisualStudioHelper {
        public static VisualStudioVersion DetectVersion() {
            using (var key = Registry.ClassesRoot.OpenSubKey("VisualStudio.DTE.11.0")) {
                if (key != null) {
                    return VisualStudioVersion.VS2012;
                }
            }
            return VisualStudioVersion.VS2010;
        }

        public static string GetDefaultProjectsLocation(VisualStudioVersion visualStudioVersion) {
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
            }
            finally {
                if (key != null) {
                    key.Close();
                }
            }
        }
    }
}