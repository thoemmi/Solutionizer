using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Shell;
using Newtonsoft.Json;
using NLog;
using Solutionizer.Framework;
using Solutionizer.Infrastructure;

namespace Solutionizer.Services {
    public interface IMostRecentUsedFoldersRepository {
        void SetCurrentFolder(string folder);
        ObservableCollection<string> Folders { get; }
    }

    public class MostRecentUsedFoldersRepository : IMostRecentUsedFoldersRepository, IDisposable {
        private readonly IUiExecution _uiExecution;
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        
        private const int LENGTH = 10;
        private readonly string _mruFile;
        private string _currentFolder;
        private readonly List<string> _folders = new List<string>();
        private readonly ObservableCollection<string> _foldersExceptCurrent = new ObservableCollection<string>();
        private readonly FileSystemWatcher _fileSystemWatcher;

        public MostRecentUsedFoldersRepository(IUiExecution uiExecution) {
            _uiExecution = uiExecution;
            _mruFile = Path.Combine(AppEnvironment.DataFolder, "mru.json");
            Load();
            _fileSystemWatcher = new FileSystemWatcher {
                Path = AppEnvironment.DataFolder, 
                Filter = "mru.json", 
                NotifyFilter = NotifyFilters.LastWrite, 
                IncludeSubdirectories = false
            };
            _fileSystemWatcher.Changed += OnMruFileChanged;
            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        void OnMruFileChanged(object sender, FileSystemEventArgs e) {
            _log.Debug("MRU file changed by other instance");
            Load();
        }

        public void Dispose() {
            _fileSystemWatcher.EnableRaisingEvents = false;
            _fileSystemWatcher.Dispose();
        }

        private void Load() {
            _log.Debug("Loading MRU list");
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
            _uiExecution.Execute(() => {
                _foldersExceptCurrent.Clear();
                foreach (var folder in _folders.Where(f => !String.Equals(f, _currentFolder))) {
                    _foldersExceptCurrent.Add(folder);
                }
            });
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

            _fileSystemWatcher.EnableRaisingEvents = false;
            _log.Debug("Saving MRU list");
            try {
                using (var textWriter = new StreamWriter(_mruFile)) {
                    textWriter.WriteLine(JsonConvert.SerializeObject(_folders.ToArray(), Formatting.Indented));
                }
            } catch (Exception e) {
                _log.ErrorException("Saving mru folder list failed", e);
            } finally {
                _fileSystemWatcher.EnableRaisingEvents = true;
            }

            UpdateJumpList();
            UpdateMruFolders();
        }

        private void UpdateJumpList() {
            var jumpList = JumpList.GetJumpList(Application.Current) ?? new JumpList();
            jumpList.ShowRecentCategory = true;

            // Remove JumpTasks for folders,which are not in the MRU list anymore
            //jumpList.JumpItems.RemoveAll(item => item is JumpTask && !_folders.Any(path => String.Equals(path, ((JumpTask)item).Title, StringComparison.OrdinalIgnoreCase)));

            // add JumpTasks for folders, which do not exist already
            foreach (var folder in _folders.Where(f => !jumpList.JumpItems.OfType<JumpTask>().Any(item => String.Equals(f, item.Title, StringComparison.OrdinalIgnoreCase)))) {
                var jumpTask = new JumpTask {
                    ApplicationPath = Assembly.GetExecutingAssembly().Location,
                    Arguments = folder,
                    IconResourcePath = @"C:\Windows\System32\shell32.dll",
                    IconResourceIndex = 3,
                    Title = folder,
                    CustomCategory = "Recent folders"
                };
                JumpList.AddToRecentCategory(jumpTask);
            }

            jumpList.Apply();        }
    }
}