using System;
using System.Collections;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using Solutionizer.Infrastructure;
using PropertyChangedBase = Caliburn.Micro.PropertyChangedBase;

namespace Solutionizer.ViewModels {
    [Export(typeof (IDialogManager)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class DialogConductorViewModel : PropertyChangedBase, IDialogManager, IConductActiveItem {
        public void ShowDialog(IScreen dialogModel) {
            ActivateItem(dialogModel);
        }

        public IScreen ActiveItem { get; private set; }

        object IHaveActiveItem.ActiveItem {
            get { return ActiveItem; }
            set { ActivateItem(value); }
        }

        public IEnumerable GetChildren() {
            return ActiveItem != null ? new[] { ActiveItem } : new object[0];
        }

        public void ActivateItem(object item) {
            ActiveItem = item as IScreen;

            var child = ActiveItem as IChild;
            if (child != null) {
                child.Parent = this;
            }

            if (ActiveItem != null) {
                ActiveItem.Activate();
            }

            NotifyOfPropertyChange(() => ActiveItem);
            OnActivationProcessed(ActiveItem, true);
        }

        public void DeactivateItem(object item, bool close) {
            var guard = item as IGuardClose;
            if (guard != null) {
                guard.CanClose(result => {
                    if (result) {
                        CloseActiveItemCore();
                    }
                });
            } else {
                CloseActiveItemCore();
            }
        }

        private void CloseActiveItemCore() {
            var oldItem = ActiveItem;
            ActivateItem(null);
            oldItem.Deactivate(true);
        }

        private void OnActivationProcessed(object item, bool success) {
            var handler = ActivationProcessed;
            if (handler != null) {
                handler(this, new ActivationProcessedEventArgs { Item = item, Success = success });
            }
        }

        public event EventHandler<ActivationProcessedEventArgs> ActivationProcessed;
    }
}