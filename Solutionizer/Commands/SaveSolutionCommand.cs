using System;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Solutionizer.Extensions;
using Solutionizer.ViewModels;

namespace Solutionizer.Commands {
    public class SaveSolutionCommand {
        private readonly string _solutionFileName;
        private readonly SolutionViewModel _solution;

        public SaveSolutionCommand(string solutionFileName, SolutionViewModel solution) {
            _solutionFileName = solutionFileName;
            _solution = solution;
        }

        public void Execute() {
            using (var streamWriter = File.CreateText(_solutionFileName)) {
                WriteHeader(streamWriter);

                var projects = _solution.SolutionFolder.Items.Flatten<SolutionItem, SolutionProject, SolutionFolder>(p => p.Items);

                foreach (var project in projects) {
                    streamWriter.WriteLine("Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"", "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}",
                                           project.Name, GetRelativePath(_solutionFileName, project.Project.Filepath),
                                           project.Guid.ToString("B").ToUpperInvariant());
                    streamWriter.WriteLine("EndProject");
                }

                var folders = _solution.SolutionFolder.Items.Flatten<SolutionItem, SolutionFolder, SolutionFolder>(p => p.Items);
                foreach (var folder in folders) {
                    streamWriter.WriteLine("Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"", "{2150E333-8FDC-42A3-9474-1A3956D46DE8}",
                                           folder.Name, folder.Name, folder.Guid.ToString("B").ToUpperInvariant());
                    streamWriter.WriteLine("EndProject");
                }

                streamWriter.WriteLine("Global");
                WriteSolutionProperties(streamWriter);
                WriteNestedProjects(streamWriter);
                streamWriter.WriteLine("EndGlobal");

                _solution.IsDirty = false;
            }
        }

        private void WriteSolutionProperties(TextWriter streamWriter) {
            streamWriter.WriteLine("\tGlobalSection(SolutionProperties) = preSolution");
            streamWriter.WriteLine("\t\tHideSolutionNode = FALSE");
            streamWriter.WriteLine("\tEndGlobalSection");
        }

        private void WriteHeader(TextWriter stream) {
            //stream.WriteLine();
            stream.WriteLine("Microsoft Visual Studio Solution File, Format Version 11.00");
            stream.WriteLine("# Visual Studio 2010");
        }

        private void WriteNestedProjects(TextWriter streamWriter) {
            var folders = _solution.SolutionFolder.Items.Flatten<SolutionItem, SolutionFolder, SolutionFolder>(p => p.Items).ToList();
            if (folders.Count == 0) {
                return;
            }

            streamWriter.WriteLine("\tGlobalSection(NestedProjects) = preSolution");
            foreach (var folder in folders) {
                foreach (var project in folder.Items.OfType<SolutionProject>()) {
                    streamWriter.WriteLine("\t\t{0} = {1}", project.Guid.ToString("B").ToUpperInvariant(),
                                           folder.Guid.ToString("B").ToUpperInvariant());
                }
            }
            streamWriter.WriteLine("\tEndGlobalSection");
        }

        public static string GetRelativePath(string fromPath, string toPath) {
            var fromAttr = GetPathAttribute(fromPath);
            var toAttr = GetPathAttribute(toPath);

            var path = new StringBuilder(260); // MAX_PATH
            if (PathRelativePathTo(path, fromPath, fromAttr, toPath, toAttr) == 0) {
                throw new ArgumentException("Paths must have a common prefix");
            }
            var relativePath = path.ToString();
            if (relativePath.StartsWith(@".\")) {
                return relativePath.Substring(2);
            } else {
                return relativePath;
            }
        }

        private static int GetPathAttribute(string path) {
            var di = new DirectoryInfo(path);
            if (di.Exists) {
                return FILE_ATTRIBUTE_DIRECTORY;
            }

            var fi = new FileInfo(path);
            if (fi.Exists) {
                return FILE_ATTRIBUTE_NORMAL;
            }

            throw new FileNotFoundException();
        }

        private const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
        private const int FILE_ATTRIBUTE_NORMAL = 0x80;

        [DllImport("shlwapi.dll", SetLastError = true)]
        private static extern int PathRelativePathTo(StringBuilder pszPath,
                                                     string pszFrom, int dwAttrFrom, string pszTo, int dwAttrTo);
    }
}