using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;

namespace Solutionizer.Infrastructure {
    public interface IUpdateManager {
        Task CheckForUpdatesAsync();
        IReadOnlyCollection<ReleaseInfo> Releases { get; }
        event EventHandler UpdatesAvailable;
        Task<string> DownloadReleaseAsync(ReleaseInfo releaseInfo, Action<int> downloadProgressCallback, CancellationToken cancellationToken);
    }

    public class UpdateManager : IUpdateManager {
        private readonly Version _currentVersion;
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly IReleaseProvider _reader;
        private readonly List<ReleaseInfo> _releases = new List<ReleaseInfo>();

        public UpdateManager(IReleaseProvider reader) {
            _currentVersion = AppEnvironment.CurrentVersion;
            _reader = reader;

            _releases = LoadReleases();
            _releases.Sort((r1, r2) => r2.Version.CompareTo(r1.Version));
            _releases.ForEach(r => r.IsNew = r.Version > _currentVersion);
        }

        public async Task CheckForUpdatesAsync() {
            _log.Debug("Checking for updates.");
            IReadOnlyCollection<ReleaseInfo> newReleases;
            try {
                newReleases = await _reader.GetReleaseInfosAsync();
            } catch (Exception ex) {
                _log.Error(ex, "Getting release informations failed");
                return;
            }

            // remove re-published versions
            foreach (var release in newReleases) {
                _releases.RemoveAll(r => r.Version == release.Version);
            }

            _releases.AddRange(newReleases);
            _releases.Sort((r1, r2) => r2.Version.CompareTo(r1.Version));
            _releases.ForEach(r => r.IsNew = r.Version > _currentVersion);
            SaveReleases();
            if (_releases.Any(r => r.IsNew)) {
                _log.Debug("{0} new updates detected.", _releases.Count(r => r.IsNew));
                OnUpdatesAvailable();
            } else {
                _log.Debug("No updates detected.");
            }
        }

        private List<ReleaseInfo> LoadReleases() {
            if (File.Exists(ReleasesPath)) {
                var fileData = File.ReadAllText(ReleasesPath);
                return JsonConvert.DeserializeObject<List<ReleaseInfo>>(fileData);
            } else {
                return new List<ReleaseInfo>();
            }
        }

        private void SaveReleases() {
            try {
                using (var textWriter = new StreamWriter(ReleasesPath)) {
                    textWriter.WriteLine(JsonConvert.SerializeObject(_releases, Formatting.Indented));
                }
            }
            catch (Exception e) {
                _log.Error(e, "Saving settings failed");
            }
        }

        private string ReleasesPath {
            get { return Path.Combine(AppEnvironment.DataFolder, "releases.json"); }
        }

        public IReadOnlyCollection<ReleaseInfo> Releases {
            get { return _releases; }
        }

        public event EventHandler UpdatesAvailable;

        protected virtual void OnUpdatesAvailable() {
            var handler = UpdatesAvailable;
            if (handler != null) {
                handler(this, EventArgs.Empty);
            }
        }

        public Task<string> DownloadReleaseAsync(ReleaseInfo releaseInfo, Action<int> downloadProgressCallback, CancellationToken cancellationToken) {
            return _reader.DownloadReleasePackage(releaseInfo, downloadProgressCallback, cancellationToken);
        }
    }
}