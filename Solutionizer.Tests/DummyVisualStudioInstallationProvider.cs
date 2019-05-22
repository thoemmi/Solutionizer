using System.Collections.Generic;
using System.IO;
using Solutionizer.Services;

namespace Solutionizer.Tests {
    public class DummyVisualStudioInstallationProvider : IVisualStudioInstallationsProvider {
        private static readonly List<IVisualStudioInstallation> _installations;

        static DummyVisualStudioInstallationProvider() {
            _installations = new List<IVisualStudioInstallation> {
                new PreVisualStudio2017Installation {
                    Name = "Visual Studio 2010",
                    VersionId = "VS2010",
                    Version = "10.0",
                    SolutionFileVersion = "11.00"
                }
            };

            var stream = typeof(ProjectTestBase).Assembly.GetManifestResourceStream("Solutionizer.Tests.TestData.vswhere.json");
            using (var input = new StreamReader(stream)) {
                var json = input.ReadToEnd();
                _installations.AddRange(VsWhereOutputParser.Parse(json));
            }
        }

        public IReadOnlyList<IVisualStudioInstallation> Installations => _installations;
    }
}
