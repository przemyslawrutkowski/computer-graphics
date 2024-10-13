using ComputerGraphics.Controls;
using ComputerGraphics.Models;
using ComputerGraphics.Services;
using ComputerGraphics.ViewModels;
using System.Windows;

namespace ComputerGraphics
{
    public partial class MainWindow : Window
    {
        private readonly ImagesService _imagesService = new ImagesService();

        public MainWindow()
        {
            InitializeComponent();
            IElementsFactory shapeFactory = new ElementsFactory();
            IElementUpdater shapeUpdater = new ElementUpdater();
            MainWindowViewModel viewModel = new MainWindowViewModel(shapeFactory, shapeUpdater);
            DataContext = viewModel;

            ToolbarControl.SaveImageEventHandler += SaveImage;
        }
        private void SaveImage(object? sender, EventArgs e)
        {
            _imagesService.SaveImage(CanvasControl.MainCanvas);
        }
    }
}
