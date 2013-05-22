using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Solutionizer.Extensions;
using Solutionizer.Helper;
using Solutionizer.Services;
using Solutionizer.ViewModels;

namespace Solutionizer.Commands {
    public class SaveSolutionCommand {
        private readonly ISettings _settings;
        private readonly string _solutionFileName;
        private readonly VisualStudioVersion _visualStudioVersion;
        private readonly SolutionViewModel _solution;

        public SaveSolutionCommand(ISettings settings, string solutionFileName, VisualStudioVersion visualStudioVersion, SolutionViewModel solution) {
            _settings = settings;
            _solutionFileName = solutionFileName;
            _visualStudioVersion = visualStudioVersion;
            _solution = solution;
        }

        public void Execute() {
            using (var writer = new StreamWriter(_solutionFileName, false, Encoding.UTF8)) {
                WriteHeader(writer);

                var projects = _solution.SolutionItems.Flatten<SolutionItem, SolutionProject, SolutionFolder>(p => p.Items).ToList();

                foreach (var project in projects) {
                    writer.WriteLine("Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"", "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}",
                                     project.Name, FileSystem.GetRelativePath(_solutionFileName, project.Filepath),
                                     project.Guid.ToString("B").ToUpperInvariant());
                    writer.WriteLine("EndProject");
                }

                var folders = _solution.SolutionItems.Flatten<SolutionItem, SolutionFolder, SolutionFolder>(p => p.Items).Where(f => f.Items.Any());
                foreach (var folder in folders) {
                    writer.WriteLine("Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"", "{2150E333-8FDC-42A3-9474-1A3956D46DE8}",
                                     folder.Name, folder.Name, folder.Guid.ToString("B").ToUpperInvariant());
                    writer.WriteLine("EndProject");
                }

                writer.WriteLine("Global");
                WriteTfsInformation(writer, projects);
                WriteSolutionConfigurationPlatforms(writer, projects);
                WriteProjectConfigurationPlatforms(writer, projects);
                WriteSolutionProperties(writer);
                WriteNestedProjects(writer);
                writer.WriteLine("EndGlobal");

                _solution.IsDirty = false;
            }
        }

        private void WriteSolutionConfigurationPlatforms(TextWriter writer, IEnumerable<SolutionProject> projects) {
            writer.WriteLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
            foreach (var configuration in projects.SelectMany(p => p.Configurations).Distinct()) {
                var fixedConfigurationNameBecauseOfA3YearsOldBugInVisualStudio = configuration.Replace("AnyCPU", "Any CPU");
                writer.WriteLine("\t\t{0} = {0}", fixedConfigurationNameBecauseOfA3YearsOldBugInVisualStudio);
            }
            writer.WriteLine("\tEndGlobalSection");
        }

        private void WriteProjectConfigurationPlatforms(TextWriter writer, IEnumerable<SolutionProject> projects) {
            writer.WriteLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
            foreach (var project in projects) {
                var guid = project.Guid.ToString("B").ToUpperInvariant();
                foreach (var configuration in project.Configurations) {
                    // there's a bug in Visual Studio since 2010 beta, that in sln files the platform is called "Any CPU" instead of "AnyCPU"
                    // http://connect.microsoft.com/VisualStudio/feedback/details/503935/msbuild-inconsistent-platform-for-any-cpu-between-solution-and-project
                    var fixedConfigurationNameBecauseOfA3YearsOldBugInVisualStudio = configuration.Replace("AnyCPU", "Any CPU");
                    writer.WriteLine("\t\t{0}.{1}.ActiveCfg = {1}", guid,  fixedConfigurationNameBecauseOfA3YearsOldBugInVisualStudio);
                    if (!_settings.DontBuildReferencedProjects || String.IsNullOrEmpty(project.Parent.Name)) {
                        writer.WriteLine("\t\t{0}.{1}.Build.0 = {1}", guid,  fixedConfigurationNameBecauseOfA3YearsOldBugInVisualStudio);
                    }
                }
            }
            writer.WriteLine("\tEndGlobalSection");
        }

        private void WriteSolutionProperties(TextWriter writer) {
            writer.WriteLine("\tGlobalSection(SolutionProperties) = preSolution");
            writer.WriteLine("\t\tHideSolutionNode = FALSE");
            writer.WriteLine("\tEndGlobalSection");
        }

        private void WriteHeader(TextWriter writer) {
            writer.WriteLine();
            switch (_visualStudioVersion) {
                case VisualStudioVersion.VS2010:
                    writer.WriteLine("Microsoft Visual Studio Solution File, Format Version 11.00");
                    writer.WriteLine("# Visual Studio 2010");
                    break;
                case VisualStudioVersion.VS2012:
                    writer.WriteLine("Microsoft Visual Studio Solution File, Format Version 12.00");
                    writer.WriteLine("# Visual Studio 2012");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void WriteNestedProjects(TextWriter writer) {
            var folders = _solution.SolutionItems.Flatten<SolutionItem, SolutionFolder, SolutionFolder>(p => p.Items).ToList();
            if (folders.Count == 0 || !folders.SelectMany(f => f.Items).Any()) {
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

        private void WriteTfsInformation(TextWriter writer, ICollection<SolutionProject> projects) {
            if (!_solution.IsSccBound) {
                return;
            }

            Uri tfsName = _settings.TfsName;
            string tfsFolder;
            if (!TfsHelper.TryGetTeamProjectCollection(_solution.RootPath, ref tfsName, out tfsFolder)) {
                return;
            }
            _settings.TfsName = tfsName;

            writer.WriteLine("\tGlobalSection({0}) = preSolution", "TeamFoundationVersionControl");
            writer.WriteLine("\t\tSccNumberOfProjects = {0}", projects.Count);
            writer.WriteLine("\t\tSccEnterpriseProvider = {4CA58AB2-18FA-4F8D-95D4-32DDF27D184C}");
            writer.WriteLine("\t\tSccTeamFoundationServer = " + tfsName);
            var n = 0;
            foreach (var project in projects) {
                WriteTeamFoundationProject(writer, n, project, tfsFolder, tfsName.ToString());
                ++n;
            }
            writer.WriteLine("\tEndGlobalSection");
        }

        private void WriteTeamFoundationProject(TextWriter w, int n, SolutionProject project, string tfsFolder, string tfsName) {
            var projectFolder = Path.GetDirectoryName(project.Filepath);
            Debug.Assert(projectFolder != null, "projectFolder != null");
            var relativeFolder = projectFolder.Length > _solution.RootPath.Length
                              ? projectFolder.Substring(_solution.RootPath.Length).Replace("\\", "/") : string.Empty;
            var relativeProjectPath = tfsFolder + relativeFolder;
            if (string.IsNullOrEmpty(relativeProjectPath)) {
                return;
            }
            w.WriteLine("\t\tSccProjectUniqueName{0} = {1}", n, FileSystem.GetRelativePath(_solutionFileName, project.Filepath).Replace("\\", "\\\\"));
            w.WriteLine("\t\tSccProjectTopLevelParentUniqueName{0} = {1}", n, Path.GetFileName(_solutionFileName));
            w.WriteLine("\t\tSccProjectName{0} = {1}", n, relativeProjectPath);
            w.WriteLine("\t\tSccAuxPath{0} = {1}", n, tfsName);
            w.WriteLine("\t\tSccLocalPath{0} = {1}", n, projectFolder.Replace("\\", "\\\\"));
            w.WriteLine("\t\tSccProvider{0} = {{4CA58AB2-18FA-4F8D-95D4-32DDF27D184C}}", n);
        }
    }
}