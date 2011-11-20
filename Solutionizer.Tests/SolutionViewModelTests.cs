using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Solutionizer.Commands;
using Solutionizer.ViewModels;

namespace Solutionizer.Tests {
    [TestFixture]
    public class SolutionViewModelTests : ProjectTestBase {
        [Test]
        public void CanAddProject() {
            CopyTestDataToPath("CsTestProject1.csproj", _testDataPath);
            var project = Project.Load(Path.Combine(_testDataPath, "CsTestProject1.csproj"));

            var sut = new SolutionViewModel();
            sut.AddProject(project);

            Assert.AreEqual(1, sut.SolutionFolder.Items.Count);
            Assert.AreEqual("CsTestProject1", sut.SolutionFolder.Items[0].Name);
            Assert.AreEqual(typeof(SolutionProject), sut.SolutionFolder.Items[0].GetType());
        }

        [Test]
        public void CanAddProjectWithProjectReference() {
            CopyTestDataToPath("CsTestProject1.csproj", _testDataPath);
            CopyTestDataToPath("CsTestProject2.csproj", _testDataPath);
            var project = Project.Load(Path.Combine(_testDataPath, "CsTestProject2.csproj"));

            var sut = new SolutionViewModel();
            sut.AddProject(project);

            Assert.AreEqual(2, sut.SolutionFolder.Items.Count);
            Assert.AreEqual("CsTestProject2", sut.SolutionFolder.Items[0].Name);
            Assert.AreEqual("References", sut.SolutionFolder.Items[1].Name);

            Assert.AreEqual(1, ((SolutionFolder)sut.SolutionFolder.Items[1]).Items.Count);
            Assert.AreEqual("CsTestProject1", ((SolutionFolder)sut.SolutionFolder.Items[1]).Items[0].Name);
        }

        [Test]
        public void CanAddSaveSolution() {
            CopyTestDataToPath("CsTestProject1.csproj", _testDataPath);
            var project = Project.Load(Path.Combine(_testDataPath, "CsTestProject1.csproj"));

            var solution = new SolutionViewModel();
            solution.AddProject(project);

            var targetPath = Path.Combine(_testDataPath, "test.sln");

            var cmd = new SaveSolutionCommand(targetPath, solution);
            cmd.Execute();

            Assert.AreEqual(ReadFromResource("CsTestProject1.sln"), File.ReadAllText(targetPath));
        }

        [Test]
        public void CanAddSaveSolutionWithProjectReferences() {
            CopyTestDataToPath("CsTestProject1.csproj", _testDataPath);
            CopyTestDataToPath("CsTestProject2.csproj", _testDataPath);
            var project = Project.Load(Path.Combine(_testDataPath, "CsTestProject2.csproj"));

            var solution = new SolutionViewModel();
            solution.AddProject(project);

            // we need to change the Guid of the reference folder
            solution.SolutionFolder.Items.OfType<SolutionFolder>().First().Guid = new Guid("{95374152-F021-4ABB-B317-74A183A89F00}");

            var targetPath = Path.Combine(_testDataPath, "test.sln");

            var cmd = new SaveSolutionCommand(targetPath, solution);
            cmd.Execute();

            Assert.AreEqual(ReadFromResource("CsTestProject2.sln"), File.ReadAllText(targetPath));
        }
    }
}