using System;
using System.IO;
using NLog;
using Newtonsoft.Json;
using Solutionizer.Infrastructure;

namespace Solutionizer.Services {
    public class SettingsProvider : IDisposable {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private Settings _settings;

        private string SettingsPath {
            get { return Path.Combine(AppEnvironment.DataFolder, "settings.json"); }
        }

        public Settings Settings {
            get { return _settings ?? (_settings = Load()); }
        }

        public void Dispose() {
            Save();
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