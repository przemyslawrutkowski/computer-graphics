using ComputerGraphics.Models;
using ComputerGraphics.MVVM;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Shapes;

namespace ComputerGraphics.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<Element> Elements { get; set; }

        private Operation? _currentOperation;
        private Element? _selectedElement;
        private Point? _initialPosition;

        public DelegateCommand SetOperationCommand { get; set; }

        private readonly IElementsFactory _elementsFactory;
        private readonly IElementUpdater _elementUpdater;

        public MainWindowViewModel(IElementsFactory elementsFactory, IElementUpdater elementUpdater)
        {
            _elementsFactory = elementsFactory;
            _elementUpdater = elementUpdater;

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
                switch (_currentOperation)
                {
                    case Operation.select:
                        _selectedElement = GetShapeAtPoint(currentPosition);
                        if (_selectedElement != null)
                        {
                            _initialPosition = currentPosition;
                        }
                        break;
                    case Operation.drawRectangle:
                    case Operation.drawEllipse:
                    case Operation.drawLine:
                        _selectedElement = _elementsFactory.CreateElement(_currentOperation.Value, currentPosition);
                        _initialPosition = currentPosition;
                        Elements.Add(_selectedElement);
                        break;
                    case Operation.addText:
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
                        break;
                    case Operation.drawRectangle:
                    case Operation.drawEllipse:
                    case Operation.drawLine:
                        if (_selectedElement != null && _initialPosition.HasValue)
                        {
                            _elementUpdater.UpdateElement(_selectedElement, _initialPosition.Value, currentPosition);
                        }
                        break;
                    case Operation.addText:
                        break;
                }
            }
        }

        public void OnMouseUp(object? parameter)
        {
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
                    var left = frameworkElement.Margin.Left;
                    var top = frameworkElement.Margin.Top;
                    var right = left + frameworkElement.Width;
                    var bottom = top + frameworkElement.Height;

                    if (frameworkElement is Shape shape)
                    {
                        var strokeThickness = shape.StrokeThickness;
                        left -= strokeThickness / 2;
                        top -= strokeThickness / 2;
                        right += strokeThickness / 2;
                        bottom += strokeThickness / 2;
                    }

                    if (point.X >= left && point.X <= right && point.Y >= top && point.Y <= bottom)
                    {
                        return element;
                    }
                }
            }
            return null;
        }

    }
}
