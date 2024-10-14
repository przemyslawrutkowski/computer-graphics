using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ComputerGraphics.Models
{
    public interface IElementsFactory
    {
        Element CreateElement(Operation operation, Point currentPosition);
    }

    public class ElementsFactory : IElementsFactory
    {
        public Element CreateElement(Operation operation, Point currentPosition)
        {
            UIElement uiElement;

            switch (operation)
            {
                case Operation.drawRectangle:
                    uiElement = new Rectangle
                    {
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        Width = 0,
                        Height = 0,
                    };
                    break;
                case Operation.drawTriangle:
                    uiElement = new Polygon
                    {
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        Points = new PointCollection
                        {
                            currentPosition, currentPosition, currentPosition
                        }
                    };
                    break;
                case Operation.drawEllipse:
                    uiElement = new Ellipse
                    {
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        Width = 0,
                        Height = 0,
                    };
                    break;
                case Operation.drawLine:
                    uiElement = new Line
                    {
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        X1 = currentPosition.X,
                        Y1 = currentPosition.Y,
                        X2 = currentPosition.X,
                        Y2 = currentPosition.Y
                    };
                    break;
                case Operation.addText:
                    uiElement = new TextBox
                    {
                        Width = 0,
                        Height = 0,
                        AcceptsReturn = true,
                        Background = Brushes.Transparent,
                    };
                    break;
                default:
                    throw new NotSupportedException($"Operation type {operation} is not supported");
            }

            if (operation == Operation.drawTriangle)
            {
                return new Element(uiElement)
                {
                    X = 0,
                    Y = 0
                };
            }
            else
            {
                return new Element(uiElement)
                {
                    X = currentPosition.X,
                    Y = currentPosition.Y
                };
            }

        }
    }
}
