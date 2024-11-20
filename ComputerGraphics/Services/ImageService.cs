using ComputerGraphics.Models;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ComputerGraphics.Services
{
    public interface IImageService
    {
        void SaveImage(RenderTargetBitmap bitmap);
    }

    public interface IAsyncImageService
    {
        Task<ImageBase> LoadImageAsync(string filePath);
        Task SaveImageAsync(ImageBase image, string filePath, bool isBinary);
        Task<BitmapImage?> ConvertToBitmapImageAsync(ImageBase image);

        Task<BitmapImage?> AddRGBAsync(BitmapImage image, int addR, int addG, int addB);
        Task<BitmapImage?> SubtractRGBAsync(BitmapImage image, int subR, int subG, int subB);
        Task<BitmapImage?> MultiplyRGBAsync(BitmapImage image, double mulR, double mulG, double mulB);
        Task<BitmapImage?> DivideRGBAsync(BitmapImage image, double divR, double divG, double divB);
        Task<BitmapImage?> ChangeBrightnessAsync(BitmapImage image, int brightness);
        Task<BitmapImage?> ConvertToGrayscaleAsync(BitmapImage image, GrayscaleMethod method);
    }

    public class ImageService : IImageService, IAsyncImageService
    {
        public void SaveImage(RenderTargetBitmap bitmap)
        {
            string path = @"C:\Users\rutko\source\repos\ComputerGraphicsSolution\image.jpeg";
            try
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder
                {
                    QualityLevel = 100
                };
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using FileStream stream = File.Create(path);
                encoder.Save(stream);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save image to {path}.", ex);
            }
        }

        public async Task<ImageBase> LoadImageAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                string extension = Path.GetExtension(filePath).ToLower();
                switch (extension)
                {
                    case ".pgm":
                        var pgm = new PgmImage();
                        pgm.Load(filePath);
                        return (ImageBase)pgm;
                    case ".ppm":
                        var ppm = new PpmImage();
                        ppm.Load(filePath);
                        return (ImageBase)ppm;
                    case ".pbm":
                        var pbm = new PbmImage();
                        pbm.Load(filePath);
                        return (ImageBase)pbm;
                    case ".png":
                    case ".jpg":
                    case ".jpeg":
                    case ".bmp":
                        return LoadStandardImage(filePath);
                    default:
                        throw new NotSupportedException("Unsupported file format.");
                }
            });
        }

        private ImageBase LoadStandardImage(string filePath)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(filePath, UriKind.Absolute);
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            WriteableBitmap writeableBitmap = ConvertToBgra32(bitmapImage);
            return new Bgra32Image(writeableBitmap);
        }

        public async Task SaveImageAsync(ImageBase image, string filePath, bool isBinary)
        {
            await Task.Run(() =>
            {
                string extension = Path.GetExtension(filePath).ToLower();

                ImageBase imageToSave = image;
                switch (extension)
                {
                    case ".pgm":
                        if (!(image is PgmImage))
                        {
                            imageToSave = ConvertToPgmImage(image);
                        }
                        break;
                    case ".ppm":
                        if (!(image is PpmImage))
                        {
                            imageToSave = ConvertToPpmImage(image);
                        }
                        break;
                    case ".pbm":
                        if (!(image is PbmImage))
                        {
                            imageToSave = ConvertToPbmImage(image);
                        }
                        break;
                    case ".png":
                    case ".jpg":
                    case ".jpeg":
                    case ".bmp":
                        SaveStandardImage(image, filePath);
                        return;
                    default:
                        throw new NotSupportedException("Unsupported file format.");
                }

                switch (extension)
                {
                    case ".pgm":
                        if (imageToSave is PgmImage pgm)
                            pgm.Save(filePath, isBinary);
                        else
                            throw new InvalidOperationException("Image is not in PGM format.");
                        break;
                    case ".ppm":
                        if (imageToSave is PpmImage ppm)
                            ppm.Save(filePath, isBinary);
                        else
                            throw new InvalidOperationException("Image is not in PPM format.");
                        break;
                    case ".pbm":
                        if (imageToSave is PbmImage pbm)
                            pbm.Save(filePath, isBinary);
                        else
                            throw new InvalidOperationException("Image is not in PBM format.");
                        break;
                }
            });
        }

        private void SaveStandardImage(ImageBase image, string filePath)
        {
            if (image is Bgra32Image bgra32Image)
            {
                BitmapEncoder encoder = GetBitmapEncoder(filePath);
                encoder.Frames.Add(BitmapFrame.Create(bgra32Image.WriteableBitmap));
                using FileStream fs = new FileStream(filePath, FileMode.Create);
                encoder.Save(fs);
            }
            else
            {
                throw new InvalidOperationException("Image is not in a supported standard format.");
            }
        }

        private BitmapEncoder GetBitmapEncoder(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return extension switch
            {
                ".jpg" or ".jpeg" => new JpegBitmapEncoder { QualityLevel = 100 },
                ".bmp" => new BmpBitmapEncoder(),
                ".png" => new PngBitmapEncoder(),
                _ => throw new NotSupportedException("Unsupported encoder format."),
            };
        }

        public async Task<BitmapImage?> ConvertToBitmapImageAsync(ImageBase image)
        {
            return await Task.Run(() =>
            {
                if (image is Bgra32Image bgra32Image)
                {
                    return bgra32Image.WriteableBitmapToBitmapImage();
                }
                else if (image is PgmImage pgm)
                {
                    return ConvertPgmToBitmapImage(pgm);
                }
                else if (image is PpmImage ppm)
                {
                    return ConvertPpmToBitmapImage(ppm);
                }
                else if (image is PbmImage pbm)
                {
                    return ConvertPbmToBitmapImage(pbm);
                }
                return null;
            });
        }

        public async Task<BitmapImage?> AddRGBAsync(BitmapImage image, int addR, int addG, int addB)
        {
            return await ApplyPixelOperationAsync(image, (r, g, b) =>
            {
                return (
                    ClampByte(r + addR),
                    ClampByte(g + addG),
                    ClampByte(b + addB)
                );
            });
        }

        public async Task<BitmapImage?> SubtractRGBAsync(BitmapImage image, int subR, int subG, int subB)
        {
            return await ApplyPixelOperationAsync(image, (r, g, b) =>
            {
                return (
                    ClampByte(r - subR),
                    ClampByte(g - subG),
                    ClampByte(b - subB)
                );
            });
        }

        public async Task<BitmapImage?> MultiplyRGBAsync(BitmapImage image, double mulR, double mulG, double mulB)
        {
            return await ApplyPixelOperationAsync(image, (r, g, b) =>
            {
                return (
                    ClampByte((int)(r * mulR)),
                    ClampByte((int)(g * mulG)),
                    ClampByte((int)(b * mulB))
                );
            });
        }

        public async Task<BitmapImage?> DivideRGBAsync(BitmapImage image, double divR, double divG, double divB)
        {
            return await ApplyPixelOperationAsync(image, (r, g, b) =>
            {
                return (
                    divR != 0 ? ClampByte((int)(r / divR)) : r,
                    divG != 0 ? ClampByte((int)(g / divG)) : g,
                    divB != 0 ? ClampByte((int)(b / divB)) : b
                );
            });
        }

        public async Task<BitmapImage?> ChangeBrightnessAsync(BitmapImage image, int brightness)
        {
            return await ApplyPixelOperationAsync(image, (r, g, b) =>
            {
                return (
                    ClampByte(r + brightness),
                    ClampByte(g + brightness),
                    ClampByte(b + brightness)
                );
            });
        }

        public async Task<BitmapImage?> ConvertToGrayscaleAsync(BitmapImage image, GrayscaleMethod method)
        {
            return await Task.Run(() =>
            {
                WriteableBitmap wb = ConvertToBgra32(image);
                int width = wb.PixelWidth;
                int height = wb.PixelHeight;
                int stride = width * 4;
                byte[] pixels = new byte[height * stride];
                wb.CopyPixels(pixels, stride, 0);

                for (int i = 0; i < pixels.Length; i += 4)
                {
                    byte r = pixels[i + 2];
                    byte g = pixels[i + 1];
                    byte b = pixels[i + 0];
                    byte gray = method switch
                    {
                        GrayscaleMethod.Average => (byte)((r + g + b) / 3),
                        GrayscaleMethod.RedChannel => r,
                        GrayscaleMethod.GreenChannel => g,
                        GrayscaleMethod.BlueChannel => b,
                        GrayscaleMethod.AverageRG => (byte)((r + g) / 2),
                        GrayscaleMethod.MaxValue => (byte)Math.Max(r, Math.Max(g, b)),
                        GrayscaleMethod.MinValue => (byte)Math.Min(r, Math.Min(g, b)),
                        _ => (byte)((r + g + b) / 3),
                    };

                    pixels[i + 2] = gray;
                    pixels[i + 1] = gray;
                    pixels[i + 0] = gray;
                }

                wb.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
                return WriteableBitmapToBitmapImage(wb);
            });
        }

        private async Task<BitmapImage?> ApplyPixelOperationAsync(BitmapImage image, Func<byte, byte, byte, (byte R, byte G, byte B)> operation)
        {
            return await Task.Run(() =>
            {
                WriteableBitmap wb = ConvertToBgra32(image);
                int width = wb.PixelWidth;
                int height = wb.PixelHeight;
                int stride = width * 4;
                byte[] pixels = new byte[height * stride];
                wb.CopyPixels(pixels, stride, 0);

                for (int i = 0; i < pixels.Length; i += 4)
                {
                    var (newR, newG, newB) = operation(pixels[i + 2], pixels[i + 1], pixels[i + 0]);
                    pixels[i + 2] = newR;
                    pixels[i + 1] = newG;
                    pixels[i + 0] = newB;
                }

                wb.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
                return WriteableBitmapToBitmapImage(wb);
            });
        }

        private WriteableBitmap ConvertToBgra32(BitmapImage bitmapImage)
        {
            if (bitmapImage.Format == PixelFormats.Bgra32)
            {
                return new WriteableBitmap(bitmapImage);
            }

            FormatConvertedBitmap formattedBitmap = new FormatConvertedBitmap();
            formattedBitmap.BeginInit();
            formattedBitmap.Source = bitmapImage;
            formattedBitmap.DestinationFormat = PixelFormats.Bgra32;
            formattedBitmap.EndInit();
            formattedBitmap.Freeze();

            return new WriteableBitmap(formattedBitmap);
        }

        private BitmapImage ConvertPgmToBitmapImage(PgmImage pgm)
        {
            var bitmap = new WriteableBitmap(pgm.Width, pgm.Height, 96, 96, PixelFormats.Bgra32, null);
            byte[] pixels = new byte[pgm.Width * pgm.Height * 4];

            double scaleFactor = pgm.MaxVal > 0 ? 255.0 / pgm.MaxVal : 1.0;

            for (int row = 0; row < pgm.Height; row++)
            {
                for (int col = 0; col < pgm.Width; col++)
                {
                    int index = (row * pgm.Width + col) * 4;
                    byte gray = (byte)(pgm.Pixels[row, col] * scaleFactor);
                    pixels[index] = gray;
                    pixels[index + 1] = gray;
                    pixels[index + 2] = gray;
                    pixels[index + 3] = 255;
                }
            }

            bitmap.WritePixels(new Int32Rect(0, 0, pgm.Width, pgm.Height), pixels, pgm.Width * 4, 0);
            return WriteableBitmapToBitmapImage(bitmap);
        }

        private BitmapImage ConvertPpmToBitmapImage(PpmImage ppm)
        {
            var bitmap = new WriteableBitmap(ppm.Width, ppm.Height, 96, 96, PixelFormats.Bgra32, null);
            byte[] pixels = new byte[ppm.Width * ppm.Height * 4];

            double scaleFactor = ppm.MaxVal > 0 ? 255.0 / ppm.MaxVal : 1.0;

            for (int row = 0; row < ppm.Height; row++)
            {
                for (int col = 0; col < ppm.Width; col++)
                {
                    int index = (row * ppm.Width + col) * 4;
                    byte r = (byte)(ppm.Pixels[row, col, 0] * scaleFactor);
                    byte g = (byte)(ppm.Pixels[row, col, 1] * scaleFactor);
                    byte b = (byte)(ppm.Pixels[row, col, 2] * scaleFactor);
                    pixels[index] = b;
                    pixels[index + 1] = g;
                    pixels[index + 2] = r;
                    pixels[index + 3] = 255;
                }
            }

            bitmap.WritePixels(new Int32Rect(0, 0, ppm.Width, ppm.Height), pixels, ppm.Width * 4, 0);
            return WriteableBitmapToBitmapImage(bitmap);
        }

        private BitmapImage ConvertPbmToBitmapImage(PbmImage pbm)
        {
            var bitmap = new WriteableBitmap(pbm.Width, pbm.Height, 96, 96, PixelFormats.Bgra32, null);
            byte[] pixels = new byte[pbm.Width * pbm.Height * 4];

            for (int row = 0; row < pbm.Height; row++)
            {
                for (int col = 0; col < pbm.Width; col++)
                {
                    int index = (row * pbm.Width + col) * 4;
                    if (pbm.Pixels[row, col])
                    {
                        pixels[index] = 0;
                        pixels[index + 1] = 0;
                        pixels[index + 2] = 0;
                    }
                    else
                    {
                        pixels[index] = 255;
                        pixels[index + 1] = 255;
                        pixels[index + 2] = 255;
                    }
                    pixels[index + 3] = 255;
                }
            }

            bitmap.WritePixels(new Int32Rect(0, 0, pbm.Width, pbm.Height), pixels, pbm.Width * 4, 0);
            return WriteableBitmapToBitmapImage(bitmap);
        }

        private byte ClampByte(int value) => (byte)Math.Max(0, Math.Min(255, value));

        public PgmImage ConvertToPgmImage(ImageBase image)
        {
            PgmImage pgm = new PgmImage();

            pgm.MaxVal = 255;

            if (image is PpmImage ppm)
            {
                pgm.Width = ppm.Width;
                pgm.Height = ppm.Height;
                pgm.Pixels = new ushort[ppm.Height, ppm.Width];

                for (int i = 0; i < ppm.Height; i++)
                {
                    for (int j = 0; j < ppm.Width; j++)
                    {
                        pgm.Pixels[i, j] = (ushort)(0.3 * ppm.Pixels[i, j, 0] + 0.59 * ppm.Pixels[i, j, 1] + 0.11 * ppm.Pixels[i, j, 2]);
                    }
                }
            }
            else if (image is PbmImage pbm)
            {
                pgm.Width = pbm.Width;
                pgm.Height = pbm.Height;
                pgm.Pixels = new ushort[pbm.Height, pbm.Width];

                for (int i = 0; i < pbm.Height; i++)
                {
                    for (int j = 0; j < pbm.Width; j++)
                    {
                        pgm.Pixels[i, j] = pbm.Pixels[i, j] ? (ushort)255 : (ushort)0;
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Unsupported image type for conversion to PGM.");
            }

            return pgm;
        }

        public PpmImage ConvertToPpmImage(ImageBase image)
        {
            PpmImage ppm = new PpmImage();

            ppm.MaxVal = 255;

            if (image is PgmImage pgm)
            {
                ppm.Width = pgm.Width;
                ppm.Height = pgm.Height;
                ppm.Pixels = new ushort[pgm.Height, pgm.Width, 3];

                for (int i = 0; i < pgm.Height; i++)
                {
                    for (int j = 0; j < pgm.Width; j++)
                    {
                        ushort gray = pgm.Pixels[i, j];
                        ppm.Pixels[i, j, 0] = gray;
                        ppm.Pixels[i, j, 1] = gray;
                        ppm.Pixels[i, j, 2] = gray;
                    }
                }
            }
            else if (image is PbmImage pbm)
            {
                ppm.Width = pbm.Width;
                ppm.Height = pbm.Height;
                ppm.Pixels = new ushort[pbm.Height, pbm.Width, 3];

                for (int i = 0; i < pbm.Height; i++)
                {
                    for (int j = 0; j < pbm.Width; j++)
                    {
                        ushort color = pbm.Pixels[i, j] ? (ushort)0 : (ushort)255;
                        ppm.Pixels[i, j, 0] = color;
                        ppm.Pixels[i, j, 1] = color;
                        ppm.Pixels[i, j, 2] = color;
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Unsupported image type for conversion to PPM.");
            }

            return ppm;
        }

        public PbmImage ConvertToPbmImage(ImageBase image)
        {
            PbmImage pbm = new PbmImage();

            if (image is PgmImage pgm)
            {
                pbm.Width = pgm.Width;
                pbm.Height = pgm.Height;
                pbm.Pixels = new bool[pgm.Height, pgm.Width];

                for (int i = 0; i < pgm.Height; i++)
                {
                    for (int j = 0; j < pgm.Width; j++)
                    {
                        pbm.Pixels[i, j] = pgm.Pixels[i, j] < (pgm.MaxVal / 2.0);
                    }
                }
            }
            else if (image is PpmImage ppm)
            {
                pbm.Width = ppm.Width;
                pbm.Height = ppm.Height;
                pbm.Pixels = new bool[ppm.Height, ppm.Width];

                for (int i = 0; i < ppm.Height; i++)
                {
                    for (int j = 0; j < ppm.Width; j++)
                    {
                        double gray = 0.3 * ppm.Pixels[i, j, 0] + 0.59 * ppm.Pixels[i, j, 1] + 0.11 * ppm.Pixels[i, j, 2];
                        pbm.Pixels[i, j] = gray < (ppm.MaxVal / 2.0);
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Unsupported image type for conversion to PBM.");
            }

            return pbm;
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
    }

    public class Bgra32Image : ImageBase
    {
        public WriteableBitmap WriteableBitmap { get; }

        public Bgra32Image(WriteableBitmap writeableBitmap)
        {
            WriteableBitmap = writeableBitmap;
        }

        public override void Load(string filePath)
        {
            throw new NotImplementedException();
        }

        public override void Save(string filePath, bool isBinary)
        {
            throw new NotImplementedException();
        }

        public BitmapImage WriteableBitmapToBitmapImage()
        {
            using MemoryStream ms = new MemoryStream();
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(WriteableBitmap));
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
    }
}
