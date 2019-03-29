using System.Collections.Generic;
using Solutionizer.Services;

namespace Solutionizer.Tests {
    public class DummyVisualStudioInstallationProvider : IVisualStudioInstallationsProvider {
        public DummyVisualStudioInstallationProvider() {
            Installations = new List<VisualStudioInstallation> {
                new VisualStudioInstallation {
                    Name = "Visual Studio 2010",
                    VersionId = "VS2010",
                    Version = "10.0",
                    SolutionFileVersion = "11.00"
                },
                new VisualStudioInstallation {
                    Name = "Visual Studio 2019 Preview",
                    VersionId = "VS2019",
                    Version = "16.0",
                    SolutionFileVersion = "12.00",
                    SolutionVisualStudioVersion = "16.0.28721.148",
                },
                new VisualStudioInstallation {
                    Name = "Visual Studio 2017",
                    VersionId = "VS2017",
                    Version = "16.0",
                    SolutionFileVersion = "12.00",
                    SolutionVisualStudioVersion = "15.9.28307.557",
                },
            };
        }

        public IReadOnlyList<VisualStudioInstallation> Installations { get; }
    }
}