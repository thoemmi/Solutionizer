using System;
using System.Collections.Generic;

namespace Solutionizer.Extensions {
    public static class EnumerableExtensions {
        public static IEnumerable<R> Flatten<T,R>(this IEnumerable<T> self, Func<T, IEnumerable<R>> selector, Func<T, IEnumerable<T>> children) {
            foreach (var item in self) {
                foreach (var r in selector(item)) {
                    yield return r;
                }
                foreach (var child in children(item).Flatten(selector, children)) {
                    yield return child;
                }
            }
        }
    }
}