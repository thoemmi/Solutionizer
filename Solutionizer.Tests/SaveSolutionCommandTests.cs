using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Solutionizer.Commands;
using Solutionizer.Infrastructure;
using Solutionizer.ViewModels;
using Solutionizer.VisualStudio;

namespace Solutionizer.Tests {
    [TestFixture]
    public class SaveSolutionCommandTests : ProjectTestBase {
        [Test]
        public void CanAddSaveSolution() {
            CopyTestDataToPath("CsTestProject1.csproj", _testDataPath);
            var project = Project.Load(Path.Combine(_testDataPath, "CsTestProject1.csproj"));

            var solution = new SolutionViewModel(_testDataPath);
            solution.AddProject(project);

            var targetPath = Path.Combine(_testDataPath, "test.sln");

            var cmd = new SaveSolutionCommand(targetPath, VisualStudioVersion.Vs2010, solution);
            cmd.Execute();

            Assert.AreEqual(ReadFromResource("CsTestProject1.sln"), File.ReadAllText(targetPath));
        }

        [Test]
        public void CanAddSaveSolutionWithProjectReferences() {
            CopyTestDataToPath("CsTestProject1.csproj", Path.Combine(_testDataPath, "p1"));
            CopyTestDataToPath("CsTestProject2.csproj", Path.Combine(_testDataPath, "p2"));
            var project = Project.Load(Path.Combine(_testDataPath, "p2", "CsTestProject2.csproj"));

            var solution = new SolutionViewModel(_testDataPath);
            solution.AddProject(project);

            // we need to change the Guid of the reference folder
            solution.SolutionRoot.Items.OfType<SolutionFolder>().First().Guid = new Guid("{95374152-F021-4ABB-B317-74A183A89F00}");

            var targetPath = Path.Combine(_testDataPath, "test.sln");

            var cmd = new SaveSolutionCommand(targetPath, VisualStudioVersion.Vs2010, solution);
            cmd.Execute();

            Assert.AreEqual(ReadFromResource("CsTestProject2.sln"), File.ReadAllText(targetPath));
        }

        [Test]
        public void CanAddSaveSolutionWithNestedProjectReferences() {
            CopyTestDataToPath("CsTestProject1.csproj", Path.Combine(_testDataPath, "sub", "p1"));
            CopyTestDataToPath("CsTestProject2.csproj", Path.Combine(_testDataPath, "sub", "p2"));
            CopyTestDataToPath("CsTestProject3.csproj", Path.Combine(_testDataPath, "p3", "sub"));
            var project = Project.Load(Path.Combine(_testDataPath, "p3", "sub", "CsTestProject3.csproj"));

            var solution = new SolutionViewModel(_testDataPath);
            solution.AddProject(project);

            // we need to change the Guid of the reference folder
            var refFolder = solution.SolutionRoot.Items.OfType<SolutionFolder>().First();
            refFolder.Guid = new Guid("{95374152-F021-4ABB-B317-74A183A89F00}");
            refFolder.Items.OfType<SolutionFolder>().First().Guid = new Guid("{CE1BA3BF-4957-4CBC-9D45-3DC68106B311}");

            var targetPath = Path.Combine(_testDataPath, "test.sln");

            var cmd = new SaveSolutionCommand(targetPath, VisualStudioVersion.Vs2010, solution);
            cmd.Execute();

            Assert.AreEqual(ReadFromResource("CsTestProject3.sln"), File.ReadAllText(targetPath));
        }
    }
}