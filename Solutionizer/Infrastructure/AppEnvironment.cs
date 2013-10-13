using System;
using System.IO;
using System.Reflection;

namespace Solutionizer.Infrastructure {
    public class AppEnvironment {
        private static string _dataFolder;

        public static string DataFolder {
            get { return _dataFolder ?? (_dataFolder = GetDataFolder()); }
        }

        public static Version CurrentVersion {
            get { return Assembly.GetEntryAssembly().GetName().Version; }
        }

        private static string GetDataFolder() {
            var dataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Assembly.GetEntryAssembly().GetName().Name);

            if (!Directory.Exists(dataFolder)) {
                Directory.CreateDirectory(dataFolder);
            }

            return dataFolder;
        }
    }
}