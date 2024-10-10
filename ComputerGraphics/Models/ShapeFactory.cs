using System.Windows;
using System.Windows.Shapes;

namespace ComputerGraphics.Models
{
    public interface IShapeFactory
    {
        UIElement CreateShape(OperationType operationType, Point startPoint);
    }

    public class ShapeFactory : IShapeFactory
    {
        public UIElement CreateShape(OperationType operationType, Point startPoint)
        {
            return operationType switch
            {
                OperationType.drawRectangle => new Rectangle
                {
                    Stroke = System.Windows.Media.Brushes.Black,
                    StrokeThickness = 2,
                    Width = 0,
                    Height = 0,
                    Margin = new Thickness(startPoint.X, startPoint.Y, 0, 0)
                },
                OperationType.drawEllipse => new Ellipse
                {
                    Stroke = System.Windows.Media.Brushes.Black,
                    StrokeThickness = 2,
                    Width = 0,
                    Height = 0,
                    Margin = new Thickness(startPoint.X, startPoint.Y, 0, 0)
                },
                OperationType.drawLine => new Line
                {
                    Stroke = System.Windows.Media.Brushes.Black,
                    StrokeThickness = 2,
                    X1 = startPoint.X,
                    Y1 = startPoint.Y,
                    X2 = startPoint.X,
                    Y2 = startPoint.Y
                },
                _ => throw new NotSupportedException($"OperationType {operationType} is not supported")
            };
        }
    }
}
