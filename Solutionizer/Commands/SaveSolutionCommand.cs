using System;
using System.IO;
using System.Linq;
using System.Text;
using Solutionizer.Extensions;
using Solutionizer.Helper;
using Solutionizer.Models;
using Solutionizer.VisualStudio;

namespace Solutionizer.Commands {
    public class SaveSolutionCommand {
        private readonly string _solutionFileName;
        private readonly SolutionViewModel _solution;

        public SaveSolutionCommand(string solutionFileName, SolutionViewModel solution) {
            _solutionFileName = solutionFileName;
            _solution = solution;
        }

        public void Execute() {
            using (var writer = new StreamWriter(_solutionFileName, false, Encoding.UTF8)) {
                WriteHeader(writer);

                var projects = _solution.SolutionRoot.Items.Flatten<SolutionItem, SolutionProject, SolutionFolder>(p => p.Items);

                foreach (var project in projects) {
                    writer.WriteLine("Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"", "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}",
                                     project.Name, FileSystem.GetRelativePath(_solutionFileName, project.Filepath),
                                     project.Guid.ToString("B").ToUpperInvariant());
                    writer.WriteLine("EndProject");
                }

                var folders = _solution.SolutionRoot.Items.Flatten<SolutionItem, SolutionFolder, SolutionFolder>(p => p.Items);
                foreach (var folder in folders) {
                    writer.WriteLine("Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"", "{2150E333-8FDC-42A3-9474-1A3956D46DE8}",
                                     folder.Name, folder.Name, folder.Guid.ToString("B").ToUpperInvariant());
                    writer.WriteLine("EndProject");
                }

                writer.WriteLine("Global");
                WriteSolutionProperties(writer);
                WriteNestedProjects(writer);
                WriteTfsInformation(writer);
                writer.WriteLine("EndGlobal");

                _solution.IsDirty = false;
            }
        }

        private void WriteSolutionProperties(TextWriter writer) {
            writer.WriteLine("\tGlobalSection(SolutionProperties) = preSolution");
            writer.WriteLine("\t\tHideSolutionNode = FALSE");
            writer.WriteLine("\tEndGlobalSection");
        }

        private void WriteHeader(TextWriter writer) {
            writer.WriteLine();
            writer.WriteLine("Microsoft Visual Studio Solution File, Format Version 11.00");
            writer.WriteLine("# Visual Studio 2010");
        }

        private void WriteNestedProjects(TextWriter writer) {
            var folders = _solution.SolutionRoot.Items.Flatten<SolutionItem, SolutionFolder, SolutionFolder>(p => p.Items).ToList();
            if (folders.Count == 0) {
                return;
            }

            writer.WriteLine("\tGlobalSection(NestedProjects) = preSolution");
            foreach (var folder in folders) {
                foreach (var project in folder.Items) {
                    writer.WriteLine("\t\t{0} = {1}", project.Guid.ToString("B").ToUpperInvariant(),
                                     folder.Guid.ToString("B").ToUpperInvariant());
                }
            }
            writer.WriteLine("\tEndGlobalSection");
        }

        private void WriteTfsInformation(TextWriter writer) {
            if (!_solution.IsSccBound) {
                return;
            }

            Uri tfsName;
            string tfsFolder;
            if (!TfsHelper.TryGetTeamProjectCollection(_solution.RootPath, out tfsName, out tfsFolder)) {
                return;
            }

            var projects = _solution.SolutionRoot.Items.Flatten<SolutionItem, SolutionProject, SolutionFolder>(p => p.Items).ToList();
            writer.WriteLine("\tGlobalSection({0}) = preSolution", "TeamFoundationVersionControl");
            writer.Write("\t\tSccNumberOfProjects = {0}", projects.Count);
            writer.Write("\t\tSccEnterpriseProvider = {4CA58AB2-18FA-4F8D-95D4-32DDF27D184C}");
            writer.Write("\t\tSccTeamFoundationServer = " + tfsName);
            var n = 0;
            foreach (var project in projects) {
                WriteTeamFoundationProject(writer, n, project, tfsFolder, tfsName.ToString());
                ++n;
            }
            writer.Write("\tEndGlobalSection");
        }

        private void WriteTeamFoundationProject(TextWriter w, int n, SolutionProject project, string tfsFolder, string tfsName) {
            string directoryName = Path.GetDirectoryName(project.Filepath);
            string str1 = directoryName.Length > _solution.RootPath.Length
                              ? directoryName.Substring(_solution.RootPath.Length).Replace("\\", "/") : string.Empty;
            string str2 = tfsFolder + str1;
            if (string.IsNullOrEmpty(str2)) {
                return;
            }
            w.WriteLine("\t\tSccProjectUniqueName{0} = {1}", n, FileSystem.GetRelativePath(_solutionFileName, project.Filepath));
            w.WriteLine("\t\tSccProjectTopLevelParentUniqueName{0} = {1}", n, Path.GetFileName(_solutionFileName));
            w.WriteLine("\t\tSccProjectName{0} = {1}", n, str2);
            w.WriteLine("\t\tSccAuxPath{0} = {1}", n, tfsName);
            w.WriteLine("\t\tSccLocalPath{0} = {1}", n, directoryName.Replace("\\", "\\\\"));
            w.WriteLine("\t\tSccProvider{0} = {{4CA58AB2-18FA-4F8D-95D4-32DDF27D184C}}", n);
        }
    }
}