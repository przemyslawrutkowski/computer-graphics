using ComputerGraphics.ViewModels;
using System.Windows;

namespace ComputerGraphics
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
