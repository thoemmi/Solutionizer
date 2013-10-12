﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Xml;

namespace Solutionizer.Models {
    public class Project {
        private readonly string _filepath;
        private ProjectFolder _parent;
        private readonly string _name;
        private string _assemblyName;
        private Guid _guid;
        private bool _isSccBound;
        private List<string> _projectReferences;
        private readonly List<string> _brokenProjectReferences = new List<string>();
        private readonly List<string> _errors = new List<string>();
        private Task<IList<string>> _taskLoadConfigurations;

        public Project(string filepath) : this(filepath, null) {}

        public Project(string filepath, ProjectFolder parent) {
            _filepath = filepath;
            _parent = parent;
            _name = Path.GetFileNameWithoutExtension(_filepath);
        }

        public void Load() {
            try {
                LoadInternal();
            }
            catch (XmlException) {
                // log exception
            }
        }

        public ProjectFolder Parent {
            get { return _parent; }
            set { _parent = value; }
        }

        private void LoadInternal() {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(_filepath);
            if (xmlDocument.DocumentElement.NamespaceURI != "http://schemas.microsoft.com/developer/msbuild/2003") {
                throw new ArgumentException("Not a valid VS2005 C# project file: \"" + _filepath + "\"");
            }

            var assemblyName = xmlDocument.GetElementsByTagName("AssemblyName")[0].FirstChild.Value;
            var guid = Guid.Parse(xmlDocument.GetElementsByTagName("ProjectGuid")[0].FirstChild.Value);
            var directoryName = Path.GetDirectoryName(_filepath);

            var projectReferences = new List<string>();
            foreach (XmlNode xmlNode in xmlDocument.GetElementsByTagName("ProjectReference")) {
                projectReferences.Add(Path.GetFullPath(Path.Combine(directoryName, xmlNode.Attributes["Include"].Value)));
            }

            var assemblyReferences = new List<string>();
            foreach (XmlNode xmlNode2 in xmlDocument.GetElementsByTagName("Reference")) {
                var include = xmlNode2.Attributes["Include"].Value;
                var num = include.IndexOf(',');
                if (num >= 0) {
                    include = include.Substring(0, num);
                }
                //Project.binary_references.Add(text);
                assemblyReferences.Add(include.ToLowerInvariant());
            }

            var isSccBound = false;
            var elementsByTagName = xmlDocument.GetElementsByTagName("SccProjectName");
            if (elementsByTagName.Count > 0) {
                isSccBound = elementsByTagName[0].ChildNodes.Count > 0 && !string.IsNullOrEmpty(elementsByTagName[0].FirstChild.Value);
            }

            _assemblyName = assemblyName;
            _guid = guid;
            _isSccBound = isSccBound;
            _projectReferences = projectReferences;

            _taskLoadConfigurations = Task<IList<string>>.Factory.StartNew(LoadConfigurationsWithMicrosoftBuild);
        }

        private IList<string> LoadConfigurationsWithMicrosoftBuild() {
            Microsoft.Build.Evaluation.Project p = null;
            try {
                p = Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.LoadProject(_filepath);
                var configurations = p.ConditionedProperties["Configuration"];
                var platforms = p.ConditionedProperties["Platform"];
                return configurations.SelectMany(configuration => platforms.Select(platform => configuration + "|" + platform)).ToList();
            } catch (Exception ex) {
                _errors.Add(ex.Message);
                return new string[0];
            } finally {
                if (p != null) {
                    Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.UnloadProject(p);
                }
            }
        }

        public string Filepath {
            get { return _filepath; }
        }

        public string Name {
            get { return _name; }
        }

        public string AssemblyName {
            get { return _assemblyName; }
        }

        public Guid Guid {
            get { return _guid; }
        }

        public bool IsSccBound {
            get { return _isSccBound; }
        }

        public List<string> ProjectReferences {
            get { return _projectReferences; }
        }

        public List<string> BrokenProjectReferences {
            get { return _brokenProjectReferences; }
        }

        public List<string> Errors {
            get { return _errors; }
        }

        public IList<string> Configurations {
            get { return _taskLoadConfigurations.Result; }
        }
    }
}