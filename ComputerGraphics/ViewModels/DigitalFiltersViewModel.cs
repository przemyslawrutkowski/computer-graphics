using ComputerGraphics.Models;
using ComputerGraphics.MVVM;
using ComputerGraphics.Services;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ComputerGraphics.ViewModels
{
    public class DigitalFiltersViewModel : ViewModelBase
    {
        private readonly IAsyncImageService _imageService;

        private BitmapImage? _originalImage;
        public BitmapImage? OriginalImage
        {
            get => _originalImage;
            set
            {
                if (_originalImage != value)
                {
                    _originalImage = value;
                    RaisePropertyChanged();
                    RaiseAllCanExecuteChanged();
                }
            }
        }

        private BitmapImage? _processedImage;
        public BitmapImage? ProcessedImage
        {
            get => _processedImage;
            set
            {
                if (_processedImage != value)
                {
                    _processedImage = value;
                    RaisePropertyChanged();
                    RaiseAllCanExecuteChanged();
                }
            }
        }

        private int _addR;
        public int AddR
        {
            get => _addR;
            set { if (_addR != value) { _addR = value; RaisePropertyChanged(); } }
        }

        private int _addG;
        public int AddG
        {
            get => _addG;
            set { if (_addG != value) { _addG = value; RaisePropertyChanged(); } }
        }

        private int _addB;
        public int AddB
        {
            get => _addB;
            set { if (_addB != value) { _addB = value; RaisePropertyChanged(); } }
        }

        private int _subtractR;
        public int SubtractR
        {
            get => _subtractR;
            set { if (_subtractR != value) { _subtractR = value; RaisePropertyChanged(); } }
        }

        private int _subtractG;
        public int SubtractG
        {
            get => _subtractG;
            set { if (_subtractG != value) { _subtractG = value; RaisePropertyChanged(); } }
        }

        private int _subtractB;
        public int SubtractB
        {
            get => _subtractB;
            set { if (_subtractB != value) { _subtractB = value; RaisePropertyChanged(); } }
        }

        private double _multiplyR = 1.0;
        public double MultiplyR
        {
            get => _multiplyR;
            set { if (_multiplyR != value) { _multiplyR = value; RaisePropertyChanged(); } }
        }

        private double _multiplyG = 1.0;
        public double MultiplyG
        {
            get => _multiplyG;
            set { if (_multiplyG != value) { _multiplyG = value; RaisePropertyChanged(); } }
        }

        private double _multiplyB = 1.0;
        public double MultiplyB
        {
            get => _multiplyB;
            set { if (_multiplyB != value) { _multiplyB = value; RaisePropertyChanged(); } }
        }

        private double _divideR = 1.0;
        public double DivideR
        {
            get => _divideR;
            set { if (_divideR != value) { _divideR = value; RaisePropertyChanged(); } }
        }

        private double _divideG = 1.0;
        public double DivideG
        {
            get => _divideG;
            set { if (_divideG != value) { _divideG = value; RaisePropertyChanged(); } }
        }

        private double _divideB = 1.0;
        public double DivideB
        {
            get => _divideB;
            set { if (_divideB != value) { _divideB = value; RaisePropertyChanged(); } }
        }

        private int _brightness;
        public int Brightness
        {
            get => _brightness;
            set { if (_brightness != value) { _brightness = value; RaisePropertyChanged(); } }
        }

        private GrayscaleMethod _selectedGrayscaleMethod = GrayscaleMethod.Average;
        public GrayscaleMethod SelectedGrayscaleMethod
        {
            get => _selectedGrayscaleMethod;
            set { if (_selectedGrayscaleMethod != value) { _selectedGrayscaleMethod = value; RaisePropertyChanged(); } }
        }

        public ICommand LoadImageCommand { get; }
        public ICommand AddRGBCommand { get; }
        public ICommand SubtractRGBCommand { get; }
        public ICommand MultiplyRGBCommand { get; }
        public ICommand DivideRGBCommand { get; }
        public ICommand ChangeBrightnessCommand { get; }
        public ICommand ConvertGrayscaleCommand { get; }
        public ICommand ResetImageCommand { get; }

        public DigitalFiltersViewModel(IAsyncImageService imageService)
        {
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));

            LoadImageCommand = new DelegateCommand(async _ => await LoadImage(), _ => true);
            AddRGBCommand = new DelegateCommand(async _ => await AddRGB(), _ => ProcessedImage != null);
            SubtractRGBCommand = new DelegateCommand(async _ => await SubtractRGB(), _ => ProcessedImage != null);
            MultiplyRGBCommand = new DelegateCommand(async _ => await MultiplyRGB(), _ => ProcessedImage != null);
            DivideRGBCommand = new DelegateCommand(async _ => await DivideRGB(), _ => ProcessedImage != null);
            ChangeBrightnessCommand = new DelegateCommand(async _ => await ChangeBrightness(), _ => ProcessedImage != null);
            ConvertGrayscaleCommand = new DelegateCommand(async _ => await ConvertGrayscale(), _ => ProcessedImage != null);
            ResetImageCommand = new DelegateCommand(ResetImage, _ => OriginalImage != null && ProcessedImage != null);
        }

        private async Task LoadImage()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files|*.pgm;*.ppm;*.pbm;*.png;*.jpg;*.jpeg;*.bmp"
            };

            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                try
                {
                    ImageBase image = await _imageService.LoadImageAsync(openFileDialog.FileName);
                    BitmapImage? bitmapImage = await _imageService.ConvertToBitmapImageAsync(image);
                    if (bitmapImage != null)
                    {
                        OriginalImage = bitmapImage;
                        ProcessedImage = OriginalImage;
                        RaiseAllCanExecuteChanged();
                    }
                    else
                    {
                        MessageBox.Show("Failed to convert image to bitmap.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task AddRGB()
        {
            if (ProcessedImage != null)
            {
                if (!IsValidRGB(AddR, AddG, AddB))
                {
                    MessageBox.Show("RGB addition values must be between -255 and 255.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ProcessedImage = await _imageService.AddRGBAsync(ProcessedImage, AddR, AddG, AddB);
            }
        }

        private async Task SubtractRGB()
        {
            if (ProcessedImage != null)
            {
                if (!IsValidRGB(SubtractR, SubtractG, SubtractB))
                {
                    MessageBox.Show("RGB subtraction values must be between -255 and 255.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ProcessedImage = await _imageService.SubtractRGBAsync(ProcessedImage, SubtractR, SubtractG, SubtractB);
            }
        }

        private async Task MultiplyRGB()
        {
            if (ProcessedImage != null)
            {
                if (!IsValidMultiplyDivide(MultiplyR, MultiplyG, MultiplyB))
                {
                    MessageBox.Show("RGB multiplication factors must be positive numbers.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ProcessedImage = await _imageService.MultiplyRGBAsync(ProcessedImage, MultiplyR, MultiplyG, MultiplyB);
            }
        }

        private async Task DivideRGB()
        {
            if (ProcessedImage != null)
            {
                if (!IsValidMultiplyDivide(DivideR, DivideG, DivideB))
                {
                    MessageBox.Show("RGB division factors must be positive numbers.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ProcessedImage = await _imageService.DivideRGBAsync(ProcessedImage, DivideR, DivideG, DivideB);
            }
        }

        private async Task ChangeBrightness()
        {
            if (ProcessedImage != null)
            {
                if (Brightness < -255 || Brightness > 255)
                {
                    MessageBox.Show("Brightness must be between -255 and 255.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ProcessedImage = await _imageService.ChangeBrightnessAsync(ProcessedImage, Brightness);
            }
        }

        private async Task ConvertGrayscale()
        {
            if (ProcessedImage != null)
            {
                ProcessedImage = await _imageService.ConvertToGrayscaleAsync(ProcessedImage, SelectedGrayscaleMethod);
            }
        }

        private void ResetImage(object? parameter)
        {
            if (OriginalImage != null)
            {
                ProcessedImage = OriginalImage;
                RaiseAllCanExecuteChanged();
            }
        }

        private WriteableBitmap BitmapImageToWriteableBitmap(BitmapImage bitmapImage)
        {
            return new WriteableBitmap(bitmapImage);
        }

        private BitmapImage WriteableBitmapToBitmapImage(WriteableBitmap writeableBitmap)
        {
            using MemoryStream ms = new MemoryStream();
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));
            encoder.Save(ms);
            ms.Position = 0;

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = ms;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }

        private void RaiseAllCanExecuteChanged()
        {
            (LoadImageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            (AddRGBCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            (SubtractRGBCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            (MultiplyRGBCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            (DivideRGBCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            (ChangeBrightnessCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            (ConvertGrayscaleCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            (ResetImageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        }

        private bool IsValidRGB(int r, int g, int b)
        {
            return r >= -255 && r <= 255 && g >= -255 && g <= 255 && b >= -255 && b <= 255;
        }

        private bool IsValidMultiplyDivide(double r, double g, double b)
        {
            return r > 0 && g > 0 && b > 0;
        }
    }
}
