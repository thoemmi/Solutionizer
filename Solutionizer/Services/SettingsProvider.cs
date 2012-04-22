using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace Solutionizer.Services {
    public class SettingsProvider {
        private Settings _settings;

        private static string SettingsPath {
            get {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    Assembly.GetEntryAssembly().GetName().Name,
                    "settings.json");
            }
        }

        public Settings Settings {
            get { return _settings ?? (_settings = Load()); }
        }

        private static Settings Load() {
            Settings settings;
            try {
                if (File.Exists(SettingsPath)) {
                    var fileData = File.ReadAllText(SettingsPath);
                    settings = JsonConvert.DeserializeObject<Settings>(fileData);
                } else {
                    settings = new Settings();
                }
                settings.IsDirty = false;
            } catch (Exception) {
                // TODO logging
                settings = new Settings {
                    IsDirty = true
                };
            }

            return settings;
        }

        public void Save() {
            if (_settings == null || !_settings.IsDirty) {
                return;
            }

            var path = Path.GetDirectoryName(SettingsPath);
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }

            try {
                using (var textWriter = new StreamWriter(SettingsPath)) {
                    textWriter.WriteLine(JsonConvert.SerializeObject(_settings, Formatting.Indented));
                }
            } catch (Exception) {
                // TODO log exception
            }

            _settings.IsDirty = false;
        }
    }
}