using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Solutionizer.ViewModels;

namespace Solutionizer.Tests {
    public class ProjectTestBase {
        private static readonly Random _random = new Randomizer(Environment.TickCount);
        protected string _testDataFolderName;
        protected string _testDataPath;
        protected ScanningCommand _scanningCommand;

        [SetUp]
        public void SetUp() {
            //Services.Settings.Instance = new Services.Settings {
            //    SimplifyProjectTree = true
            //};

            _testDataFolderName = "SolutionizerTest-" + DateTime.Now.ToString("o").Replace(':', '-') + "-" + _random.Next();
            _testDataPath = Path.Combine(Path.GetTempPath(), _testDataFolderName);
            Directory.CreateDirectory(_testDataPath);
        } 

        [TearDown]
        public void TearDown() {
            Directory.Delete(_testDataPath, true);
        }

        protected void CopyTestDataToPath(string resource, string path) {
            Directory.CreateDirectory(path);
            var content = ReadFromResource(resource);
            using (var output = File.CreateText(Path.Combine(path, resource))) {
                output.Write(content);
            }
        }

        protected string ReadFromResource(string resource) {
            var stream = typeof(ProjectTestBase).Assembly.GetManifestResourceStream("Solutionizer.Tests.TestData." + resource);
            Assert.NotNull(stream);
            using (var input = new StreamReader(stream)) {
                return input.ReadToEnd();
            }
        }
    }
}