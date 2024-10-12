using ComputerGraphics.Models;
using ComputerGraphics.ViewModels;
using System.Windows;

namespace ComputerGraphics
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            IElementsFactory shapeFactory = new ElementsFactory();
            IElementUpdater shapeUpdater = new ElementUpdater();
            MainWindowViewModel viewModel = new MainWindowViewModel(shapeFactory, shapeUpdater);
            DataContext = viewModel;
        }
    }
}
