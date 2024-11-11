using ComputerGraphics.Models;
using System.IO;
using System.Windows;
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
                using (FileStream stream = File.Create(path))
                {
                    encoder.Save(stream);
                }
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
                    default:
                        throw new NotSupportedException("Unsupported file format.");
                }
            });
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

        public async Task<BitmapImage?> ConvertToBitmapImageAsync(ImageBase image)
        {
            return await Task.Run(() =>
            {
                if (image is PgmImage pgm)
                    return ConvertPgmToBitmapImage(pgm);
                if (image is PpmImage ppm)
                    return ConvertPpmToBitmapImage(ppm);
                if (image is PbmImage pbm)
                    return ConvertPbmToBitmapImage(pbm);
                return null;
            });
        }

        private BitmapImage ConvertPgmToBitmapImage(PgmImage pgm)
        {
            var bitmap = new WriteableBitmap(pgm.Width, pgm.Height, 96, 96, System.Windows.Media.PixelFormats.Gray8, null);
            byte[] pixels = new byte[pgm.Width * pgm.Height];

            double scaleFactor = pgm.MaxVal > 0 ? 255.0 / pgm.MaxVal : 1.0;

            for (int row = 0; row < pgm.Height; row++)
            {
                for (int col = 0; col < pgm.Width; col++)
                {
                    pixels[row * pgm.Width + col] = (byte)(pgm.Pixels[row, col] * scaleFactor);
                }
            }
            bitmap.WritePixels(new Int32Rect(0, 0, pgm.Width, pgm.Height), pixels, pgm.Width, 0);

            using (var memory = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(memory);
                memory.Position = 0;
                BitmapImage bmpImage = new BitmapImage();
                bmpImage.BeginInit();
                bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                bmpImage.StreamSource = memory;
                bmpImage.EndInit();
                bmpImage.Freeze();
                return bmpImage;
            }
        }

        private BitmapImage ConvertPpmToBitmapImage(PpmImage ppm)
        {
            var bitmap = new WriteableBitmap(ppm.Width, ppm.Height, 96, 96, System.Windows.Media.PixelFormats.Rgb24, null);
            byte[] pixels = new byte[ppm.Width * ppm.Height * 3];

            double scaleFactor = ppm.MaxVal > 0 ? 255.0 / ppm.MaxVal : 1.0;

            for (int row = 0; row < ppm.Height; row++)
            {
                for (int col = 0; col < ppm.Width; col++)
                {
                    int index = (row * ppm.Width + col) * 3;
                    pixels[index] = (byte)(ppm.Pixels[row, col, 0] * scaleFactor);
                    pixels[index + 1] = (byte)(ppm.Pixels[row, col, 1] * scaleFactor);
                    pixels[index + 2] = (byte)(ppm.Pixels[row, col, 2] * scaleFactor);
                }
            }
            bitmap.WritePixels(new Int32Rect(0, 0, ppm.Width, ppm.Height), pixels, ppm.Width * 3, 0);

            using (var memory = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(memory);
                memory.Position = 0;
                BitmapImage bmpImage = new BitmapImage();
                bmpImage.BeginInit();
                bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                bmpImage.StreamSource = memory;
                bmpImage.EndInit();
                bmpImage.Freeze();
                return bmpImage;
            }
        }

        private BitmapImage ConvertPbmToBitmapImage(PbmImage pbm)
        {
            var bitmap = new WriteableBitmap(pbm.Width, pbm.Height, 96, 96, System.Windows.Media.PixelFormats.BlackWhite, null);
            byte[] pixels = new byte[(pbm.Width + 7) / 8 * pbm.Height];

            for (int row = 0; row < pbm.Height; row++)
            {
                for (int col = 0; col < pbm.Width; col++)
                {
                    if (pbm.Pixels[row, col])
                    {
                        int byteIndex = row * ((pbm.Width + 7) / 8) + (col / 8);
                        int bitIndex = 7 - (col % 8);
                        pixels[byteIndex] |= (byte)(1 << bitIndex);
                    }
                }
            }
            bitmap.WritePixels(new Int32Rect(0, 0, pbm.Width, pbm.Height), pixels, (pbm.Width + 7) / 8, 0);

            using (var memory = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(memory);
                memory.Position = 0;
                BitmapImage bmpImage = new BitmapImage();
                bmpImage.BeginInit();
                bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                bmpImage.StreamSource = memory;
                bmpImage.EndInit();
                bmpImage.Freeze();
                return bmpImage;
            }
        }

        private PgmImage ConvertToPgmImage(ImageBase image)
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

        private PpmImage ConvertToPpmImage(ImageBase image)
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

        private PbmImage ConvertToPbmImage(ImageBase image)
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
    }
}
