using System.IO;
using NUnit.Framework;
using Solutionizer.Models;
using Solutionizer.Services;
using Solutionizer.ViewModels;

namespace Solutionizer.Tests {
    [TestFixture]
    public class SolutionViewModelTests : ProjectTestBase {
        private readonly ISettings _settings;
        private readonly IVisualStudioInstallationsProvider _visualStudioInstallationsProvider = new DummyVisualStudioInstallationProvider();

        public SolutionViewModelTests() {
            _settings = new Settings {
                SimplifyProjectTree = true
            };
        }

        [Test]
        public void CanAddProject() {
            CopyTestDataToPath("CsTestProject1.csproj", _testDataPath);

            var scanningCommand = new ScanningCommand(_testDataPath, true);
            scanningCommand.Start().Wait();

            Project project;
            scanningCommand.Projects.TryGetValue(Path.Combine(_testDataPath, "CsTestProject1.csproj"), out project);

            var sut = new SolutionViewModel(new DummyStatusMessenger(), _settings, _visualStudioInstallationsProvider, _testDataPath, scanningCommand.Projects);
            sut.AddProject(project);

            Assert.AreEqual(1, sut.SolutionItems.Count);
            Assert.AreEqual("CsTestProject1", sut.SolutionItems[0].Name);
            Assert.AreEqual(typeof (SolutionProject), sut.SolutionItems[0].GetType());
        }

        [Test]
        public void CanAddProjectWithProjectReference() {
            CopyTestDataToPath("CsTestProject1.csproj", Path.Combine(_testDataPath, "p1"));
            CopyTestDataToPath("CsTestProject2.csproj", Path.Combine(_testDataPath, "p2"));

            var scanningCommand = new ScanningCommand(_testDataPath, true);
            scanningCommand.Start().Wait();

            Project project;
            scanningCommand.Projects.TryGetValue(Path.Combine(_testDataPath, "p2", "CsTestProject2.csproj"), out project);

            var sut = new SolutionViewModel(new DummyStatusMessenger(), _settings, _visualStudioInstallationsProvider, _testDataPath, scanningCommand.Projects);
            sut.AddProject(project);

            Assert.AreEqual(2, sut.SolutionItems.Count);
            Assert.AreEqual("_References", sut.SolutionItems[0].Name);
            Assert.AreEqual("CsTestProject2", sut.SolutionItems[1].Name);

            Assert.AreEqual(1, ((SolutionFolder) sut.SolutionItems[0]).Items.Count);
            Assert.AreEqual("CsTestProject1", ((SolutionFolder) sut.SolutionItems[0]).Items[0].Name);
        }
    }
}