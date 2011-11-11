using System.Windows;
using Solutionizer.ViewModels;

namespace Solutionizer.Views {
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window {
        public MainView() {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}