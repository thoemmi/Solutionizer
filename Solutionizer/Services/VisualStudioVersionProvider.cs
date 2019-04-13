using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using Newtonsoft.Json;
using NLog;

namespace Solutionizer.Services {
    public interface IVisualStudioInstallationsProvider {
        IReadOnlyList<VisualStudioInstallation> Installations { get; }
    }

    public static class VisualStudioInstallationsProviderExtensions {
        public static VisualStudioInstallation GetVisualStudioInstallationByVersionId(this IVisualStudioInstallationsProvider provider, string versionId)
        {
            return provider.Installations.FirstOrDefault(installation => installation.VersionId == versionId);
        }

        public static VisualStudioInstallation GetMostRecentVisualStudioInstallation(this IVisualStudioInstallationsProvider provider)
        {
            return provider.Installations.OrderByDescending(installation => installation.Version).FirstOrDefault();
        }
    }

    public class VisualStudioInstallationsProvider : IVisualStudioInstallationsProvider {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly List<VisualStudioInstallation> _installations = new List<VisualStudioInstallation>();

        public VisualStudioInstallationsProvider() {
            AddInstallationIfExists("Visual Studio 2010", "VS2010", "10.0", "11.00", _installations);
            AddInstallationIfExists("Visual Studio 2012", "VS2012", "11.0", "12.00", _installations);
            AddInstallationIfExists("Visual Studio 2013", "VS2013", "12.0", "12.00", _installations);
            AddInstallationIfExists("Visual Studio 2015", "VS2015", "14.0", "14.00", _installations);

            var vswherePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Microsoft Visual Studio",
                "Installer",
                "vswhere.exe");
            if (File.Exists(vswherePath)) {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = @".\vswhere.exe",
                        Arguments = @"-nologo -prerelease -format json -utf8",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    }
                };

                // Run vswhere.exe and wait for its termination.
                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(30000);

                if (process.ExitCode == 0) {
                    _installations.AddRange(VsWhereOutputParser.Parse(output));
                }
            }
        }

        public IReadOnlyList<VisualStudioInstallation> Installations => _installations;

        private static void AddInstallationIfExists(string name, string versionId, string versionKey, string solutionFileVersion, ICollection<VisualStudioInstallation> installations) {
            using (var key = Registry.ClassesRoot.OpenSubKey($"VisualStudio.DTE.{versionKey}"))
            {
                if (key != null)
                {
                    installations.Add(new VisualStudioInstallation
                    {
                        Name = name,
                        Version = versionKey,
                        VersionId = versionId,
                        SolutionFileVersion = solutionFileVersion,
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
        public string SolutionVisualStudioVersion { get; set; }
        public string SolutionFileVersion { get; set; }
        public string InstallationPath { get; set; }
        public string ProjectsLocation { get; set; }
    }
}