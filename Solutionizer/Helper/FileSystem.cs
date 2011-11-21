using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Solutionizer.Helper {
    public static class FileSystem {
        public static string GetRelativePath(string fromPath, string toPath) {
            var fromAttr = GetPathAttribute(fromPath);
            var toAttr = GetPathAttribute(toPath);

            var path = new StringBuilder(260); // MAX_PATH
            if (PathRelativePathTo(path, fromPath, fromAttr, toPath, toAttr) == 0) {
                throw new ArgumentException("Paths must have a common prefix");
            }
            var relativePath = path.ToString();
            return relativePath.StartsWith(@".\") ? relativePath.Substring(2) : relativePath;
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