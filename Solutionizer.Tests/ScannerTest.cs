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
    }
}