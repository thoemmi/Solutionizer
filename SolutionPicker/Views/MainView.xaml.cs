using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SolutionPicker.ViewModels;

namespace SolutionPicker.Views {
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