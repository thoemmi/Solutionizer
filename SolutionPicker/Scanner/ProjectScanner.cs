using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SolutionPicker.Scanner {
    public class ProjectScanner {
        public DirectoryNode Scan(string path, Action<int> numberFilesFound) {
            return CreateDirectoryNode(path);
        }

        private List<DirectoryNode> GetDirectoryNodes(string path) {
            return
                Directory.EnumerateDirectories(path)
                    .Select(CreateDirectoryNode)
                    .Where(x => x != null)
                    .ToList();
        }

        private DirectoryNode CreateDirectoryNode(string folderpath) {
            var files = GetFileNodes(folderpath);
            var subfolders = GetDirectoryNodes(folderpath);

            // replace subfolders with only a file child with their child
            var simples = subfolders.Where(s => s.Files.Count == 1 && s.Subdirectories.Count == 0).Reverse().ToArray();
            foreach (var directoryNode in simples) {
                subfolders.Remove(directoryNode);
                files.Insert(0, directoryNode.Files[0]);
            }

            if (files.Count > 0 || subfolders.Count > 0) {
                return new DirectoryNode {
                    Name = Path.GetFileName(folderpath),
                    Path = folderpath,
                    Subdirectories = subfolders,
                    Files = files,
                };
            } else {
                return null;
            }
        }

        private List<FileNode> GetFileNodes(string path) {
            return
                Directory.EnumerateFiles(path, "*.csproj", SearchOption.TopDirectoryOnly)
                    .Concat(Directory.EnumerateFiles(path, "*.vbproj", SearchOption.TopDirectoryOnly))
                    .Concat(Directory.EnumerateFiles(path, "*.vcproj", SearchOption.TopDirectoryOnly))
                    .Select(CreateFileNode)
                    .ToList();
        }

        private FileNode CreateFileNode(string filepath) {
            return new FileNode {
                Name = Path.GetFileNameWithoutExtension(filepath),
                Path = filepath
            };
        }
    }

    public class DirectoryNode {
        private List<DirectoryNode> _subdirectories = new List<DirectoryNode>();
        private List<FileNode> _files = new List<FileNode>();

        public string Name { get; set; }
        public string Path { get; set; }

        public List<DirectoryNode> Subdirectories {
            get { return _subdirectories; }
            set { _subdirectories = value; }
        }

        public List<FileNode> Files {
            get { return _files; }
            set { _files = value; }
        }

        public List<object> Children {
            get { return _subdirectories.Cast<object>().Concat(_files).ToList(); }
        }
    }

    public class FileNode {
        public string Name { get; set; }
        public string Path { get; set; }
    }
}