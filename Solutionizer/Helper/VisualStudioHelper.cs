using System;
using System.IO;
using Microsoft.Win32;
using Solutionizer.Services;

namespace Solutionizer.Helper {
    public static class VisualStudioHelper {
        public static VisualStudioVersion DetectVersion() {
            using (var key = Registry.ClassesRoot.OpenSubKey("VisualStudio.DTE.14.0")) {
                if (key != null) {
                    return VisualStudioVersion.VS2013;
                }
            }
            using (var key = Registry.ClassesRoot.OpenSubKey("VisualStudio.DTE.12.0")) {
                if (key != null) {
                    return VisualStudioVersion.VS2013;
                }
            }
            using (var key = Registry.ClassesRoot.OpenSubKey("VisualStudio.DTE.11.0")) {
                if (key != null) {
                    return VisualStudioVersion.VS2012;
                }
            }
            return VisualStudioVersion.VS2010;
        }

        private static string GetVersionKey(VisualStudioVersion visualStudioVersion) {
            switch (visualStudioVersion) {
                case VisualStudioVersion.VS2012:
                    return "11.0";
                case VisualStudioVersion.VS2013:
                    return "12.0";
                case VisualStudioVersion.VS2015:
                    return "14.0";
            }
            return "10.0";
        }

        public static string GetDefaultProjectsLocation(VisualStudioVersion visualStudioVersion) {
            RegistryKey key = null;
            string location = null;
            try {
                key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\VisualStudio\" + GetVersionKey(visualStudioVersion));
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

        public static string GetVisualStudioExecutable(VisualStudioVersion visualStudioVersion) {
            var regPath = String.Format(@"Software\{0}Microsoft\VisualStudio\{1}",
                Environment.Is64BitOperatingSystem ? @"Wow6432Node\" : String.Empty,
                GetVersionKey(visualStudioVersion));
            using (var key = Registry.LocalMachine.OpenSubKey(regPath)) {
                var installPath = key.GetValue("InstallDir") as string;
                return Path.Combine(installPath, "devenv.exe");
            }
        }
    }
}