using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NLog;
using Solutionizer.Infrastructure;

namespace Solutionizer.Services {
    public interface IMostRecentUsedFoldersRepository {
        void AddFolder(string folder);
        ObservableCollection<string> Folders { get; }
    }

    public class MostRecentUsedFoldersRepository : IMostRecentUsedFoldersRepository {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        
        private const int LENGTH = 10;
        private readonly string _mruFile;
        private readonly ObservableCollection<string> _folders = new ObservableCollection<string>();

        public MostRecentUsedFoldersRepository() {
            _mruFile = Path.Combine(AppEnvironment.DataFolder, "mru.json");
            Load();
        }

        private void Load() {
            try {
                if (File.Exists(_mruFile)) {
                    var fileData = File.ReadAllText(_mruFile);
                    var folders = JsonConvert.DeserializeObject<string[]>(fileData);
                    _folders.Clear();
                    foreach (var folder in folders) {
                        _folders.Add(folder);
                    }
                } 
            }
            catch (Exception e) {
                _log.ErrorException("Loading settings from " + _mruFile + " failed", e);
            }
        }

        public ObservableCollection<string> Folders {
            get { return _folders; }
        }

        public void AddFolder(string folder) {
            _folders.Remove(folder);
            _folders.Insert(0, folder);
            while (_folders.Count > LENGTH) {
                _folders.RemoveAt(_folders.Count - 1);
            }

            try {
                using (var textWriter = new StreamWriter(_mruFile)) {
                    textWriter.WriteLine(JsonConvert.SerializeObject(_folders.ToArray(), Formatting.Indented));
                }
            }
            catch (Exception e) {
                _log.ErrorException("Saving mru folder list failed", e);
            }
        }
    }
}