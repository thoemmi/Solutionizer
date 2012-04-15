using System.IO;
using NUnit.Framework;
using Solutionizer.FileScanning;

namespace Solutionizer.Tests {
    [TestFixture]
    public class ScannerTests : ProjectTestBase {
        [Test]
        public void CanScanEmptyDirectory() {
            _fileScanner = new FileScanningViewModel();
            _fileScanner.Path = _testDataPath;
            _fileScanner.LoadProjects();

            Assert.AreEqual(_testDataPath, _fileScanner.ProjectFolder.FullPath);
            Assert.AreEqual(_testDataFolderName, _fileScanner.ProjectFolder.Name);
            Assert.IsEmpty(_fileScanner.ProjectFolder.Folders);
            Assert.IsEmpty(_fileScanner.ProjectFolder.Projects);
        }

        [Test]
        public void EmptySubdirectoriesAreOmitted() {
            Directory.CreateDirectory(Path.Combine(_testDataPath, "YouShouldNotSeeMe"));

            _fileScanner = new FileScanningViewModel();
            _fileScanner.Path = _testDataPath;
            _fileScanner.LoadProjects();

            Assert.AreEqual(_testDataPath, _fileScanner.ProjectFolder.FullPath);
            Assert.AreEqual(_testDataFolderName, _fileScanner.ProjectFolder.Name);
            Assert.IsEmpty(_fileScanner.ProjectFolder.Folders);
            Assert.IsEmpty(_fileScanner.ProjectFolder.Projects);
        }

        [Test]
        public void CanScanTwoProjectsInRoot() {
            CopyTestDataToPath("CsTestProject1.csproj", _testDataPath);
            CopyTestDataToPath("CsTestProject2.csproj", _testDataPath);

            _fileScanner = new FileScanningViewModel();
            _fileScanner.Path = _testDataPath;
            _fileScanner.LoadProjects();

            Assert.AreEqual(_testDataPath, _fileScanner.ProjectFolder.FullPath);
            Assert.AreEqual(_testDataFolderName, _fileScanner.ProjectFolder.Name);
            Assert.IsEmpty(_fileScanner.ProjectFolder.Folders);
            Assert.AreEqual(2, _fileScanner.ProjectFolder.Projects.Count);
        }

        [Test]
        public void CanReadProjectInRoot() {
            CopyTestDataToPath("CsTestProject1.csproj", _testDataPath);

            _fileScanner = new FileScanningViewModel();
            _fileScanner.Path = _testDataPath;
            _fileScanner.LoadProjects();

            Assert.AreEqual("CsTestProject1", _fileScanner.ProjectFolder.Projects[0].Name);
            Assert.AreEqual(Path.Combine(_testDataPath, "CsTestProject1.csproj"), _fileScanner.ProjectFolder.Projects[0].Filepath);
        }

        [Test]
        public void CanScanTwoProjectsInSubdirectories() {
            CopyTestDataToPath("CsTestProject1.csproj", Path.Combine(_testDataPath, "p1"));
            CopyTestDataToPath("CsTestProject2.csproj", Path.Combine(_testDataPath, "p2"));

            _fileScanner = new FileScanningViewModel();
            _fileScanner.Path = _testDataPath;
            _fileScanner.LoadProjects();

            Assert.AreEqual(_testDataPath, _fileScanner.ProjectFolder.FullPath);
            Assert.AreEqual(_testDataFolderName, _fileScanner.ProjectFolder.Name);
            Assert.AreEqual(2, _fileScanner.ProjectFolder.Projects.Count);
        }

        [Test]
        public void CanReadProjectInSubdirectory() {
            CopyTestDataToPath("CsTestProject1.csproj", Path.Combine(_testDataPath, "dir"));

            _fileScanner = new FileScanningViewModel();
            _fileScanner.Path = _testDataPath;
            _fileScanner.LoadProjects();

            Assert.AreEqual(Path.Combine(_testDataPath, "dir", "CsTestProject1.csproj"), _fileScanner.ProjectFolder.Projects[0].Filepath);
        }
    }
}