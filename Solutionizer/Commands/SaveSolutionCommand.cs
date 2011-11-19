using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
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
            //var solutionName = Path.GetFileName(filename);
            using (var streamWriter = File.CreateText(_solutionFileName)) {
                WriteHeader(streamWriter);

                foreach (var project in _solution.Projects) {
                    streamWriter.WriteLine("Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"", "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}",
                                           project.Name, GetRelativePath(_solutionFileName, project.Filepath),
                                           project.Guid.ToString("B").ToUpperInvariant());
                    streamWriter.WriteLine("EndProject");
                }

                foreach (var project in _solution.ReferencedProjects) {
                    streamWriter.WriteLine("Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"", "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}",
                                           project.Name, GetRelativePath(_solutionFileName, project.Filepath),
                                           project.Guid.ToString("B").ToUpperInvariant());
                    streamWriter.WriteLine("EndProject");
                }

                // write solution folder for referenced projects
                streamWriter.WriteLine("Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"", "{2150E333-8FDC-42A3-9474-1A3956D46DE8}",
                                       "References", "References", "{95374152-F021-4ABB-B317-74A183A89F00}");
                streamWriter.WriteLine("EndProject");

                streamWriter.WriteLine("Global");
                WriteSolutionProperties(streamWriter);
                WriteNestedProjects(streamWriter);
                streamWriter.WriteLine("EndGlobal");
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
            streamWriter.WriteLine("\tGlobalSection(NestedProjects) = preSolution");
            foreach (var referencedProject in _solution.ReferencedProjects) {
                streamWriter.WriteLine("\t\t{0} = {1}", referencedProject.Guid.ToString("B").ToUpperInvariant(),
                                       "{95374152-F021-4ABB-B317-74A183A89F00}");
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