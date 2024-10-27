using ComputerGraphics.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ComputerGraphics.Views
{
    public partial class ColorConverterView : UserControl
    {
        public ColorConverterView()
        {
            InitializeComponent();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ColorConverterViewModel viewModel)
            {
                var position = e.GetPosition(ColorsMap);
                if (IsWithinImageBounds(position))
                {
                    viewModel.OnMouseDown(position);
                }
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (DataContext is ColorConverterViewModel viewModel && e.LeftButton == MouseButtonState.Pressed)
            {
                var position = e.GetPosition(ColorsMap);
                if (IsWithinImageBounds(position))
                {
                    viewModel.OnMouseMove(position);
                }
            }
        }

        private bool IsWithinImageBounds(Point position)
        {
            return position.X >= 0 && position.X <= ColorsMap.ActualWidth &&
                   position.Y >= 0 && position.Y <= ColorsMap.ActualHeight;
        }
    }
}
