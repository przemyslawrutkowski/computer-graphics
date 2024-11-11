using ComputerGraphics.Models;
using ComputerGraphics.MVVM;
using ComputerGraphics.Services;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ComputerGraphics.ViewModels
{
    public class FilesViewModel : ViewModelBase
    {
        private ImageBase? _currentImage;
        public ImageBase? CurrentImage
        {
            get => _currentImage;
            set
            {
                _currentImage = value;
                RaisePropertyChanged();
                SaveImageCommand.RaiseCanExecuteChanged();
            }
        }

        private BitmapImage? _imageSource;
        public BitmapImage? ImageSource
        {
            get => _imageSource;
            private set
            {
                _imageSource = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand LoadImageCommand { get; private set; }
        public DelegateCommand SaveImageCommand { get; private set; }

        private readonly IAsyncImageService _imageService;
        private readonly CommandQueue _commandQueue;

        public FilesViewModel(IAsyncImageService imageService, CommandQueue commandQueue)
        {
            _imageService = imageService;
            _commandQueue = commandQueue;

            LoadImageCommand = new DelegateCommand(param => EnqueueLoadImage(), _ => true);
            SaveImageCommand = new DelegateCommand(param => EnqueueSaveImage(), _ => CurrentImage != null);
        }

        private void EnqueueLoadImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PBM Files (*.pbm)|*.pbm|PGM Files (*.pgm)|*.pgm|PPM Files (*.ppm)|*.ppm"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                _commandQueue.Enqueue(async () =>
                {
                    try
                    {
                        ImageBase image = await _imageService.LoadImageAsync(filePath);
                        CurrentImage = image;

                        BitmapImage? bitmapImage = await _imageService.ConvertToBitmapImageAsync(image);
                        ImageSource = bitmapImage;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred while loading the file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
        }

        private void EnqueueSaveImage()
        {
            if (CurrentImage == null)
                return;

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = CurrentImage switch
                {
                    PgmImage => "PGM Files (*.pgm)|*.pgm",
                    PpmImage => "PPM Files (*.ppm)|*.ppm",
                    PbmImage => "PBM Files (*.pbm)|*.pbm",
                    _ => "All Supported Formats (*.pgm;*.ppm;*.pbm)|*.pgm;*.ppm;*.pbm"
                }
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                MessageBoxResult result = MessageBox.Show("Do you want to save in binary format?", "Save Format", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Cancel)
                    return;

                bool isBinary = result == MessageBoxResult.Yes;

                _commandQueue.Enqueue(async () =>
                {
                    try
                    {
                        await _imageService.SaveImageAsync(CurrentImage, filePath, isBinary);
                        MessageBox.Show("Image saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (NotSupportedException ex)
                    {
                        MessageBox.Show($"Unsupported file format: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (InvalidOperationException ex)
                    {
                        MessageBox.Show($"Invalid operation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred while saving the file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
        }
    }
}
