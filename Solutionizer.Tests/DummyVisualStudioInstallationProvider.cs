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

                }
            };
        }

        public IReadOnlyList<VisualStudioInstallation> Installations { get; }
    }
}