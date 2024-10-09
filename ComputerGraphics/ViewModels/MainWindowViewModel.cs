using ComputerGraphics.Models;
using ComputerGraphics.MVVM;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace ComputerGraphics.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<UIElement> Shapes { get; set; }
        public ICommand SetOperationCommand { get; set; }
        private OperationType _currentOperationType;

        // Necessary for drawing shapes
        public ICommand MouseDownCommand { get; set; }
        public ICommand MouseMoveCommand { get; set; }
        public ICommand MouseUpCommand { get; set; }
        private UIElement? _currentShape;
        private Point _startPoint;

        // Necessary for changing the position of the shape
        private UIElement? _selectedShape;
        private Point _initialMousePosition;
        private Thickness _initialMargin;

        public MainWindowViewModel()
        {
            Shapes = new ObservableCollection<UIElement>();
            SetOperationCommand = new RelayCommand(SetOperation);
            MouseDownCommand = new RelayCommand(OnMouseDown);
            MouseMoveCommand = new RelayCommand(OnMouseMove);
            MouseUpCommand = new RelayCommand(OnMouseUp);
        }

        private void SetOperation(object? parameter)
        {
            if (parameter is OperationType operationType)
            {
                _currentOperationType = operationType;
            }
        }

        private void OnMouseDown(object? parameter)
        {
            if (parameter is MouseButtonEventArgs mouseEventArgs)
            {
                if (mouseEventArgs.Source is Canvas canvas)
                {
                    _startPoint = mouseEventArgs.GetPosition(canvas);
                    if (_currentOperationType == OperationType.select)
                    {
                        _selectedShape = GetShapeAtPoint(_startPoint);
                        if (_selectedShape != null)
                        {
                            _initialMousePosition = _startPoint;
                            if (_selectedShape is FrameworkElement frameworkElement)
                            {
                                _initialMargin = frameworkElement.Margin;
                            }
                        }
                    }
                    else
                    {
                        _currentShape = CreateShape(_startPoint, _startPoint);
                        if (_currentShape != null)
                        {
                            Shapes.Add(_currentShape);
                        }
                    }
                }
            }
        }

        private void OnMouseMove(object? parameter)
        {
            if (parameter is MouseEventArgs mouseEventArgs)
            {
                if (mouseEventArgs.Source is Canvas canvas)
                {
                    var currentPoint = mouseEventArgs.GetPosition(canvas);

                    if (_selectedShape != null && _currentOperationType == OperationType.select)
                    {
                        if (_selectedShape is FrameworkElement element)
                        {
                            var offsetX = currentPoint.X - _initialMousePosition.X;
                            var offsetY = currentPoint.Y - _initialMousePosition.Y;
                            element.Margin = new Thickness(
                                _initialMargin.Left + offsetX,
                                _initialMargin.Top + offsetY,
                                0,
                                0
                            );
                        }
                    }
                    else if (_currentShape != null)
                    {
                        UpdateShape(_currentShape, _startPoint, currentPoint);
                        OnPropertyChanged(nameof(Shapes));
                    }
                }
            }
        }

        private void OnMouseUp(object? parameter)
        {
            _currentShape = null;
            _selectedShape = null;
        }

        private UIElement? CreateShape(Point startPoint, Point endPoint)
        {
            UIElement? shape = null;
            switch (_currentOperationType)
            {
                case OperationType.drawRectangle:
                    shape = new Rectangle
                    {
                        Stroke = System.Windows.Media.Brushes.Black,
                        StrokeThickness = 2,
                        Width = 0,
                        Height = 0,
                        Margin = new Thickness(startPoint.X, startPoint.Y, 0, 0)
                    };
                    break;
                case OperationType.drawEllipse:
                    shape = new Ellipse
                    {
                        Stroke = System.Windows.Media.Brushes.Black,
                        StrokeThickness = 2,
                        Width = 0,
                        Height = 0,
                        Margin = new Thickness(startPoint.X, startPoint.Y, 0, 0)
                    };
                    break;
                case OperationType.drawLine:
                    shape = new Line
                    {
                        Stroke = System.Windows.Media.Brushes.Black,
                        StrokeThickness = 2,
                        X1 = startPoint.X,
                        Y1 = startPoint.Y,
                        X2 = startPoint.X,
                        Y2 = startPoint.Y
                    };
                    break;
                case OperationType.addText:
                    shape = new TextBlock
                    {
                        Text = "Sample Text",
                        Margin = new Thickness(startPoint.X, startPoint.Y, 0, 0)
                    };
                    break;
            }

            return shape;
        }

        private void UpdateShape(UIElement shape, Point startPoint, Point endPoint)
        {
            if (shape is Rectangle rectangle)
            {
                rectangle.Width = Math.Abs(endPoint.X - startPoint.X);
                rectangle.Height = Math.Abs(endPoint.Y - startPoint.Y);
                rectangle.Margin = new Thickness(Math.Min(startPoint.X, endPoint.X), Math.Min(startPoint.Y, endPoint.Y), 0, 0);
            }
            else if (shape is Ellipse ellipse)
            {
                ellipse.Width = Math.Abs(endPoint.X - startPoint.X);
                ellipse.Height = Math.Abs(endPoint.Y - startPoint.Y);
                ellipse.Margin = new Thickness(Math.Min(startPoint.X, endPoint.X), Math.Min(startPoint.Y, endPoint.Y), 0, 0);
            }
            else if (shape is Line line)
            {
                line.X2 = endPoint.X;
                line.Y2 = endPoint.Y;
            }
            else if (shape is TextBlock textBlock)
            {
                textBlock.Margin = new Thickness(startPoint.X, startPoint.Y, 0, 0);
            }
        }

        private UIElement? GetShapeAtPoint(Point point)
        {
            for (var i = Shapes.Count - 1; i >= 0; i--)
            {
                if (Shapes[i] is FrameworkElement element)
                {
                    var left = element.Margin.Left;
                    var top = element.Margin.Top;
                    var right = left + element.Width;
                    var bottom = top + element.Height;

                    if (element is Shape shape)
                    {
                        var strokeThickness = shape.StrokeThickness;
                        left -= strokeThickness / 2;
                        top -= strokeThickness / 2;
                        right += strokeThickness / 2;
                        bottom += strokeThickness / 2;
                    }

                    if (point.X >= left && point.X <= right && point.Y >= top && point.Y <= bottom)
                    {
                        return Shapes[i];
                    }
                }
            }
            return null;
        }
    }
}
