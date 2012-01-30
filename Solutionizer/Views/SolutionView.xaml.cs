using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Solutionizer.Extensions;
using Solutionizer.Models;
using Solutionizer.VisualStudio;

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
                var visual = e.OriginalSource as Visual;
                var parentTreeViewItem = visual.TryFindParent<TreeViewItem>();

                var selectedItem = _tree.SelectedItem as SolutionItem;
                if (selectedItem == null) {
                    return;
                }

                var index = parentTreeViewItem != null ? parentTreeViewItem.Items.IndexOf(selectedItem) : -1;

                var viewModel = (SolutionViewModel) DataContext;
                viewModel.RemoveSolutionItemCommand.Execute(selectedItem);

                if (index >= 0) {
                    if (index >= parentTreeViewItem.Items.Count) {
                        index--;
                    }
                    if (index >= 0) {
                        ((TreeViewItem) parentTreeViewItem.ItemContainerGenerator.ContainerFromItem(parentTreeViewItem.Items[index])).
                            IsSelected = true;
                    } else {
                        parentTreeViewItem.IsSelected = true;
                    }
                }
            }
        }
    }
}