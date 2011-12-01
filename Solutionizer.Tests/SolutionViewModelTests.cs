using System.IO;
using NUnit.Framework;
using Solutionizer.Models;
using Solutionizer.VisualStudio;

namespace Solutionizer.Tests {
    [TestFixture]
    public class SolutionViewModelTests : ProjectTestBase {
        [Test]
        public void CanAddProject() {
            CopyTestDataToPath("CsTestProject1.csproj", _testDataPath);
            var project = Project.Load(Path.Combine(_testDataPath, "CsTestProject1.csproj"));

            var sut = new SolutionViewModel();
            sut.CreateSolution(_testDataPath);
            sut.AddProject(project);

            Assert.AreEqual(1, sut.SolutionRoot.Items.Count);
            Assert.AreEqual("CsTestProject1", sut.SolutionRoot.Items[0].Name);
            Assert.AreEqual(typeof (SolutionProject), sut.SolutionRoot.Items[0].GetType());
        }

        [Test]
        public void CanAddProjectWithProjectReference() {
            CopyTestDataToPath("CsTestProject1.csproj", Path.Combine(_testDataPath, "p1"));
            CopyTestDataToPath("CsTestProject2.csproj", Path.Combine(_testDataPath, "p2"));
            var project = Project.Load(Path.Combine(_testDataPath, "p2", "CsTestProject2.csproj"));

            var sut = new SolutionViewModel();
            sut.CreateSolution(_testDataPath);
            sut.AddProject(project);

            Assert.AreEqual(2, sut.SolutionRoot.Items.Count);
            Assert.AreEqual("_References", sut.SolutionRoot.Items[0].Name);
            Assert.AreEqual("CsTestProject2", sut.SolutionRoot.Items[1].Name);

            Assert.AreEqual(1, ((SolutionFolder) sut.SolutionRoot.Items[0]).Items.Count);
            Assert.AreEqual("CsTestProject1", ((SolutionFolder) sut.SolutionRoot.Items[0]).Items[0].Name);
        }
    }
}