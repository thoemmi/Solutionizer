using System.Windows.Controls;
using Solutionizer.ViewModels;

namespace Solutionizer.Views {
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl {
        public MainView() {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}