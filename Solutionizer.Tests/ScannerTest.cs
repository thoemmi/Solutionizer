using System.IO;
using NUnit.Framework;
using Solutionizer.Scanner;

namespace Solutionizer.Tests {
    [TestFixture]
    public class ScannerTest : ProjectTestBase {
        [Test]
        public void CanScanEmptyDirectory() {
            var scanner = new ProjectScanner();
            var root = scanner.Scan(_testDataPath);
            Assert.AreEqual(_testDataPath, root.Path);
            Assert.AreEqual(_testDataPath, root.Name);
            Assert.IsEmpty(root.Subdirectories);
            Assert.IsEmpty(root.Files);
        }

        [Test]
        public void EmptySubdirectoriesAreOmitted() {
            Directory.CreateDirectory(Path.Combine(_testDataPath, "YouShouldNotSeeMe"));

            var scanner = new ProjectScanner();
            var root = scanner.Scan(_testDataPath);
            Assert.AreEqual(_testDataPath, root.Path);
            Assert.AreEqual(_testDataPath, root.Name);
            Assert.IsEmpty(root.Subdirectories);
            Assert.IsEmpty(root.Files);
        }

        [Test]
        public void CanScanTwoProjectsInRoot() {
            CopyTestDataToPath("CsTestProject1.csproj", _testDataPath);
            CopyTestDataToPath("CsTestProject2.csproj", _testDataPath);

            var scanner = new ProjectScanner();
            var root = scanner.Scan(_testDataPath);
            Assert.AreEqual(_testDataPath, root.Path);
            Assert.AreEqual(_testDataPath, root.Name);
            Assert.IsEmpty(root.Subdirectories);
            Assert.AreEqual(2, root.Files.Count);
        }

        [Test]
        public void CanScanTwoProjectsInSubdirectories() {
            CopyTestDataToPath("CsTestProject1.csproj", Path.Combine(_testDataPath, "p1"));
            CopyTestDataToPath("CsTestProject2.csproj", Path.Combine(_testDataPath, "p2"));

            var scanner = new ProjectScanner();
            var root = scanner.Scan(_testDataPath);
            Assert.AreEqual(_testDataPath, root.Path);
            Assert.AreEqual(_testDataPath, root.Name);
            Assert.AreEqual(2, root.Subdirectories.Count);
            Assert.IsEmpty(root.Files);
        }
    }
}