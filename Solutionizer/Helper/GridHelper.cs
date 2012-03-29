using System.Windows;
using System.Windows.Controls;

namespace Solutionizer.Helper {
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

        private static void RowCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var grid = d as Grid;
            var rowCount = (int)e.NewValue;
            RecreateGridCells(grid, rowCount);
        }

        private static void RecreateGridCells(Grid grid, int numRows) {
            int currentNumRows = grid.RowDefinitions.Count;

            while (numRows > currentNumRows) {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                currentNumRows++;
            }

            while (numRows < currentNumRows) {
                currentNumRows--;
                grid.RowDefinitions.RemoveAt(currentNumRows);
            }

            grid.UpdateLayout();
        }
    }
}