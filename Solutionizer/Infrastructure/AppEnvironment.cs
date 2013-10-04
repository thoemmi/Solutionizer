using System.IO;
using System.Reflection;

namespace Solutionizer.Infrastructure {
    public class AppEnvironment {
        private static string _dataFolder;

        public static string DataFolder {
            get {
                return _dataFolder ?? (_dataFolder = GetDataFolder());
            }
        }

        private static string GetDataFolder() {
            var dataFolder = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                Assembly.GetEntryAssembly().GetName().Name);

            if (!Directory.Exists(dataFolder)) {
                Directory.CreateDirectory(dataFolder);
            }

            return dataFolder;
        }
    }
}