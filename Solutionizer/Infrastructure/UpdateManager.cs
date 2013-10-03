using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Solutionizer.Infrastructure {
    public class UpdateManager {
        private IReleaseProvider _reader;
        private IReadOnlyCollection<ReleaseInfo> _releases;

        public async Task LoadCompletedEventHandler() {
            //_reader = new FakeReleaseProvider();
            _reader = new GithubReleaseProvider();
            _releases = await _reader.GetReleaseInfosAsync();
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