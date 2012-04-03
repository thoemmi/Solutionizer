using System.IO;
using NUnit.Framework;
using Solutionizer.Infrastructure;

namespace Solutionizer.Tests {
    [TestFixture]
    public class ScannerTests : ProjectTestBase {
        [Test]
        public void CanScanEmptyDirectory() {
            var root = Solutionizer.Infrastructure.ProjectRepository.Instance.GetProjects(_testDataPath);
            Assert.AreEqual(_testDataPath, root.FullPath);
            Assert.AreEqual(_testDataFolderName, root.Name);
            Assert.IsEmpty(root.Folders);
            Assert.IsEmpty(root.Projects);
        }

        [Test]
        public void EmptySubdirectoriesAreOmitted() {
            Directory.CreateDirectory(Path.Combine(_testDataPath, "YouShouldNotSeeMe"));

            var root = Solutionizer.Infrastructure.ProjectRepository.Instance.GetProjects(_testDataPath);
            Assert.AreEqual(_testDataPath, root.FullPath);
            Assert.AreEqual(_testDataFolderName, root.Name);
            Assert.IsEmpty(root.Folders);
            Assert.IsEmpty(root.Projects);
        }

        [Test]
        public void CanScanTwoProjectsInRoot() {
            CopyTestDataToPath("CsTestProject1.csproj", _testDataPath);
            CopyTestDataToPath("CsTestProject2.csproj", _testDataPath);

            var root = Solutionizer.Infrastructure.ProjectRepository.Instance.GetProjects(_testDataPath);
            Assert.AreEqual(_testDataPath, root.FullPath);
            Assert.AreEqual(_testDataFolderName, root.Name);
            Assert.IsEmpty(root.Folders);
            Assert.AreEqual(2, root.Projects.Count);
        }

        [Test]
        public void CanReadProjectInRoot() {
            CopyTestDataToPath("CsTestProject1.csproj", _testDataPath);

            var root = Solutionizer.Infrastructure.ProjectRepository.Instance.GetProjects(_testDataPath);

            Assert.AreEqual("CsTestProject1", root.Projects[0].Name);
            Assert.AreEqual(Path.Combine(_testDataPath, "CsTestProject1.csproj"), root.Projects[0].Filepath);
        }

        [Test]
        public void CanScanTwoProjectsInSubdirectories() {
            CopyTestDataToPath("CsTestProject1.csproj", Path.Combine(_testDataPath, "p1"));
            CopyTestDataToPath("CsTestProject2.csproj", Path.Combine(_testDataPath, "p2"));

            var root = Solutionizer.Infrastructure.ProjectRepository.Instance.GetProjects(_testDataPath);
            Assert.AreEqual(_testDataPath, root.FullPath);
            Assert.AreEqual(_testDataFolderName, root.Name);
            Assert.AreEqual(2, root.Projects.Count);
        }

        [Test]
        public void CanReadProjectInSubdirectory() {
            CopyTestDataToPath("CsTestProject1.csproj", Path.Combine(_testDataPath, "dir"));

            var root = Solutionizer.Infrastructure.ProjectRepository.Instance.GetProjects(_testDataPath);

            Assert.AreEqual(Path.Combine(_testDataPath, "dir", "CsTestProject1.csproj"), root.Projects[0].Filepath);
        }
    }
}