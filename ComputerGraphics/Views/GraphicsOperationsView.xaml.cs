using ComputerGraphics.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ComputerGraphics.Views
{
    public partial class GraphicsOperationsView : UserControl
    {
        public GraphicsOperationsView()
        {
            InitializeComponent();
        }

        private void HandleMouseEvent(object sender, MouseEventArgs e, Action<GraphicsOperationsViewModel, Point> viewModelAction)
        {
            if (sender is Canvas canvas)
            {
                Point currentPosition = e.GetPosition(canvas);
                if (DataContext is GraphicsOperationsViewModel viewModel)
                {
                    viewModelAction(viewModel, currentPosition);
                }
            }
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            HandleMouseEvent(sender, e, (viewModel, currentPosition) => viewModel.OnMouseDown(currentPosition));
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            HandleMouseEvent(sender, e, (viewModel, currentPosition) => viewModel.OnMouseMove(currentPosition));
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            HandleMouseEvent(sender, e, (viewModel, currentPosition) => viewModel.OnMouseUp(currentPosition));
        }

        private void OnSaveImage(object Sender, RoutedEventArgs e)
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)MainCanvas.ActualWidth, (int)MainCanvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(MainCanvas);
            if (DataContext is GraphicsOperationsViewModel viewModel)
            {
                viewModel.SaveImage(bitmap);
            }
        }
    }
}
