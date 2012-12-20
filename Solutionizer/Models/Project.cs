using System;
using System.Linq;
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
        private List<string> _configurations;

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
            var directoryName = Path.GetDirectoryName(_filepath);

            var p = Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.LoadProject(_filepath);
            _projectReferences = p
                .Items
                .Where(item => item.ItemType == "ProjectReference")
                .Select(item => Path.GetFullPath(Path.Combine(directoryName, item.EvaluatedInclude)))
                .ToList();

            _guid = Guid.Parse(p.Properties.Single(property => property.Name == "ProjectGuid").EvaluatedValue);
            _assemblyName = p.Properties.Single(property => property.Name == "AssemblyName").EvaluatedValue;
            _isSccBound = p.Properties
                .Where(property => property.Name == "SccProjectName")
                .Select(property => property.EvaluatedValue == "SAK")
                .SingleOrDefault();

            _configurations = p.ConditionedProperties["Configuration"];

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

        public List<string> Configurations {
            get { return _configurations; }
        }
    }
}