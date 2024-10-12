using ComputerGraphics.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ComputerGraphics.Controls
{
    /// <summary>
    /// Interaction logic for CanvasControl.xaml
    /// </summary>
    public partial class CanvasControl : UserControl
    {
        public CanvasControl()
        {
            InitializeComponent();
        }

        private void HandleMouseEvent(object sender, MouseEventArgs e, Action<MainWindowViewModel, Point> viewModelAction)
        {
            if (sender is Canvas canvas)
            {
                Point currentPosition = e.GetPosition(canvas);
                if (this.DataContext is MainWindowViewModel viewModel)
                {
                    viewModelAction(viewModel, currentPosition);
                }
            }
        }

        private void MouseDownEventHandler(object sender, MouseEventArgs e)
        {
            HandleMouseEvent(sender, e, (viewModel, currentPosition) => viewModel.OnMouseDown(currentPosition));
        }

        private void MouseMoveEventHandler(object sender, MouseEventArgs e)
        {
            HandleMouseEvent(sender, e, (viewModel, currentPosition) => viewModel.OnMouseMove(currentPosition));
        }

        private void MouseUpEventHandler(object sender, MouseEventArgs e)
        {
            HandleMouseEvent(sender, e, (viewModel, currentPosition) => viewModel.OnMouseUp(currentPosition));
        }
    }
}
