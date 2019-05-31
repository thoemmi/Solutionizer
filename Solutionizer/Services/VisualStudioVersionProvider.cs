using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using NLog;

namespace Solutionizer.Services {
    public interface IVisualStudioInstallationsProvider {
        IReadOnlyList<IVisualStudioInstallation> Installations { get; }
    }

    public static class VisualStudioInstallationsProviderExtensions {
        public static IVisualStudioInstallation GetVisualStudioInstallationByVersionId(this IVisualStudioInstallationsProvider provider, string versionId) {
            return provider.Installations.FirstOrDefault(installation => installation.VersionId == versionId);
        }

        public static IVisualStudioInstallation GetMostRecentVisualStudioInstallation(this IVisualStudioInstallationsProvider provider) {
            return provider.Installations.OrderByDescending(installation => installation.Version).FirstOrDefault();
        }
    }

    public class VisualStudioInstallationsProvider : IVisualStudioInstallationsProvider {
        private readonly List<IVisualStudioInstallation> _installations = new List<IVisualStudioInstallation>();

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
                var process = new Process {
                    StartInfo = new ProcessStartInfo {
                        FileName = vswherePath,
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

        public IReadOnlyList<IVisualStudioInstallation> Installations => _installations;

        private static void AddInstallationIfExists(string name, string versionId, string versionKey, string solutionFileVersion, ICollection<IVisualStudioInstallation> installations) {
            using (var key = Registry.ClassesRoot.OpenSubKey($"VisualStudio.DTE.{versionKey}")) {
                if (key != null) {
                    installations.Add(new PreVisualStudio2017Installation {
                        Name = name,
                        Version = versionKey,
                        VersionId = versionId,
                        SolutionFileVersion = solutionFileVersion
                    });
                }
            }
        }
    }

    public interface IVisualStudioInstallation {
        string Name { get; }
        string Version { get; }
        string VersionId { get; }
        string SolutionVisualStudioVersion { get; }
        string SolutionFileVersion { get; }
        string DevEnvExePath { get; }
        string DefaultNewProjectLocation { get; }
        string SolutionComment { get; }
    }

    public class PreVisualStudio2017Installation : IVisualStudioInstallation {
        public string Name { get; set; }
        public string Version { get; set; }
        public string VersionId { get; set; }
        public string SolutionVisualStudioVersion => null;
        public string SolutionComment => Name;
        public string SolutionFileVersion { get; set; }

        public string DevEnvExePath
        {
            get
            {
                using (var hiveKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)) {
                    using (var key = hiveKey.OpenSubKey($@"Software\Microsoft\VisualStudio\{Version}")) {
                        if (key == null) {
                            throw new InvalidOperationException($@"Couldn't open registry key 'Software\Microsoft\VisualStudio\{Version}'");
                        }

                        var installPath = (string) key.GetValue("InstallDir");
                        return Path.Combine(installPath, "devenv.exe");
                    }
                }
            }
        }

        public string DefaultNewProjectLocation
        {
            get
            {
                using (var key = Registry.CurrentUser.OpenSubKey($@"Software\Microsoft\VisualStudio\{Version}")) {
                    if (key == null) {
                        throw new InvalidOperationException($@"Couldn't open registry key 'Software\Microsoft\VisualStudio\{Version}'");
                    }

                    return key.GetValue("DefaultNewProjectLocation") as string;
                }

            }
        }
    }

    public class VisualStudio2017AndFollowingInstallation : IVisualStudioInstallation {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public string Name { get; set; }
        public string Version { get; set; }
        public string VersionId { get; set; }
        public string SolutionVisualStudioVersion { get; set; }
        public string SolutionComment
        {
            get
            {
                var majorVersion = Version.Split('.').First();
                return majorVersion == "15" 
                    ? $"Visual Studio {majorVersion}"
                    : $"Visual Studio Version {majorVersion}";
            }
        }

        public string SolutionFileVersion { get; set; }
        public string InstallationPath { get; set; }
        public string DevEnvExePath { get; set; }

        public string DefaultNewProjectLocation
        {
            get
            {
                var process = new Process {
                    StartInfo = new ProcessStartInfo {
                        FileName = Path.Combine(InstallationPath, "Common7", "IDE", "vsregedit.exe"),
                        Arguments = $@"read ""{InstallationPath}"" HKCU """" DefaultNewProjectLocation string",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    }
                };

                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(30000);

                if (process.ExitCode != 0) {
                    _log.Error($"Could not read 'DefaultNewProjectLocation' for {Name}, vsregedit returned {process.ExitCode}, output: {Environment.NewLine}{output}");
                    return null;
                }

                if (!output.StartsWith("Name: DefaultNewProjectLocation, Value:")) {
                    _log.Error($"Could not read 'DefaultNewProjectLocation' for {Name}, output of vsregedit was:{Environment.NewLine}{output}");
                    return null;
                }

                return output.Substring("Name: DefaultNewProjectLocation, Value:".Length).Trim();
            }
        }
    }
}
