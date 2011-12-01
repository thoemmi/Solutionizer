using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Solutionizer.Commands;
using Solutionizer.Models;
using Solutionizer.VisualStudio;

namespace Solutionizer.Tests {
    [TestFixture]
    public class SaveSolutionCommmandTests : ProjectTestBase {
        [Test]
        public void CanAddSaveSolution() {
            CopyTestDataToPath("CsTestProject1.csproj", _testDataPath);
            var project = Project.Load(Path.Combine(_testDataPath, "CsTestProject1.csproj"));

            var solution = new SolutionViewModel();
            solution.CreateSolution(_testDataPath);
            solution.AddProject(project);

            var targetPath = Path.Combine(_testDataPath, "test.sln");

            var cmd = new SaveSolutionCommand(targetPath, solution);
            cmd.Execute();

            Assert.AreEqual(ReadFromResource("CsTestProject1.sln"), File.ReadAllText(targetPath));
        }

        [Test]
        public void CanAddSaveSolutionWithProjectReferences() {
            CopyTestDataToPath("CsTestProject1.csproj", _testDataPath + @"\p1");
            CopyTestDataToPath("CsTestProject2.csproj", _testDataPath + @"\p2");
            var project = Project.Load(Path.Combine(_testDataPath + @"\p2", "CsTestProject2.csproj"));

            var solution = new SolutionViewModel();
            solution.CreateSolution(_testDataPath);
            solution.AddProject(project);

            // we need to change the Guid of the reference folder
            solution.SolutionRoot.Items.OfType<SolutionFolder>().First().Guid = new Guid("{95374152-F021-4ABB-B317-74A183A89F00}");

            var targetPath = Path.Combine(_testDataPath, "test.sln");

            var cmd = new SaveSolutionCommand(targetPath, solution);
            cmd.Execute();

            Assert.AreEqual(ReadFromResource("CsTestProject2.sln"), File.ReadAllText(targetPath));
        }
    }
}