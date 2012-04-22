using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Solutionizer.FileScanning;

namespace Solutionizer.Tests {
    public class ProjectTestBase {
        private static readonly Random _random = new Randomizer(Environment.TickCount);
        protected string _testDataFolderName;
        protected string _testDataPath;
        protected FileScanningViewModel _fileScanner;

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
            if (_fileScanner != null) {
                WaitForProjectLoaded(_fileScanner);
            }
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

        protected void WaitForProjectLoaded(FileScanningViewModel fileScanner) {
            while (!fileScanner.Projects.Values.All(p => p.IsLoaded)) {
                System.Threading.Thread.Sleep(50);
            }
        }
    }
}