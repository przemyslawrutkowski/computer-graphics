using System.Windows;
using System.Windows.Shapes;

namespace ComputerGraphics.Models
{
    public interface IShapeUpdater
    {
        void UpdateShape(UIElement shape, Point initialPosition, Point finalPosition);
    }

    public class ShapeUpdater : IShapeUpdater
    {
        public void UpdateShape(UIElement shape, Point initialPosition, Point finalPosition)
        {
            switch (shape)
            {
                case Rectangle rectangle:
                    rectangle.Width = Math.Abs(finalPosition.X - initialPosition.X);
                    rectangle.Height = Math.Abs(finalPosition.Y - initialPosition.Y);
                    rectangle.Margin = new Thickness(Math.Min(initialPosition.X, finalPosition.X), Math.Min(initialPosition.Y, finalPosition.Y), 0, 0);
                    break;
                case Ellipse ellipse:
                    ellipse.Width = Math.Abs(finalPosition.X - initialPosition.X);
                    ellipse.Height = Math.Abs(finalPosition.Y - initialPosition.Y);
                    ellipse.Margin = new Thickness(Math.Min(initialPosition.X, finalPosition.X), Math.Min(initialPosition.Y, finalPosition.Y), 0, 0);
                    break;
                case Line line:
                    line.X2 = finalPosition.X;
                    line.Y2 = finalPosition.Y;
                    break;
                default:
                    throw new NotSupportedException($"Shape type {shape.GetType()} is not supported");
            }
        }
    }
}
