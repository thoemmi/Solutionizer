using System;
using System.Linq;
using Caliburn.Micro;
using Solutionizer.Infrastructure;

namespace Solutionizer.ViewModels {
    public class UpdateViewModel : Screen {
        public UpdateViewModel(UpdateManager updateManager) {
            DisplayName = "Updates";

            ReleaseNotes = String.Join("\n\n", updateManager.Releases.Select(r => r.ReleaseNotes));
        }

        public string ReleaseNotes {
            get; private set;
        }
    }
}