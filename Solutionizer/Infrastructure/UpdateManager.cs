using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using Solutionizer.Services;

namespace Solutionizer.Infrastructure {
    public class UpdateManager {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly ISettings _settings;
        private readonly IReleaseProvider _reader;
        private readonly List<ReleaseInfo> _releases = new List<ReleaseInfo>();

        public UpdateManager(ISettings settings) {
            _settings = settings;
            //_reader = new FakeReleaseProvider();
            _reader = new GithubReleaseProvider(_settings);

            _releases = LoadReleases();
        }

        public async Task LoadCompletedEventHandler() {
            IReadOnlyCollection<ReleaseInfo> readOnlyCollection;
            try {
                readOnlyCollection = await _reader.GetReleaseInfosAsync();
            } catch (Exception ex) {
                _log.ErrorException("Getting release informations failed", ex);
                return;
            }
            _releases.AddRange(readOnlyCollection);
            SaveReleases();
            if (_releases.Any()) {
                OnUpdatesAvailable();
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
                _log.ErrorException("Saving settings failed", e);
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