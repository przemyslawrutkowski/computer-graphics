using ComputerGraphics.Models;
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
            IShapeFactory shapeFactory = new ShapeFactory();
            IShapeUpdater shapeUpdater = new ShapeUpdater();
            MainWindowViewModel viewModel = new MainWindowViewModel(shapeFactory, shapeUpdater);
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
