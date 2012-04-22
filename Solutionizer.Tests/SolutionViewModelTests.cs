using System.IO;
using NUnit.Framework;
using Solutionizer.FileScanning;
using Solutionizer.Models;
using Solutionizer.Services;
using Solutionizer.Solution;
using Solutionizer.VisualStudio;

namespace Solutionizer.Tests {
    [TestFixture]
    public class SolutionViewModelTests : ProjectTestBase {
        private readonly ISettings _settings;

        public SolutionViewModelTests() {
            _settings = new Services.Settings {
                SimplifyProjectTree = true
            };
        }

        [Test]
        public void CanAddProject() {
            CopyTestDataToPath("CsTestProject1.csproj", _testDataPath);

            _fileScanner = new FileScanningViewModel(_settings);
            _fileScanner.Path = _testDataPath;
            _fileScanner.LoadProjects();

            WaitForProjectLoaded(_fileScanner);

            Project project;
            _fileScanner.Projects.TryGetValue(Path.Combine(_testDataPath, "CsTestProject1.csproj"), out project);

            var sut = new SolutionViewModel(_settings, _testDataPath, _fileScanner.Projects);
            sut.AddProject(project);

            Assert.AreEqual(1, sut.SolutionItems.Count);
            Assert.AreEqual("CsTestProject1", sut.SolutionItems[0].Name);
            Assert.AreEqual(typeof (SolutionProject), sut.SolutionItems[0].GetType());
        }

        [Test]
        public void CanAddProjectWithProjectReference() {
            CopyTestDataToPath("CsTestProject1.csproj", Path.Combine(_testDataPath, "p1"));
            CopyTestDataToPath("CsTestProject2.csproj", Path.Combine(_testDataPath, "p2"));

            _fileScanner = new FileScanningViewModel(_settings);
            _fileScanner.Path = _testDataPath;
            _fileScanner.LoadProjects();

            WaitForProjectLoaded(_fileScanner);

            Project project;
            _fileScanner.Projects.TryGetValue(Path.Combine(_testDataPath, "p2", "CsTestProject2.csproj"), out project);

            var sut = new SolutionViewModel(_settings, _testDataPath, _fileScanner.Projects);
            sut.AddProject(project);

            Assert.AreEqual(2, sut.SolutionItems.Count);
            Assert.AreEqual("_References", sut.SolutionItems[0].Name);
            Assert.AreEqual("CsTestProject2", sut.SolutionItems[1].Name);

            Assert.AreEqual(1, ((SolutionFolder) sut.SolutionItems[0]).Items.Count);
            Assert.AreEqual("CsTestProject1", ((SolutionFolder) sut.SolutionItems[0]).Items[0].Name);
        }
    }
}