using ComputerGraphics.Models;
using ComputerGraphics.MVVM;
using ComputerGraphics.Services;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ComputerGraphics.ViewModels
{
    public class ColorConverterViewModel : ViewModelBase
    {
        private readonly IColorService _colorService;
        private SolidColorBrush _selectedColor;
        private bool _isUpdating;

        private WriteableBitmap _colorsMapBitmap;
        private byte[] _pixelsBuffer;

        public ImageSource ColorsMapImage => _colorsMapBitmap;

        public RGBColor CurrentRGBColor { get; set; }
        public CMYKColor CurrentCMYKColor { get; set; }
        public HSVColor CurrentHSVColor { get; set; }
        public SolidColorBrush SelectedColor
        {
            get => _selectedColor;
            set
            {
                if (_selectedColor != value)
                {
                    _selectedColor = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ColorConverterViewModel(IColorService colorService)
        {
            _colorService = colorService;

            CurrentRGBColor = new RGBColor();
            CurrentCMYKColor = new CMYKColor();
            CurrentHSVColor = new HSVColor();

            CurrentRGBColor.PropertyChanged += (s, e) => UpdateColorsFromRGB();
            CurrentCMYKColor.PropertyChanged += (s, e) => UpdateColorsFromCMYK();
            CurrentHSVColor.PropertyChanged += (s, e) => UpdateColorsFromHSV();

            const int width = 256;
            const int height = 256;
            _colorsMapBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
            _pixelsBuffer = new byte[height * width * 4];

            GenerateColorsMapImage();

            _selectedColor = new SolidColorBrush(_colorService.ConvertFromRGB(CurrentRGBColor));
        }

        private void GenerateColorsMapImage()
        {
            int width = _colorsMapBitmap.PixelWidth;
            int height = _colorsMapBitmap.PixelHeight;
            int stride = width * 4;

            double hue = CurrentHSVColor.Hue;

            Parallel.For(0, height, y =>
            {
                double value = 1.0 - (double)y / (height - 1);
                int yStride = y * stride;

                for (int x = 0; x < width; x++)
                {
                    double saturation = (double)x / (width - 1);

                    HSVColor hsvColor = new HSVColor
                    {
                        Hue = hue,
                        Saturation = saturation * 100,
                        Value = value * 100
                    };

                    Color color = _colorService.ConvertFromHSV(hsvColor);

                    int index = yStride + x * 4;

                    _pixelsBuffer[index + 0] = color.B;
                    _pixelsBuffer[index + 1] = color.G;
                    _pixelsBuffer[index + 2] = color.R;
                    _pixelsBuffer[index + 3] = color.A;
                }
            });

            _colorsMapBitmap.WritePixels(new Int32Rect(0, 0, width, height), _pixelsBuffer, stride, 0);

            RaisePropertyChanged(nameof(ColorsMapImage));
        }

        public void OnMouseDown(Point position)
        {
            UpdateColorFromPosition(position);
        }

        public void OnMouseMove(Point position)
        {
            UpdateColorFromPosition(position);
        }

        private void UpdateColorFromPosition(Point position)
        {
            int width = _colorsMapBitmap.PixelWidth;
            int height = _colorsMapBitmap.PixelHeight;

            double x = Math.Clamp(position.X, 0, width - 1);
            double y = Math.Clamp(position.Y, 0, height - 1);

            double saturation = x / (width - 1);
            double value = 1.0 - y / (height - 1);

            if (_isUpdating) return;
            _isUpdating = true;

            CurrentHSVColor.Saturation = saturation * 100;
            CurrentHSVColor.Value = value * 100;

            var color = _colorService.ConvertFromHSV(CurrentHSVColor);
            var rgbColor = _colorService.ConvertToRGB(color);
            var cmykColor = _colorService.ConvertToCMYK(color);

            CurrentRGBColor.UpdateFrom(rgbColor);
            CurrentCMYKColor.UpdateFrom(cmykColor);

            SelectedColor = new SolidColorBrush(color);

            _isUpdating = false;
        }

        private void UpdateColorsFromRGB()
        {
            if (_isUpdating) return;
            _isUpdating = true;

            var color = _colorService.ConvertFromRGB(CurrentRGBColor);
            var cmykColor = _colorService.ConvertToCMYK(color);
            var hsvColor = _colorService.ConvertToHSV(color);

            CurrentCMYKColor.UpdateFrom(cmykColor);
            CurrentHSVColor.UpdateFrom(hsvColor);

            SelectedColor = new SolidColorBrush(color);

            GenerateColorsMapImage();

            _isUpdating = false;
        }

        private void UpdateColorsFromCMYK()
        {
            if (_isUpdating) return;
            _isUpdating = true;

            var color = _colorService.ConvertFromCMYK(CurrentCMYKColor);
            var rgbColor = _colorService.ConvertToRGB(color);
            var hsvColor = _colorService.ConvertToHSV(color);

            CurrentRGBColor.UpdateFrom(rgbColor);
            CurrentHSVColor.UpdateFrom(hsvColor);

            SelectedColor = new SolidColorBrush(color);

            GenerateColorsMapImage();

            _isUpdating = false;
        }

        private void UpdateColorsFromHSV()
        {
            if (_isUpdating) return;
            _isUpdating = true;

            var color = _colorService.ConvertFromHSV(CurrentHSVColor);
            var rgbColor = _colorService.ConvertToRGB(color);
            var cmykColor = _colorService.ConvertToCMYK(color);

            CurrentRGBColor.UpdateFrom(rgbColor);
            CurrentCMYKColor.UpdateFrom(cmykColor);

            SelectedColor = new SolidColorBrush(color);

            GenerateColorsMapImage();

            _isUpdating = false;
        }
    }
}
