using ComputerGraphics.Models;
using ComputerGraphics.MVVM;
using ComputerGraphics.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ComputerGraphics.ViewModels
{
    public class GraphicsOperationsViewModel : ViewModelBase
    {
        private readonly IElementFactory _elementsFactory;
        private readonly IElementUpdater _elementUpdater;
        private readonly IImageService _imageService;
        private Operation? _currentOperation;
        private Element? _selectedElement;
        private Point? _initialPosition;
        private Element? _textBox;

        public ObservableCollection<Element> Elements { get; }
        public DelegateCommand SetOperationCommand { get; }

        public GraphicsOperationsViewModel(IElementFactory elementFactory, IElementUpdater elementUpdater, IImageService imageService)
        {
            _elementsFactory = elementFactory;
            _elementUpdater = elementUpdater;
            _imageService = imageService;

            Elements = new ObservableCollection<Element>();
            SetOperationCommand = new DelegateCommand(SetOperation);
        }

        private void SetOperation(object? parameter)
        {
            if (parameter is Operation operationType)
            {
                _currentOperation = operationType;
            }
        }

        public void OnMouseDown(object? parameter)
        {
            if (parameter is Point currentPosition)
            {
                if (_textBox != null && IsFocusLost(_textBox, currentPosition))
                {
                    OnTextBoxFocusLost();
                }

                switch (_currentOperation)
                {
                    case Operation.select:
                        _selectedElement = GetShapeAtPoint(currentPosition);
                        if (_selectedElement != null)
                        {
                            _initialPosition = currentPosition;
                        }
                        break;
                    case Operation.drawTriangle:
                    case Operation.drawRectangle:
                    case Operation.drawEllipse:
                    case Operation.drawLine:
                        _selectedElement = _elementsFactory.CreateElement(_currentOperation.Value, currentPosition);
                        _initialPosition = currentPosition;
                        Elements.Add(_selectedElement);
                        break;
                    case Operation.addText:
                        _selectedElement = _elementsFactory.CreateElement(_currentOperation.Value, currentPosition);
                        _initialPosition = currentPosition;
                        _textBox = _selectedElement;
                        Elements.Add(_selectedElement);
                        break;
                }
            }
        }

        public void OnMouseMove(object? parameter)
        {
            if (parameter is Point currentPosition)
            {
                switch (_currentOperation)
                {
                    case Operation.select:
                        if (_selectedElement != null && _initialPosition.HasValue)
                        {
                            _elementUpdater.MoveElement(_selectedElement, _initialPosition.Value, currentPosition);
                            _initialPosition = currentPosition;
                        }
                        break;
                    case Operation.drawTriangle:
                    case Operation.drawRectangle:
                    case Operation.drawEllipse:
                    case Operation.drawLine:
                        if (_selectedElement != null && _initialPosition.HasValue)
                        {
                            _elementUpdater.SetElement(_selectedElement, _initialPosition.Value, currentPosition);
                        }
                        break;
                    case Operation.addText:
                        if (_selectedElement != null && _initialPosition.HasValue)
                        {
                            _elementUpdater.SetElement(_selectedElement, _initialPosition.Value, currentPosition);
                        }
                        break;
                }
            }
        }

        public void OnMouseUp(object? parameter)
        {
            if (_textBox != null && _textBox.UIElement is TextBox textBox)
            {
                textBox.Focus();
            }
            ResetState();
        }

        private void ResetState()
        {
            _selectedElement = null;
            _initialPosition = null;
        }

        private Element? GetShapeAtPoint(Point point)
        {
            foreach (var element in Elements)
            {
                if (element.UIElement is FrameworkElement frameworkElement)
                {
                    if (frameworkElement is Polygon triangle)
                    {
                        if (IsPointInPolygon(point, triangle.Points))
                        {
                            return element;
                        }
                    }
                    else
                    {
                        var left = element.X;
                        var top = element.Y;
                        var right = left + frameworkElement.Width;
                        var bottom = top + frameworkElement.Height;

                        if (point.X >= left && point.X <= right && point.Y >= top && point.Y <= bottom)
                        {
                            return element;
                        }
                    }
                }
            }
            return null;
        }

        private bool IsPointInPolygon(Point point, PointCollection polygonPoints)
        {
            int numVertices = polygonPoints.Count;
            double x = point.X;
            double y = point.Y;
            bool inside = false;

            Point p1 = polygonPoints[0];
            Point p2;

            for (int i = 1; i <= numVertices; i++)
            {
                p2 = polygonPoints[i % numVertices];

                if (y > Math.Min(p1.Y, p2.Y))
                {
                    if (y <= Math.Max(p1.Y, p2.Y))
                    {
                        if (x <= Math.Max(p1.X, p2.X))
                        {
                            double xIntersection = (y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y) + p1.X;

                            if (p1.X == p2.X || x <= xIntersection)
                            {
                                inside = !inside;
                            }
                        }
                    }
                }
                p1 = p2;
            }

            return inside;
        }

        private void OnTextBoxFocusLost()
        {
            if (_textBox != null && _textBox.UIElement is TextBox textBox)
            {
                var textBlock = new TextBlock
                {
                    Text = textBox.Text,
                };
                var element = new Element(textBlock)
                {
                    X = _textBox.X,
                    Y = _textBox.Y
                };
                Elements.Remove(_textBox);
                Elements.Add(element);
                _textBox = null;
            }
        }

        private bool IsFocusLost(Element element, Point currentPosition)
        {
            if (element != null && element.UIElement is FrameworkElement frameworkElement)
            {
                var left = element.X;
                var top = element.Y;
                var right = left + frameworkElement.Width;
                var bottom = top + frameworkElement.Height;

                return currentPosition.X < left || currentPosition.X > right || currentPosition.Y < top || currentPosition.Y > bottom;
            }
            return true;
        }

        public void SaveImage(RenderTargetBitmap bitmap)
        {
            _imageService.SaveImage(bitmap);
        }
    }
}
