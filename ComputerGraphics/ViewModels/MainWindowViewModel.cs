using ComputerGraphics.Models;
using ComputerGraphics.MVVM;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ComputerGraphics.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<UIElement> Shapes { get; set; }

        private OperationType? _currentOperationType;
        private Point? _initialPosition;
        private Thickness? _initialMargin;

        public ICommand SetOperationCommand { get; set; }
        public ICommand MouseDownCommand { get; set; }
        public ICommand MouseMoveCommand { get; set; }
        public ICommand MouseUpCommand { get; set; }

        private UIElement? _currentShape;
        private UIElement? _selectedShape;
        private TextBox? _textBox;

        private readonly IShapeFactory _shapeFactory;
        private readonly IShapeUpdater _shapeUpdater;

        public MainWindowViewModel(IShapeFactory shapeFactory, IShapeUpdater shapeUpdater)
        {
            _shapeFactory = shapeFactory;
            _shapeUpdater = shapeUpdater;

            Shapes = new ObservableCollection<UIElement>();

            SetOperationCommand = new RelayCommand(SetOperation);
            MouseDownCommand = new RelayCommand(OnMouseDown);
            MouseMoveCommand = new RelayCommand(OnMouseMove);
            MouseUpCommand = new RelayCommand(OnMouseUp);

            ResetState();
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
            if (parameter is MouseButtonEventArgs mouseEventArgs && mouseEventArgs.Source is Canvas canvas)
            {
                _initialPosition = mouseEventArgs.GetPosition(canvas);
                if (_textBox != null && !_textBox.IsMouseOver)
                {
                    ReplaceTextBoxWithTextBlock();
                }
                switch (_currentOperationType)
                {
                    case OperationType.select:
                        _selectedShape = GetShapeAtPoint(_initialPosition.Value);
                        if (_selectedShape is FrameworkElement frameworkElement)
                        {
                            _initialMargin = frameworkElement.Margin;
                        }
                        break;
                    case OperationType.drawRectangle:
                    case OperationType.drawEllipse:
                    case OperationType.drawLine:
                        _currentShape = _shapeFactory.CreateShape(_currentOperationType.Value, _initialPosition.Value);
                        if (_currentShape != null)
                        {
                            Shapes.Add(_currentShape);
                        }
                        break;
                    case OperationType.addText:
                        if (_textBox == null)
                        {
                            _textBox = new TextBox
                            {
                                Width = 0,
                                Height = 0,
                                AcceptsReturn = true,
                                Margin = new Thickness(_initialPosition.Value.X, _initialPosition.Value.Y, 0, 0),
                                Background = Brushes.Transparent
                            };
                            Shapes.Add(_textBox);
                        }
                        break;
                }
            }
        }

        private void OnMouseMove(object? parameter)
        {
            if (parameter is MouseEventArgs mouseEventArgs && mouseEventArgs.Source is Canvas canvas)
            {
                var currentPosition = mouseEventArgs.GetPosition(canvas);

                if (_initialPosition.HasValue)
                {
                    switch (_currentOperationType)
                    {
                        case OperationType.select:
                            if (_selectedShape is FrameworkElement element && _initialMargin.HasValue)
                            {
                                var offsetX = currentPosition.X - _initialPosition.Value.X;
                                var offsetY = currentPosition.Y - _initialPosition.Value.Y;
                                element.Margin = new Thickness(
                                    _initialMargin.Value.Left + offsetX,
                                    _initialMargin.Value.Top + offsetY,
                                    0,
                                    0
                                );
                            }
                            break;
                        case OperationType.drawRectangle:
                        case OperationType.drawEllipse:
                        case OperationType.drawLine:
                            if (_currentShape != null)
                            {
                                _shapeUpdater.UpdateShape(_currentShape, _initialPosition.Value, currentPosition);
                            }
                            break;
                        case OperationType.addText:
                            if (_textBox != null)
                            {
                                _textBox.Width = Math.Abs(currentPosition.X - _initialPosition.Value.X);
                                _textBox.Height = Math.Abs(currentPosition.Y - _initialPosition.Value.Y);
                                _textBox.Margin = new Thickness(
                                    Math.Min(_initialPosition.Value.X, currentPosition.X),
                                    Math.Min(_initialPosition.Value.Y, currentPosition.Y),
                                    0,
                                    0
                                );
                            }
                            break;
                    }
                }
            }
        }

        private void OnMouseUp(object? parameter)
        {
            if (_textBox != null)
            {
                _textBox.Focus();
            }

            ResetState();
        }

        private void ResetState()
        {
            _currentShape = null;
            _selectedShape = null;
            _initialPosition = null;
            _initialMargin = null;
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

        private void ReplaceTextBoxWithTextBlock()
        {
            if (_textBox != null)
            {
                var textBlock = new TextBlock
                {
                    Text = _textBox.Text,
                    Margin = _textBox.Margin,
                    TextWrapping = TextWrapping.Wrap
                };

                Shapes.Remove(_textBox);
                Shapes.Add(textBlock);
                _textBox = null;
            }
        }
    }
}
