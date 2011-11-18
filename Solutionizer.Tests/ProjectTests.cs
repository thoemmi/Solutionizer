using System.IO;
using NUnit.Framework;
using Solutionizer.ViewModels;

namespace Solutionizer.Tests {
    [TestFixture]
    public class ProjectTests : ProjectTestBase {
        [Test]
        public void CanReadProjectFileWithoutReferencedProjects() {
            CopyTestDataToPath("CsTestProject1.csproj", _testDataPath);

            var project = Project.Load(Path.Combine(_testDataPath, "CsTestProject1.csproj"));

            Assert.AreEqual("CsTestProject1", project.Name);
            Assert.AreEqual("CsTestProject1", project.AssemblyName);
            Assert.IsFalse(project.IsSccBound);
            Assert.IsEmpty(project.ProjectReferences);
        }
    }
}