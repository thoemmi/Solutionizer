using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.Setup.Configuration;
using Microsoft.Win32;
using Solutionizer.Services;

namespace Solutionizer.Helper {
    public static class VisualStudioHelper {
        public static VisualStudioVersion DetectVersion() {
            using (var key = Registry.ClassesRoot.OpenSubKey("VisualStudio.DTE.16.0")) {
                if (key != null) {
                    return VisualStudioVersion.VS2019;
                }
            }
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
                case VisualStudioVersion.VS2019:
                    return "16.0";
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
            string installPath;
            switch (visualStudioVersion) {
                case VisualStudioVersion.VS2010:
                case VisualStudioVersion.VS2012:
                case VisualStudioVersion.VS2013:
                case VisualStudioVersion.VS2015: {
                    using (var hiveKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)) {
                        var regPath = string.Format(@"Software\Microsoft\VisualStudio\{0}", GetVersionKey(visualStudioVersion));
                        using (var key = hiveKey.OpenSubKey(regPath)) {
                            installPath = key.GetValue("InstallDir") as string;
                        }
                        break;
                    }
                }
                default: {
                    var setupInstances = new ISetupInstance[10];
                    new SetupConfiguration().EnumAllInstances().Next(setupInstances.Length, setupInstances, out var fetched);
                    var installedVSInstances = setupInstances.Take(fetched).Select((l, idx) => new {
                        MajorVersion = setupInstances[idx].GetInstallationVersion().Split('.').First(),
                        Path = setupInstances[idx].GetInstallationPath(),
                        InstallDay = setupInstances[idx].GetInstallDate().dwHighDateTime,
                    }).OrderByDescending(l => l.InstallDay).ToArray(); //Why: parallel early installed (Preview, RC) Versions
                    installPath = installedVSInstances.First(l => GetVersionKey(visualStudioVersion).StartsWith(l.MajorVersion)).Path;
                    installPath = Path.Combine(installPath, "Common7", "IDE");
                    break;
                }
            }
            return Path.Combine(installPath, "devenv.exe");
        }
    }
}