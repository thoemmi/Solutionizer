using System.Windows.Controls;
using System.Windows.Input;
using Solutionizer.Models;

namespace Solutionizer.Views {
    /// <summary>
    /// Interaction logic for SolutionView.xaml
    /// </summary>
    public partial class SolutionView : UserControl {
        public SolutionView() {
            InitializeComponent();
        }

        private void TreeView_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Delete) {
                //var si = _tree.SelectedItem;
                var viewModel = (SolutionViewModel) DataContext;
                viewModel.RemoveSolutionItemCommand.Execute(_tree.SelectedItem);
            }
        }
    }
}