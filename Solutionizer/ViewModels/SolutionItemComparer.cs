using System;
using System.Collections.Generic;

namespace Solutionizer.ViewModels {
    public class SolutionItemComparer : IComparer<SolutionItem> {
        public int Compare(SolutionItem x, SolutionItem y) {
            var xIsFolder = x is SolutionFolder;
            var yIsFolder = y is SolutionFolder;
            if (xIsFolder && !yIsFolder) {
                return -1;
            }
            if (!xIsFolder && yIsFolder) {
                return +1;
            }
            return StringComparer.InvariantCultureIgnoreCase.Compare(x.Name, y.Name);
        }
    }
}