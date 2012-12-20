using System;
using System.Collections.Generic;
using System.IO;
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
        private List<string> _brokenProjectReferences;

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
            var projectReferences = new List<string>();
            var assemblyReferences = new List<string>();

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(_filepath);
            if (xmlDocument.DocumentElement.NamespaceURI != "http://schemas.microsoft.com/developer/msbuild/2003") {
                throw new ArgumentException("Not a valid VS2005 C# project file: \"" + _filepath + "\"");
            }

            var assemblyName = xmlDocument.GetElementsByTagName("AssemblyName")[0].FirstChild.Value;
            var guid = Guid.Parse(xmlDocument.GetElementsByTagName("ProjectGuid")[0].FirstChild.Value);
            var directoryName = Path.GetDirectoryName(_filepath);
            foreach (XmlNode xmlNode in xmlDocument.GetElementsByTagName("ProjectReference")) {
                projectReferences.Add(Path.GetFullPath(Path.Combine(directoryName, xmlNode.Attributes["Include"].Value)));
            }
            foreach (XmlNode xmlNode2 in xmlDocument.GetElementsByTagName("Reference")) {
                var include = xmlNode2.Attributes["Include"].Value;
                var num = include.IndexOf(',');
                if (num >= 0) {
                    include = include.Substring(0, num);
                }
                //Project.binary_references.Add(text);
                assemblyReferences.Add(include.ToLowerInvariant());
            }

            var outputPath = xmlDocument.GetElementsByTagName("OutputPath")[0].FirstChild.Value;
            var outputType = xmlDocument.GetElementsByTagName("OutputType")[0].FirstChild.Value;
            var path = assemblyName + ((outputType == "WinExe") ? ".exe" : ".dll");
            var targetFilePath = Path.Combine(Path.GetFullPath(Path.Combine(directoryName, outputPath)), path);

            var isSccBound = false;
            var elementsByTagName = xmlDocument.GetElementsByTagName("SccProjectName");
            if (elementsByTagName.Count > 0) {
                isSccBound = !string.IsNullOrEmpty(elementsByTagName[0].FirstChild.Value);
            }

            _assemblyName = assemblyName;
            _guid = guid;
            _isSccBound = isSccBound;
            _projectReferences = projectReferences;
            _brokenProjectReferences = new List<string>();
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

        public bool HasBrokenProjectReferences {
            get { return _brokenProjectReferences != null && _brokenProjectReferences.Count > 0; }
        }
    }
}