using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NLog;

namespace Solutionizer.Services {
    public static class VsWhereOutputParser {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public static IEnumerable<IVisualStudioInstallation> Parse(string json) {
            try {
                var vsWhereInstallations = JsonConvert.DeserializeObject<VsWhereInstallation[]>(json);
                return vsWhereInstallations.Select(inst => new VisualStudio2017AndFollowingInstallation
                {
                    Name = inst.DisplayName + (inst.Catalog.ProductMilestone == "RTW" ? "" : $" {inst.Catalog.ProductMilestone}"),
                    VersionId = inst.InstanceId,
                    Version = inst.InstallationVersion,
                    InstallationPath = inst.InstallationPath,
                    DevEnvExePath = inst.ProductPath,
                    SolutionFileVersion = "12",
                    SolutionVisualStudioVersion = inst.InstallationVersion
                });
            } catch (Exception ex) {
                _log.Error(ex, "Deserializing output from vswhere.exe failed");
                return Enumerable.Empty<IVisualStudioInstallation>();
            }
        }

        private class VsWhereInstallation {
            public string InstanceId { get; set; }
            public string DisplayName { get; set; }
            public string InstallationVersion { get; set; }
            public string InstallationPath { get; set; }
            public string ProductPath { get; set; }
            public CatalogInfo Catalog { get; set; }
        }

        private class CatalogInfo {
            public string ProductName { get; set; }
            public string ProductLineVersion { get; set; }
            public string ProductDisplayVersion { get; set; }
            public string ProductMilestone { get; set; }
        }
    }
}