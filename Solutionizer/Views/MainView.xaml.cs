namespace Solutionizer.Views {
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView {
        public MainView() {
            InitializeComponent();
        }

        private void Grid_MouseDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            DragMove();
        }
    }
}