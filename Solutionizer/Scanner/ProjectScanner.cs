using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Solutionizer.Scanner {
    public static class ProjectScanner {
        public static DirectoryNode Scan(string path, bool simplify) {
            if (!Directory.Exists(path)) {
                return null;
            }

            var node = CreateDirectoryNode(path, simplify);
            if (node == null) {
                node = new DirectoryNode {
                    Path = path
                };
            }
            node.Name = path;
            return node;
        }

        private static List<DirectoryNode> GetDirectoryNodes(string path, bool simplify) {
            return
                Directory.EnumerateDirectories(path)
                    .Select(p => CreateDirectoryNode(p, simplify))
                    .Where(x => x != null)
                    .ToList();
        }

        private static DirectoryNode CreateDirectoryNode(string folderpath, bool simplify) {
            var files = GetFileNodes(folderpath);
            var subfolders = GetDirectoryNodes(folderpath, simplify);

            // replace subfolders with only a file child with their child
            if (simplify) {
                var simples = subfolders.Where(s => s.Files.Count == 1 && s.Subdirectories.Count == 0).Reverse().ToArray();
                foreach (var directoryNode in simples) {
                    subfolders.Remove(directoryNode);
                    files.Insert(0, directoryNode.Files[0]);
                }
            }

            if (files.Count == 0 && subfolders.Count == 0) {
                // we're not interested in empty directories
                return null;
            }

            subfolders.Sort((d1, d2) => String.Compare(d1.Name, d2.Name, StringComparison.InvariantCultureIgnoreCase));
            files.Sort((f1, f2) => String.Compare(f1.Name, f2.Name, StringComparison.InvariantCultureIgnoreCase));

            return new DirectoryNode {
                Name = Path.GetFileName(folderpath),
                Path = folderpath,
                Subdirectories = subfolders,
                Files = files,
            };
        }

        private static List<FileNode> GetFileNodes(string path) {
            return
                Directory.EnumerateFiles(path, "*.csproj", SearchOption.TopDirectoryOnly)
                    .Concat(Directory.EnumerateFiles(path, "*.vbproj", SearchOption.TopDirectoryOnly))
                    //.Concat(Directory.EnumerateFiles(path, "*.vcproj", SearchOption.TopDirectoryOnly))
                    .Select(CreateFileNode)
                    .ToList();
        }

        private static FileNode CreateFileNode(string filepath) {
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