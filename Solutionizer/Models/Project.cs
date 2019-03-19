using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using NLog;

namespace Solutionizer.Models {
    public class Project {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private List<string> _projectReferences = new List<string>();
        private Task<IList<string>> _taskLoadConfigurations;

        public Project(string filepath) : this(filepath, null) {}

        public Project(string filepath, ProjectFolder parent) {
            Filepath = filepath;
            Parent = parent;
            Name = Path.GetFileNameWithoutExtension(Filepath);
        }

        public void Load() {
            try {
                LoadInternal();
            }
            catch (Exception ex) {
                _log.Error(ex, "Loading project file '{0}' failed", Filepath);
                // log exception
            }
        }

        public ProjectFolder Parent { get; set; }

        private void LoadInternal() {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(Filepath);
            if (xmlDocument.DocumentElement.NamespaceURI != "http://schemas.microsoft.com/developer/msbuild/2003") {
                throw new ArgumentException("Not a supported C# project file: \"" + Filepath + "\"");
            }

            var assemblyNameElement = xmlDocument.GetElementsByTagName("AssemblyName");
            if (assemblyNameElement == null || assemblyNameElement.Count == 0) {
                throw new ArgumentException("Not a supported C# project file: \"" + Filepath + "\"");
            }
            var assemblyName = assemblyNameElement[0].FirstChild.Value;
            var guid = Guid.Parse(xmlDocument.GetElementsByTagName("ProjectGuid")[0].FirstChild.Value);
            var directoryName = Path.GetDirectoryName(Filepath);

            var projectReferences = new List<string>();
            foreach (var xmlNode in xmlDocument.GetElementsByTagName("ProjectReference").Cast<XmlNode>()) {
                if (xmlNode.Attributes != null) {
                    projectReferences.Add(Path.GetFullPath(Path.Combine(directoryName, xmlNode.Attributes["Include"].Value)));
                }
            }

            var assemblyReferences = new List<string>();
            foreach (var xmlNode in xmlDocument.GetElementsByTagName("Reference").Cast<XmlNode>()) {
                if (xmlNode.Attributes != null) {
                    var include = xmlNode.Attributes["Include"].Value;
                    var num = include.IndexOf(',');
                    if (num >= 0) {
                        include = include.Substring(0, num);
                    }
                    //Project.binary_references.Add(text);
                    assemblyReferences.Add(include.ToLowerInvariant());
                }
            }

            var isSccBound = false;
            var elementsByTagName = xmlDocument.GetElementsByTagName("SccProjectName");
            if (elementsByTagName.Count > 0) {
                isSccBound = elementsByTagName[0].ChildNodes.Count > 0 && !string.IsNullOrEmpty(elementsByTagName[0].FirstChild.Value);
            }

            AssemblyName = assemblyName;
            Guid = guid;
            IsSccBound = isSccBound;
            _projectReferences = projectReferences;

            _taskLoadConfigurations = Task<IList<string>>.Factory.StartNew(LoadConfigurationsWithMicrosoftBuild);
        }

        private IList<string> LoadConfigurationsWithMicrosoftBuild() {
            Microsoft.Build.Evaluation.Project p = null;
            try {
                p = Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.LoadProject(Filepath);
                var configurations = p.ConditionedProperties["Configuration"];
                var platforms = p.ConditionedProperties["Platform"];
                return configurations.SelectMany(configuration => platforms.Select(platform => configuration + "|" + platform)).ToList();
            } catch (Exception ex) {
                Errors.Add(ex.Message);
                return new string[0];
            } finally {
                if (p != null) {
                    try {
                        Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.UnloadProject(p);
                        // ReSharper disable once EmptyGeneralCatchClause
                    } catch {
                    }
                }
            }
        }

        public string Filepath { get; }

        public string Name { get; }

        public string AssemblyName { get; private set; }

        public Guid Guid { get; private set; }

        public bool IsSccBound { get; private set; }

        public List<string> ProjectReferences => _projectReferences ?? new List<string>();

        public List<string> BrokenProjectReferences { get; } = new List<string>();

        public List<string> Errors { get; } = new List<string>();

        public IList<string> Configurations => _taskLoadConfigurations.Result;
    }
}