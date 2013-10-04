using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Solutionizer.Services;

namespace Solutionizer.Infrastructure {
    public class UpdateManager {
        private readonly ISettings _settings;
        private readonly IReleaseProvider _reader;
        private readonly List<ReleaseInfo> _releases = new List<ReleaseInfo>();

        public UpdateManager(ISettings settings) {
            _settings = settings;
            //_reader = new FakeReleaseProvider();
            _reader = new GithubReleaseProvider(_settings);
        }

        public async Task LoadCompletedEventHandler() {
            _releases.AddRange(await _reader.GetReleaseInfosAsync());
            if (_releases.Any()) {
                OnUpdatesAvailable();
            }
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