using System;
using System.Linq.Expressions;
using Caliburn.Micro;

namespace Solutionizer.Settings {
    public sealed class SettingsViewModel : Screen {
        private bool _scanOnStartup;
        private bool _simplifyProjectTree;
        private bool _includeReferencedProjects;

        public SettingsViewModel() {
            DisplayName = "Settings";
        }

        protected override void OnActivate() {
            base.OnActivate();

            ScanOnStartup = Services.Settings.Instance.ScanOnStartup;
            SimplifyProjectTree = Services.Settings.Instance.SimplifyProjectTree;
            IncludeReferencedProjects = Services.Settings.Instance.IncludeReferencedProjects;
        }

        public bool ScanOnStartup {
            get { return _scanOnStartup; }
            set {
                if (_scanOnStartup != value) {
                    _scanOnStartup = value;
                    NotifyOfPropertyChange(() => ScanOnStartup);
                }
            }
        }

        public bool SimplifyProjectTree {
            get { return _simplifyProjectTree; }
            set {
                if (_simplifyProjectTree != value) {
                    _simplifyProjectTree = value;
                    NotifyOfPropertyChange(() => SimplifyProjectTree);
                }
            }
        }

        public bool IncludeReferencedProjects {
            get { return _includeReferencedProjects; }
            set {
                if (_includeReferencedProjects != value) {
                    _includeReferencedProjects = value;
                    NotifyOfPropertyChange(() => IncludeReferencedProjects);
                }
            }
        }

        public override void NotifyOfPropertyChange(string propertyName) {
            base.NotifyOfPropertyChange(propertyName);

            // if any property changed that is not CanOk, we want the UI to evaluate that property
            Expression<Func<bool>> property = () => CanOk;
            var canOkName = property.GetMemberInfo().Name;
            if (propertyName != canOkName) {
                NotifyOfPropertyChange(property);
            }
        }

        public void Ok() {
            Services.Settings.Instance.ScanOnStartup = ScanOnStartup;
            Services.Settings.Instance.SimplifyProjectTree = SimplifyProjectTree;
            Services.Settings.Instance.IncludeReferencedProjects = IncludeReferencedProjects;

            TryClose(true);
        }

        public bool CanOk {
            get {
                return
                    ScanOnStartup != Services.Settings.Instance.ScanOnStartup ||
                    SimplifyProjectTree != Services.Settings.Instance.SimplifyProjectTree ||
                    IncludeReferencedProjects != Services.Settings.Instance.IncludeReferencedProjects;
            }
        }

        public void Cancel() {
            TryClose(false);
        }
    }
}