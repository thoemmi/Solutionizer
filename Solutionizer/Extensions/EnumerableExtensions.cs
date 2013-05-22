using System;
using System.Linq;
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

        public static IEnumerable<TResult> Flatten<T,TResult,U>(this IEnumerable<T> self, Func<U, IEnumerable<T>> children) where TResult : T where U : T {
            if (self == null) {
                throw new ArgumentNullException("self");
            }

            foreach (var item in self.OfType<TResult>()) {
                yield return item;
            }

            foreach (var child in self.OfType<U>().SelectMany(item2 => children(item2).Flatten<T,TResult, U>(children))) {
                yield return child;
            }
        }
    }
}