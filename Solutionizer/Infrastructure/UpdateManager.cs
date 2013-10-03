using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Solutionizer.Infrastructure {
    public class UpdateManager {
        private IReadOnlyCollection<ReleaseInfo> _releases;

        public async Task LoadCompletedEventHandler() {
            var reader = new FakeReleaseInfoReader();
            _releases = await reader.GetReleaseInfosAsync();
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
    }
}