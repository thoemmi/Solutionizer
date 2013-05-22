using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Solutionizer.Infrastructure {
    public class SortedObservableCollection<T> : ObservableCollection<T> {
        private readonly IComparer<T> _comparer;

        public SortedObservableCollection(IComparer<T> comparer) {
            _comparer = comparer;
        }

        protected override void InsertItem(int index, T item) {
            for (var i = 0; i < Count; i++) {
                switch (Math.Sign(_comparer.Compare(this[i], item))) {
                    case 0:
                        throw new InvalidOperationException("Cannot insert duplicated items");
                    case 1:
                        base.InsertItem(i, item);
                        return;
                    case -1:
                        break;
                }
            }

            base.InsertItem(Count, item);
        }
    }
}