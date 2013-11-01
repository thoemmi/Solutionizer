using System;
using System.Threading.Tasks;

namespace Solutionizer.Framework {
    internal interface IDialogViewModel {
        event EventHandler Closed;
    }

    public abstract class DialogViewModel<TResult> : PropertyChangedBase, IDialogViewModel {
        private readonly TaskCompletionSource<TResult> _tcs;

        protected DialogViewModel() {
            _tcs = new TaskCompletionSource<TResult>();
        }

        protected void Close(TResult result) {
            _tcs.SetResult(result);

            var handler = Closed;
            if (handler != null) {
                handler(this, EventArgs.Empty);
            }
        }

        internal Task<TResult> Task {
            get { return _tcs.Task; }
        }

        public event EventHandler Closed;
    }
}