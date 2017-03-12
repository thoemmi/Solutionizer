using System;
using System.IO;
using Microsoft.Win32;
using Solutionizer.Services;

namespace Solutionizer.Helper {
    public static class VisualStudioHelper {
        public static VisualStudioVersion DetectVersion() {
            using (var key = Registry.ClassesRoot.OpenSubKey("VisualStudio.DTE.15.0")) {
                if (key != null) {
                    return VisualStudioVersion.VS2017;
                }
            }
            using (var key = Registry.ClassesRoot.OpenSubKey("VisualStudio.DTE.14.0")) {
                if (key != null) {
                    return VisualStudioVersion.VS2015;
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
                case VisualStudioVersion.VS2017:
                    return "15.0";
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
            using (var hiveKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)) {
                if (visualStudioVersion == VisualStudioVersion.VS2017) {
                    //TODO maybe use https://www.nuget.org/packages/Microsoft.VisualStudio.Setup.Configuration.Interop/ to detect multiple parallel installed VS2017 instances
                    var regPath = @"Software\WOW6432Node\Microsoft\VisualStudio\SxS\VS7";
                    using (var key = hiveKey.OpenSubKey(regPath)) {
                        var installPath = key.GetValue("15.0") as string;
                        return Path.Combine(installPath, @"Common7\IDE\devenv.exe");
                    }
                } else {
                    var regPath = String.Format(@"Software\Microsoft\VisualStudio\{0}", GetVersionKey(visualStudioVersion));
                    using (var key = hiveKey.OpenSubKey(regPath)) {
                        var installPath = key.GetValue("InstallDir") as string;
                        return Path.Combine(installPath, "devenv.exe");
                    }
                }
            }
        }
    }
}