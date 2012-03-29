using System.Windows;
using System.Windows.Controls;

namespace Solutionizer.Helper {
    /// <summary>
    /// Inspired by http://blog.rag.no/post/A-simpler-(and-dynamic)-Grid-control-for-WPF.aspx
    /// </summary>
    public class GridHelper {
        public static int GetRowCount(DependencyObject obj) {
            return (int)obj.GetValue(RowCountProperty);
        }

        public static void SetRowCount(DependencyObject obj, int value) {
            obj.SetValue(RowCountProperty, value);
        }

        public static readonly DependencyProperty RowCountProperty =
            DependencyProperty.RegisterAttached("RowCount", typeof(int), typeof(GridHelper),
                                                new UIPropertyMetadata(1, RowCountChanged));


        public static void SetColumnCount(DependencyObject obj, int value) {
            obj.SetValue(ColumnCountProperty, value);
        }

        public static int GetColumnCount(DependencyObject obj) {
            return (int)obj.GetValue(ColumnCountProperty);
        }

        public static readonly DependencyProperty ColumnCountProperty =
            DependencyProperty.RegisterAttached("ColumnCount", typeof (int), typeof (GridHelper),
                                                new UIPropertyMetadata(1, ColumnCountChanged));

        private static void RowCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var grid = (Grid)d;
            var newRowCount = (int)e.NewValue;
            var currentRowCount = grid.RowDefinitions.Count;

            while (newRowCount > currentRowCount) {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                currentRowCount++;
            }

            while (newRowCount < currentRowCount) {
                currentRowCount--;
                grid.RowDefinitions.RemoveAt(currentRowCount);
            }

            grid.UpdateLayout();
        }

        private static void ColumnCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var grid = (Grid)d;
            var newColumnCount = (int)e.NewValue;
            var currentColumnCount = grid.RowDefinitions.Count;

            while (newColumnCount > currentColumnCount) {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                currentColumnCount++;
            }

            while (newColumnCount < currentColumnCount) {
                currentColumnCount--;
                grid.RowDefinitions.RemoveAt(currentColumnCount);
            }

            grid.UpdateLayout();
        }
    }
}