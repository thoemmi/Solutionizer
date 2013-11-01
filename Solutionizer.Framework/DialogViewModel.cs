using System;

namespace Solutionizer.Framework {
    public abstract class DialogViewModel : PropertyChangedBase {
        protected void Close() {
            var handler = Closed;
            if (handler != null) {
                handler(this, EventArgs.Empty);
            }
        }

        public event EventHandler Closed;
    }
}