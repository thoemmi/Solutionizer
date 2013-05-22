using System;
using System.IO;
using NLog;
using Newtonsoft.Json;

namespace Solutionizer.Services {
    public class SettingsProvider {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly string _dataFolder;
        private Settings _settings;

        public SettingsProvider(string dataFolder) {
            _dataFolder = dataFolder;
        }

        private string SettingsPath {
            get { return Path.Combine(_dataFolder, "settings.json"); }
        }

        public Settings Settings {
            get { return _settings ?? (_settings = Load()); }
        }

        private Settings Load() {
            Settings settings;
            try {
                if (File.Exists(SettingsPath)) {
                    var fileData = File.ReadAllText(SettingsPath);
                    settings = JsonConvert.DeserializeObject<Settings>(fileData);
                } else {
                    settings = new Settings();
                }
                settings.IsDirty = false;
            } catch (Exception e) {
                _log.ErrorException("Loading settings from " + SettingsPath + " failed", e);
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
            } catch (Exception e) {
                _log.ErrorException("Saving settings failed", e);
            }

            _settings.IsDirty = false;
        }
    }
}