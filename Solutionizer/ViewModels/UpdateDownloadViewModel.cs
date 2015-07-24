using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using NLog;
using Solutionizer.Infrastructure;
using TinyLittleMvvm;

namespace Solutionizer.ViewModels {
    public class UpdateDownloadViewModel : DialogViewModel<bool>, IOnLoadedHandler {
        public delegate UpdateDownloadViewModel Factory(ReleaseInfo releaseInfo);

        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IUpdateManager _updateManager;
        private readonly ReleaseInfo _releaseInfo;
        private readonly ICommand _cancelCommand;
        private int _progress;
        private string _progressText;
        private bool _isPreparingDownload;

        public UpdateDownloadViewModel(IUpdateManager updateManager, ReleaseInfo releaseInfo) {
            _updateManager = updateManager;
            _releaseInfo = releaseInfo;

            _cancelCommand = new RelayCommand(() => {
                _log.Debug("Cancelling download");
                _cancellationTokenSource.Cancel();
            });
        }

        public async Task OnLoadedAsync() {
            string filename;
            try {
                _log.Debug("Downloading update");
                ProgressText = "Downloading package from\n" + _releaseInfo.DownloadUrl;
                filename = await _updateManager.DownloadReleaseAsync(
                    _releaseInfo, 
                    progress => {
                        Progress = progress;
                        IsPreparingDownload = false;
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
                Close(true);
            } else {
                _log.Debug("Download failed or cancelled");
                Close(false);
            }
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

        public string ProgressText {
            get { return _progressText; }
            set {
                if (_progressText != value) {
                    _progressText = value;
                    NotifyOfPropertyChange(() => ProgressText);
                }
            }
        }

        public bool IsPreparingDownload {
            get { return _isPreparingDownload; }
            set {
                if (_isPreparingDownload != value) {
                    _isPreparingDownload = value;
                    NotifyOfPropertyChange(() => IsPreparingDownload);
                }
            }
        }

        public ICommand CancelCommand {
            get { return _cancelCommand; }
        }
    }
}