using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Solutionizer.Framework {
    public class DialogsControl : ItemsControl {
        protected override bool IsItemItsOwnContainerOverride(object item) {
            return base.IsItemItsOwnContainerOverride(item);
        }

        protected override DependencyObject GetContainerForItemOverride() {
            return base.GetContainerForItemOverride();
            //var view = (FrameworkElement)ViewLocator.GetViewForViewModel(viewModel);

            //return base.GetContainerForItemOverride();
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e) {
            base.OnItemsChanged(e);
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    AttachHandlers(GetViewModels(e.NewItems));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    DetachHandlers(GetViewModels(e.OldItems));
                    break;
                case NotifyCollectionChangedAction.Replace:
                    DetachHandlers(GetViewModels(e.OldItems));
                    AttachHandlers(GetViewModels(e.NewItems));
                    break;
                case NotifyCollectionChangedAction.Reset:
                    AttachHandlers(GetViewModels(Items));
                    break;
            }
        }

        private void AttachHandlers(IEnumerable<DialogViewModel> items) {
            foreach (var viewModel in items) {
                viewModel.Closed += ViewModelOnClosed;
            }
        }

        private void ViewModelOnClosed(object sender, EventArgs eventArgs) {
            var list = ItemsSource as IList;
            if (list != null) {
                list.Remove(sender);
            }
        }

        private void DetachHandlers(IEnumerable<DialogViewModel> items) {
            foreach (var viewModel in items) {
                viewModel.Closed -= ViewModelOnClosed;
            }
        }

        private static IEnumerable<DialogViewModel> GetViewModels(IEnumerable items) {
            return items.OfType<DialogViewModel>();
        }
    }
}