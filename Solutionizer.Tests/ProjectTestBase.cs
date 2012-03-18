using System;
using System.IO;
using NUnit.Framework;
using Solutionizer.ViewModels;

namespace Solutionizer.Tests {
    public class ProjectTestBase {
        private static readonly Random _random = new Randomizer(Environment.TickCount);
        protected string _testDataPath;

        [SetUp]
        public void SetUp() {
            Settings.Instance = new Settings();

            _testDataPath = Path.Combine(Path.GetTempPath(), "SolutionizerTest-" + DateTime.Now.ToString("o").Replace(':', '-') + "-" + _random.Next());
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
            using (var input = new StreamReader(stream)) {
                return input.ReadToEnd();
            }
        }
    }
}