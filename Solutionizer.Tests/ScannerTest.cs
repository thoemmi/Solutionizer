using System.IO;
using NUnit.Framework;
using Solutionizer.Scanner;

namespace Solutionizer.Tests {
    [TestFixture]
    public class ScannerTest : ProjectTestBase {
        [Test]
        public void CanScanEmptyDirectory() {
            var root = ProjectScanner.Scan(_testDataPath);
            Assert.AreEqual(_testDataPath, root.Path);
            Assert.AreEqual(_testDataPath, root.Name);
            Assert.IsEmpty(root.Subdirectories);
            Assert.IsEmpty(root.Files);
        }

        [Test]
        public void EmptySubdirectoriesAreOmitted() {
            Directory.CreateDirectory(Path.Combine(_testDataPath, "YouShouldNotSeeMe"));

            var root = ProjectScanner.Scan(_testDataPath);
            Assert.AreEqual(_testDataPath, root.Path);
            Assert.AreEqual(_testDataPath, root.Name);
            Assert.IsEmpty(root.Subdirectories);
            Assert.IsEmpty(root.Files);
        }

        [Test]
        public void CanScanTwoProjectsInRoot() {
            CopyTestDataToPath("CsTestProject1.csproj", _testDataPath);
            CopyTestDataToPath("CsTestProject2.csproj", _testDataPath);

            var root = ProjectScanner.Scan(_testDataPath);
            Assert.AreEqual(_testDataPath, root.Path);
            Assert.AreEqual(_testDataPath, root.Name);
            Assert.IsEmpty(root.Subdirectories);
            Assert.AreEqual(2, root.Files.Count);
        }

        [Test]
        public void CanReadProjectInRoot() {
            CopyTestDataToPath("CsTestProject1.csproj", _testDataPath);

            var root = ProjectScanner.Scan(_testDataPath);

            Assert.AreEqual("CsTestProject1", root.Files[0].Name);
            Assert.AreEqual(Path.Combine(_testDataPath, "CsTestProject1.csproj"), root.Files[0].Path);
        }

        [Test]
        public void CanScanTwoProjectsInSubdirectories() {
            CopyTestDataToPath("CsTestProject1.csproj", Path.Combine(_testDataPath, "p1"));
            CopyTestDataToPath("CsTestProject2.csproj", Path.Combine(_testDataPath, "p2"));

            var root = ProjectScanner.Scan(_testDataPath);
            Assert.AreEqual(_testDataPath, root.Path);
            Assert.AreEqual(_testDataPath, root.Name);
            Assert.AreEqual(2, root.Subdirectories.Count);
            Assert.IsEmpty(root.Files);
        }

        [Test]
        public void CanReadProjectInSubdirectory() {
            CopyTestDataToPath("CsTestProject1.csproj", Path.Combine(_testDataPath, "dir"));

            var root = ProjectScanner.Scan(_testDataPath);

            Assert.AreEqual("CsTestProject1", root.Subdirectories[0].Files[0].Name);
            Assert.AreEqual(Path.Combine(_testDataPath, "dir", "CsTestProject1.csproj"), root.Subdirectories[0].Files[0].Path);
        }
    }
}