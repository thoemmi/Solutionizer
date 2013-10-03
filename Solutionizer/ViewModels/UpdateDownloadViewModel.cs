using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using Caliburn.Micro;
using NLog;
using Solutionizer.Infrastructure;

namespace Solutionizer.ViewModels {
    public class UpdateDownloadViewModel : Screen {
        private static readonly Logger _log = NLog.LogManager.GetCurrentClassLogger();

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly UpdateManager _updateManager;
        private readonly ReleaseInfo _releaseInfo;
        private int _progress;
        private bool _isNotDownloading = true;

        public UpdateDownloadViewModel(UpdateManager updateManager, ReleaseInfo releaseInfo) {
            _updateManager = updateManager;
            _releaseInfo = releaseInfo;
            DisplayName = "Downloading update";
        }

        protected override void OnViewLoaded(object view) {
            base.OnViewLoaded(view);

            Download();
        }

        private async void Download() {
            string filename;
            try {
                _log.Debug("Downloading update");
                filename = await _updateManager.DownloadReleaseAsync(
                    _releaseInfo, 
                    progress => { 
                        Progress = progress;
                        IsNotDownloading = false;
                    },
                    _cancellationTokenSource.Token);
            } catch (WebException ex) {
                if (ex.Status != WebExceptionStatus.RequestCanceled) {
                    _log.ErrorException("Error downloading release from " + _releaseInfo.DownloadUrl, ex);
                }
                filename = null;
            }
            if (filename != null && File.Exists(filename)) {
                _log.Debug("Downloading succeeded, spawning");
                Process.Start(filename);
                TryClose(true);
            } else {
                _log.Debug("Download failed or cancelled");
                TryClose(false);
            }
        }

        public void Cancel() {
            _log.Debug("Cancelling download");
            _cancellationTokenSource.Cancel();
        }

        public int Progress {
            get { return _progress; }
            set {
                if (value != _progress) {
                    _progress = value;
                    NotifyOfPropertyChange(() => Progress);
                }
            }
        }

        public bool IsNotDownloading {
            get { return _isNotDownloading; }
            set {
                if (value.Equals(_isNotDownloading)) {
                    return;
                }
                _isNotDownloading = value;
                NotifyOfPropertyChange(() => IsNotDownloading);
            }
        }
    }
}