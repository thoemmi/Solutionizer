using System.IO;
using NUnit.Framework;
using Solutionizer.ViewModels;

namespace Solutionizer.Tests {
    [TestFixture]
    public class ScannerTests : ProjectTestBase {
        [Test]
        public void CanScanEmptyDirectory() {
            var scanningCommand = new ScanningCommand(_testDataPath, true);
            var scanResult = scanningCommand.Start().Result;

            Assert.AreEqual(_testDataPath, scanResult.ProjectFolder.FullPath);
            Assert.AreEqual(_testDataFolderName, scanResult.ProjectFolder.Name);
            Assert.IsEmpty(scanResult.ProjectFolder.Folders);
            Assert.IsEmpty(scanResult.ProjectFolder.Projects);
        }

        [Test]
        public void EmptySubdirectoriesAreOmitted() {
            Directory.CreateDirectory(Path.Combine(_testDataPath, "YouShouldNotSeeMe"));

            var scanningCommand = new ScanningCommand(_testDataPath, true);
            var scanResult = scanningCommand.Start().Result;

            Assert.AreEqual(_testDataPath, scanResult.ProjectFolder.FullPath);
            Assert.AreEqual(_testDataFolderName, scanResult.ProjectFolder.Name);
            Assert.IsEmpty(scanResult.ProjectFolder.Folders);
            Assert.IsEmpty(scanResult.ProjectFolder.Projects);
        }

        [Test]
        public void CanScanTwoProjectsInRoot() {
            CopyTestDataToPath("CsTestProject1.csproj", _testDataPath);
            CopyTestDataToPath("CsTestProject2.csproj", _testDataPath);

            var scanningCommand = new ScanningCommand(_testDataPath, true);
            var scanResult = scanningCommand.Start().Result;

            Assert.AreEqual(_testDataPath, scanResult.ProjectFolder.FullPath);
            Assert.AreEqual(_testDataFolderName, scanResult.ProjectFolder.Name);
            Assert.IsEmpty(scanResult.ProjectFolder.Folders);
            Assert.AreEqual(2, scanResult.ProjectFolder.Projects.Count);
        }

        [Test]
        public void CanReadProjectInRoot() {
            CopyTestDataToPath("CsTestProject1.csproj", _testDataPath);

            var scanningCommand = new ScanningCommand(_testDataPath, true);
            var scanResult = scanningCommand.Start().Result;

            Assert.AreEqual("CsTestProject1", scanResult.ProjectFolder.Projects[0].Name);
            Assert.AreEqual(Path.Combine(_testDataPath, "CsTestProject1.csproj"), scanResult.ProjectFolder.Projects[0].Filepath);
        }

        [Test]
        public void CanScanTwoProjectsInSubdirectories() {
            CopyTestDataToPath("CsTestProject1.csproj", Path.Combine(_testDataPath, "p1"));
            CopyTestDataToPath("CsTestProject2.csproj", Path.Combine(_testDataPath, "p2"));

            var scanningCommand = new ScanningCommand(_testDataPath, true);
            var scanResult = scanningCommand.Start().Result;

            Assert.AreEqual(_testDataPath, scanResult.ProjectFolder.FullPath);
            Assert.AreEqual(_testDataFolderName, scanResult.ProjectFolder.Name);
            Assert.AreEqual(2, scanResult.ProjectFolder.Projects.Count);
        }

        [Test]
        public void CanReadProjectInSubdirectory() {
            CopyTestDataToPath("CsTestProject1.csproj", Path.Combine(_testDataPath, "dir"));

            var scanningCommand = new ScanningCommand(_testDataPath, true);
            var scanResult = scanningCommand.Start().Result;

            Assert.AreEqual(Path.Combine(_testDataPath, "dir", "CsTestProject1.csproj"), scanResult.ProjectFolder.Projects[0].Filepath);
        }
    }
}