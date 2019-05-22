using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Solutionizer.Commands;
using Solutionizer.Models;
using Solutionizer.Services;
using Solutionizer.ViewModels;

namespace Solutionizer.Tests {
    [TestFixture]
    public class SaveSolutionCommandTests : ProjectTestBase {
        private readonly ISettings _settings;
        private readonly IVisualStudioInstallationsProvider _visualStudioInstallationsProvider = new DummyVisualStudioInstallationProvider();

        public SaveSolutionCommandTests() {
            _settings = new Settings {
                SimplifyProjectTree = true
            };
        }

        [Test]
        public void CanAddSaveSolution() {
            CopyTestDataToPath("CsTestProject1.csproj", _testDataPath);

            var scanningCommand = new ScanningCommand(_testDataPath, true);
            scanningCommand.Start().Wait();

            Project project;
            scanningCommand.Projects.TryGetValue(Path.Combine(_testDataPath, "CsTestProject1.csproj"), out project);

            var solution = new SolutionViewModel(new DummyStatusMessenger(), _settings, _visualStudioInstallationsProvider, _testDataPath, scanningCommand.Projects);
            solution.AddProject(project);

            var targetPath = Path.Combine(_testDataPath, "test.sln");

            var cmd = new SaveSolutionCommand(_settings, _visualStudioInstallationsProvider, targetPath, "VS2010", solution);
            cmd.Execute();

            Assert.AreEqual(ReadFromResource("CsTestProject1.sln"), File.ReadAllText(targetPath));
        }

        [Test]
        public void CanAddSaveSolutionWithProjectReferences() {
            CopyTestDataToPath("CsTestProject1.csproj", Path.Combine(_testDataPath, "p1"));
            CopyTestDataToPath("CsTestProject2.csproj", Path.Combine(_testDataPath, "p2"));

            var scanningCommand = new ScanningCommand(_testDataPath, true);
            scanningCommand.Start().Wait();

            Project project;
            scanningCommand.Projects.TryGetValue(Path.Combine(_testDataPath, "p2", "CsTestProject2.csproj"), out project);

            var solution = new SolutionViewModel(new DummyStatusMessenger(), _settings, _visualStudioInstallationsProvider, _testDataPath, scanningCommand.Projects);
            solution.AddProject(project);

            // we need to change the Guid of the reference folder
            solution.SolutionItems.OfType<SolutionFolder>().First().Guid = new Guid("{95374152-F021-4ABB-B317-74A183A89F00}");

            var targetPath = Path.Combine(_testDataPath, "test.sln");

            var cmd = new SaveSolutionCommand(_settings, _visualStudioInstallationsProvider, targetPath, "VS2010", solution);
            cmd.Execute();

            Assert.AreEqual(ReadFromResource("CsTestProject2.sln"), File.ReadAllText(targetPath));
        }

        [Test]
        public void CanAddSaveSolutionWithNestedProjectReferences() {
            CopyTestDataToPath("CsTestProject1.csproj", Path.Combine(_testDataPath, "sub", "p1"));
            CopyTestDataToPath("CsTestProject2.csproj", Path.Combine(_testDataPath, "sub", "p2"));
            CopyTestDataToPath("CsTestProject3.csproj", Path.Combine(_testDataPath, "p3", "sub"));

            var scanningCommand = new ScanningCommand(_testDataPath, true);
            scanningCommand.Start().Wait();

            Project project;
            scanningCommand.Projects.TryGetValue(Path.Combine(_testDataPath, "p3", "sub", "CsTestProject3.csproj"), out project);

            var solution = new SolutionViewModel(new DummyStatusMessenger(), _settings, _visualStudioInstallationsProvider, _testDataPath, scanningCommand.Projects);
            solution.AddProject(project);

            // we need to change the Guid of the reference folder
            var refFolder = solution.SolutionItems.OfType<SolutionFolder>().First();
            refFolder.Guid = new Guid("{95374152-F021-4ABB-B317-74A183A89F00}");
            refFolder.Items.OfType<SolutionFolder>().First().Guid = new Guid("{CE1BA3BF-4957-4CBC-9D45-3DC68106B311}");

            var targetPath = Path.Combine(_testDataPath, "test.sln");

            var cmd = new SaveSolutionCommand(_settings, _visualStudioInstallationsProvider, targetPath, "VS2010", solution);
            cmd.Execute();

            Assert.AreEqual(ReadFromResource("CsTestProject3.sln"), File.ReadAllText(targetPath));
        }

        [Test, Ignore("Don't know how to determine solution version yet")]
        public void CanWriteEmptyVs2017Solution()
        {
            var solution = new SolutionViewModel(new DummyStatusMessenger(), _settings, _visualStudioInstallationsProvider, _testDataPath, null);
            solution.SolutionId = Guid.Parse("07A0D871-36B7-441B-8091-5323BF63C5A5");

            var targetPath = Path.Combine(_testDataPath, $"{Guid.NewGuid()}.sln");

            var cmd = new SaveSolutionCommand(_settings, _visualStudioInstallationsProvider, targetPath, "cc0d24c6", solution);
            cmd.Execute();

            Assert.AreEqual(ReadFromResource("EmptyVs2017Solution.sln"), File.ReadAllText(targetPath));
        }

        [Test, Ignore("Don't know how to determine solution version yet")]
        public void CanWriteEmptyVs2019Solution() {
            var solution = new SolutionViewModel(new DummyStatusMessenger(), _settings, _visualStudioInstallationsProvider, _testDataPath, null);
            solution.SolutionId = Guid.Parse("BB2AB6FA-8028-47EA-99C9-5BFA5DE54864");

            var targetPath = Path.Combine(_testDataPath, $"{Guid.NewGuid()}.sln");

            var cmd = new SaveSolutionCommand(_settings, _visualStudioInstallationsProvider, targetPath, "21375828", solution);
            cmd.Execute();

            Assert.AreEqual(ReadFromResource("EmptyVs2019Solution.sln"), File.ReadAllText(targetPath));
        }
    }
}