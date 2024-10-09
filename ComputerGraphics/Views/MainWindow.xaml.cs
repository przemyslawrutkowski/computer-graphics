using ComputerGraphics.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ComputerGraphics.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainWindowViewModel viewModel = new MainWindowViewModel();
            DataContext = viewModel;
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.MouseDownCommand.Execute(e);
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.MouseMoveCommand.Execute(e);
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.MouseUpCommand.Execute(e);
            }
        }
    }
}
