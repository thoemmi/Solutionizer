using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Solutionizer.Scanner;

namespace Solutionizer.ViewModels {
    public class SolutionViewModel : ViewModelBase {
        private readonly ObservableCollection<Project> _projects = new ObservableCollection<Project>();
        private readonly ObservableCollection<Project> _referencedProjects = new ObservableCollection<Project>();
        private readonly ICommand _dropCommand;

        private readonly Dictionary<string,Project> _knownProjectsByPath = new Dictionary<string, Project>(StringComparer.InvariantCultureIgnoreCase);

        private bool _isSccBound;

        public SolutionViewModel() {
            _dropCommand = new RelayCommand<object>(OnDrop, obj => obj is FileNode);
        }

        private void OnDrop(object node) {
            var project = Project.Load(((FileNode)node).Path);
            AddProject(project);
        }

        public void AddProject(Project project) {
            _isSccBound |= project.IsSccBound;

            if (!_knownProjectsByPath.ContainsKey(project.Filepath)) {
                _knownProjectsByPath.Add(project.Filepath, project);
            }

            if (_projects.Contains(project)) {
                return;
            }
            _projects.Add(project);
            if (_referencedProjects.Contains(project)) {
                _referencedProjects.Remove(project);
            }
            AddReferencedProjects(project, 6);
        }

        private void AddReferencedProjects(Project project, int depth) {
            foreach (var projectReference in project.ProjectReferences) {
                Project referencedProject;
                if (!_knownProjectsByPath.TryGetValue(projectReference, out referencedProject)) {
                    referencedProject = Project.Load(projectReference);
                    _knownProjectsByPath.Add(referencedProject.Filepath, referencedProject);
                }
                if (!_referencedProjects.Contains(referencedProject) && !_projects.Contains(referencedProject)) {
                    _referencedProjects.Add(referencedProject);
                    if (depth > 0) {
                        AddReferencedProjects(referencedProject, depth - 1);
                    }
                }
            }
        }

        public ObservableCollection<Project> Projects {
            get { return _projects; }
        }

        public ObservableCollection<Project> ReferencedProjects {
            get { return _referencedProjects; }
        }

        public bool IsSccBound {
            get { return _isSccBound; }
            set {
                if (_isSccBound != value) {
                    _isSccBound = value;
                    RaisePropertyChanged(() => IsSccBound);
                }
            }
        }

        public ICommand DropCommand {
            get { return _dropCommand; }
        }

        public bool Save(string filename) {
            //var solutionName = Path.GetFileName(filename);
            using (var streamWriter = File.CreateText(filename)) {
                WriteHeader(streamWriter);

                foreach (var project in _referencedProjects.Concat(_projects)) {
                    streamWriter.WriteLine("Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"", "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}", project.Name, GetRelativePath(filename, project.Filepath), project.Guid.ToString("B").ToUpperInvariant());
                    streamWriter.WriteLine("EndProject");
                }

                streamWriter.WriteLine("Global");
                WriteSolutionConfigurationPlatforms(streamWriter);
                WriteProjectConfigurationPlatforms(streamWriter);
                WriteSolutionProperties(streamWriter);
                streamWriter.WriteLine("EndGlobal");
            }

            return true;
        }

        private void WriteSolutionConfigurationPlatforms(TextWriter streamWriter) {
            //streamWriter.WriteLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
            //streamWriter.WriteLine("\tEndGlobalSection");
        }

        private void WriteProjectConfigurationPlatforms(TextWriter streamWriter) {
            //streamWriter.WriteLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
            //streamWriter.WriteLine("\tEndGlobalSection");
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