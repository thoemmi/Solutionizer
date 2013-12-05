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
        void SetCurrentFolder(string folder);
        ObservableCollection<string> Folders { get; }
    }

    public class MostRecentUsedFoldersRepository : IMostRecentUsedFoldersRepository {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        
        private const int LENGTH = 10;
        private readonly string _mruFile;
        private string _currentFolder;
        private readonly List<string> _folders = new List<string>();
        private readonly ObservableCollection<string> _foldersExceptCurrent = new ObservableCollection<string>();

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
                    UpdateMruFolders();
                } 
            }
            catch (Exception e) {
                _log.ErrorException("Loading most recent used folders from " + _mruFile + " failed", e);
            }
        }

        private void UpdateMruFolders() {
            _foldersExceptCurrent.Clear();
            foreach (var folder in _folders.Where(f => !String.Equals(f, _currentFolder))) {
                _foldersExceptCurrent.Add(folder);
            }
        }

        public ObservableCollection<string> Folders {
            get { return _foldersExceptCurrent; }
        }

        public void SetCurrentFolder(string folder) {
            _currentFolder = folder;
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

            UpdateMruFolders();
        }
    }
}