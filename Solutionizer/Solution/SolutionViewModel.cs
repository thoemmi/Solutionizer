using System.Windows;

namespace Solutionizer.Solution {
    public class SolutionViewModel {
        public void PreviewDragOver(DragEventArgs e) {
            var x = e.Data.GetFormats();
        }

        public void PreviewDrop(DragEventArgs e) {
        }
    }
}