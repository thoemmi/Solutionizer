using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace Solutionizer.Services {
    public interface IVisualStudioInstallationsProvider {
        IReadOnlyList<VisualStudioInstallation> Installations { get; }
        VisualStudioInstallation GetVisualStudioInstallationByVersionId(string versionId);
        VisualStudioInstallation GetMostRecentVisualStudioInstallation();
    }

    public class VisualStudioInstallationsProvider : IVisualStudioInstallationsProvider {
        private readonly List<VisualStudioInstallation> _installations = new List<VisualStudioInstallation>();

        public VisualStudioInstallationsProvider() {
            AddInstallationIfExists("Visual Studio 2010", "VS2010", "10.0", _installations);
            AddInstallationIfExists("Visual Studio 2012", "VS2012", "11.0", _installations);
            AddInstallationIfExists("Visual Studio 2013", "VS2013", "12.0", _installations);
            AddInstallationIfExists("Visual Studio 2015", "VS2015", "14.0", _installations);

            // TODO: use vswhere to read VS2017 and newer instalations
            AddInstallationIfExists("Visual Studio 2017", "VS2017", "15.0", _installations);
        }

        public VisualStudioInstallation GetVisualStudioInstallationByVersionId(string versionId) {
            return _installations.FirstOrDefault(installation => installation.VersionId == versionId);
        }

        public VisualStudioInstallation GetMostRecentVisualStudioInstallation() {
            return _installations.OrderByDescending(installation => installation.Version).FirstOrDefault();
        }

        public IReadOnlyList<VisualStudioInstallation> Installations => _installations;

        private static void AddInstallationIfExists(string name, string versionId, string versionKey, ICollection<VisualStudioInstallation> installations) {
            using (var key = Registry.ClassesRoot.OpenSubKey($"VisualStudio.DTE.{versionKey}"))
            {
                if (key != null)
                {
                    installations.Add(new VisualStudioInstallation
                    {
                        Name = name,
                        Version = versionKey,
                        VersionId = versionId,
                        VersionKey = versionKey,
                        InstallationPath = GetVisualStudioExecutable(versionKey),
                        ProjectsLocation = GetDefaultProjectsLocation(versionKey)
                    });
                }
            }

        }

        private static string GetVisualStudioExecutable(string versionKey)
        {
            using (var hiveKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)) {
                if (versionKey == "15.0") {
                    //TODO maybe use https://www.nuget.org/packages/Microsoft.VisualStudio.Setup.Configuration.Interop/ to detect multiple parallel installed VS2017 instances
                    var regPath = @"Software\WOW6432Node\Microsoft\VisualStudio\SxS\VS7";
                    using (var key = hiveKey.OpenSubKey(regPath))
                    {
                        var installPath = key.GetValue("15.0") as string;
                        return Path.Combine(installPath, @"Common7\IDE\devenv.exe");
                    }
                }
                else {
                    using (var key = hiveKey.OpenSubKey($@"Software\Microsoft\VisualStudio\{versionKey}")) {
                        var installPath = key.GetValue("InstallDir") as string;
                        return Path.Combine(installPath, "devenv.exe");
                    }
                }
            }
        }

        private static string GetDefaultProjectsLocation(string versionKey) {
            using (var key = Registry.CurrentUser.OpenSubKey($@"Software\Microsoft\VisualStudio\{versionKey}")) {
                if (key != null) {
                    return key.GetValue("DefaultNewProjectLocation") as string;
                }
            }

            return null;
        }

    }

    public class VisualStudioInstallation {
        public string Name { get; set; }
        public string Version { get; set; }
        public string VersionId { get; set; }
        public string VersionKey { get; set; }
        public string InstallationPath { get; set; }
        public string ProjectsLocation { get; set; }
    }
}