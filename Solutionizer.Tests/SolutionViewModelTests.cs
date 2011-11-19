using System.IO;
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

            Assert.AreEqual(1, sut.Projects.Count);
            Assert.AreEqual("CsTestProject1", sut.Projects[0].Name);

            Assert.IsEmpty(sut.ReferencedProjects);
        }

        [Test]
        public void CanAddProjectWithProjectReference() {
            CopyTestDataToPath("CsTestProject1.csproj", _testDataPath);
            CopyTestDataToPath("CsTestProject2.csproj", _testDataPath);
            var project = Project.Load(Path.Combine(_testDataPath, "CsTestProject2.csproj"));

            var sut = new SolutionViewModel();
            sut.AddProject(project);

            Assert.AreEqual(1, sut.Projects.Count);
            Assert.AreEqual("CsTestProject2", sut.Projects[0].Name);

            Assert.AreEqual(1, sut.ReferencedProjects.Count);
            Assert.AreEqual("CsTestProject1", sut.ReferencedProjects[0].Name);
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

            var targetPath = Path.Combine(_testDataPath, "test.sln");

            var cmd = new SaveSolutionCommand(targetPath, solution);
            cmd.Execute();

            Assert.AreEqual(ReadFromResource("CsTestProject2.sln"), File.ReadAllText(targetPath));
        }
    }
}