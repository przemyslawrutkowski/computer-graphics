using ComputerGraphics.Models;
using ComputerGraphics.MVVM;
using ComputerGraphics.Services;
using System.Windows.Media;

namespace ComputerGraphics.ViewModels
{
    public class ColorSpacesViewModel : ViewModelBase
    {
        private readonly IColorService _colorService;
        private SolidColorBrush _selectedColor;
        private RGBColor _currentRGBColor;
        private CMYKColor _currentCMYKColor;
        private HSVColor _currentHSVColor;

        public RGBColor CurrentRGBColor
        {
            get => _currentRGBColor;
            private set
            {
                _currentRGBColor = value;
                RaisePropertyChanged();
            }
        }
        public CMYKColor CurrentCMYKColor
        {
            get => _currentCMYKColor;
            private set
            {
                _currentCMYKColor = value;
                RaisePropertyChanged();
            }
        }
        public HSVColor CurrentHSVColor
        {
            get => _currentHSVColor;
            private set
            {
                _currentHSVColor = value;
                RaisePropertyChanged();
            }
        }
        public SolidColorBrush SelectedColor
        {
            get => _selectedColor;
            private set
            {
                _selectedColor = value;
                RaisePropertyChanged();
            }
        }
        public DelegateCommand ConvertCommand { get; }

        public ColorSpacesViewModel(IColorService colorService)
        {
            _colorService = colorService;

            _currentRGBColor = new RGBColor();
            _currentCMYKColor = new CMYKColor();
            _currentHSVColor = new HSVColor();
            _selectedColor = new SolidColorBrush(_colorService.ConvertFromRGB(CurrentRGBColor));
            ConvertCommand = new DelegateCommand(Convert);
        }

        private void Convert(object? parameter)
        {
            Color color;
            if (parameter is RGBColor)
            {
                color = _colorService.ConvertFromRGB(CurrentRGBColor);
                CurrentCMYKColor = _colorService.ConvertToCMYK(color);
                CurrentHSVColor = _colorService.ConvertToHSV(color);
            }
            else if (parameter is CMYKColor)
            {
                color = _colorService.ConvertFromCMYK(CurrentCMYKColor);
                CurrentRGBColor = _colorService.ConvertToRGB(color);
                CurrentHSVColor = _colorService.ConvertToHSV(color);
            }
            else if (parameter is HSVColor)
            {
                color = _colorService.ConvertFromHSV(CurrentHSVColor);
                CurrentRGBColor = _colorService.ConvertToRGB(color);
                CurrentCMYKColor = _colorService.ConvertToCMYK(color);
            }
            SelectedColor = new SolidColorBrush(color);
        }
    }
}
