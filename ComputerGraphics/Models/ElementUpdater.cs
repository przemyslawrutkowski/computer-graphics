using System.Windows;
using System.Windows.Shapes;

namespace ComputerGraphics.Models
{
    public interface IElementUpdater
    {
        void UpdateElement(Element element, Point initialPosition, Point finalPosition);
    }

    public class ElementUpdater : IElementUpdater
    {
        public void UpdateElement(Element element, Point initialPosition, Point currentPosition)
        {
            switch (element.UIElement)
            {
                case Rectangle rectangle:
                    rectangle.Width = Math.Abs(currentPosition.X - initialPosition.X);
                    rectangle.Height = Math.Abs(currentPosition.Y - initialPosition.Y);
                    element.X = Math.Min(initialPosition.X, currentPosition.X);
                    element.Y = Math.Min(initialPosition.Y, currentPosition.Y);
                    break;
                case Ellipse ellipse:
                    ellipse.Width = Math.Abs(currentPosition.X - initialPosition.X);
                    ellipse.Height = Math.Abs(currentPosition.Y - initialPosition.Y);
                    element.X = Math.Min(initialPosition.X, currentPosition.X);
                    element.Y = Math.Min(initialPosition.Y, currentPosition.Y);
                    break;
                case Line line:
                    double x1 = initialPosition.X;
                    double y1 = initialPosition.Y;
                    double x2 = currentPosition.X;
                    double y2 = currentPosition.Y;

                    line.X1 = x1 - element.X;
                    line.Y1 = y1 - element.Y;
                    line.X2 = x2 - element.X;
                    line.Y2 = y2 - element.Y;

                    element.X = Math.Min(x1, x2);
                    element.Y = Math.Min(y1, y2);
                    break;
                default:
                    throw new NotSupportedException($"Element type {element.GetType()} is not supported");
            }
        }
    }
}
