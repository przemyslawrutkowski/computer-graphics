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
        Task<BitmapImage?> MedianFilterAsync(BitmapImage image);
        Task<BitmapImage?> SobelFilterAsync(BitmapImage image);
        Task<BitmapImage?> HighPassFilterAsync(BitmapImage image);
        Task<BitmapImage?> GaussianBlurAsync(BitmapImage image);
    }

    public class ImageService : IImageService, IAsyncImageService
    {
        /// <summary>
        /// Zapisuje obraz RenderTargetBitmap jako JPEG.
        /// </summary>
        /// <param name="bitmap">Obraz do zapisania.</param>
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

        /// <summary>
        /// Asynchronicznie ładuje obraz z podanej ścieżki.
        /// </summary>
        /// <param name="filePath">Ścieżka do pliku obrazu.</param>
        /// <returns>Obiekt ImageBase reprezentujący załadowany obraz.</returns>
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

        /// <summary>
        /// Ładuje standardowy obraz (PNG, JPG, BMP) i konwertuje go do Bgra32Image.
        /// </summary>
        /// <param name="filePath">Ścieżka do pliku obrazu.</param>
        /// <returns>Obiekt Bgra32Image.</returns>
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

        /// <summary>
        /// Asynchronicznie zapisuje obraz do podanej ścieżki.
        /// </summary>
        /// <param name="image">Obiekt ImageBase do zapisania.</param>
        /// <param name="filePath">Ścieżka do pliku zapisu.</param>
        /// <param name="isBinary">Czy zapisać w formacie binarnym.</param>
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

        /// <summary>
        /// Zapisuje standardowy obraz (PNG, JPG, BMP).
        /// </summary>
        /// <param name="image">Obiekt ImageBase do zapisania.</param>
        /// <param name="filePath">Ścieżka do pliku zapisu.</param>
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

        /// <summary>
        /// Zwraca odpowiedni BitmapEncoder na podstawie rozszerzenia pliku.
        /// </summary>
        /// <param name="filePath">Ścieżka do pliku.</param>
        /// <returns>Odpowiedni BitmapEncoder.</returns>
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

        /// <summary>
        /// Asynchronicznie konwertuje ImageBase do BitmapImage.
        /// </summary>
        /// <param name="image">Obiekt ImageBase do konwersji.</param>
        /// <returns>BitmapImage lub null, jeśli konwersja się nie powiodła.</returns>
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

        /// <summary>
        /// Konwertuje obraz PGM do BitmapImage.
        /// </summary>
        /// <param name="pgm">Obiekt PgmImage.</param>
        /// <returns>BitmapImage lub null.</returns>
        private BitmapImage? ConvertPgmToBitmapImage(PgmImage pgm)
        {
            if (pgm == null)
                return null;

            WriteableBitmap wb = new WriteableBitmap(pgm.Width, pgm.Height, 96, 96, PixelFormats.Bgra32, null);
            byte[] pixels = new byte[pgm.Width * pgm.Height * 4];

            for (int y = 0; y < pgm.Height; y++)
            {
                for (int x = 0; x < pgm.Width; x++)
                {
                    ushort gray = pgm.Pixels[y, x];
                    byte grayByte = ClampByte(gray);
                    int idx = (y * pgm.Width + x) * 4;
                    pixels[idx + 0] = grayByte; // B
                    pixels[idx + 1] = grayByte; // G
                    pixels[idx + 2] = grayByte; // R
                    pixels[idx + 3] = 255;      // A
                }
            }

            wb.WritePixels(new Int32Rect(0, 0, pgm.Width, pgm.Height), pixels, pgm.Width * 4, 0);
            return WriteableBitmapToBitmapImage(wb);
        }

        /// <summary>
        /// Konwertuje obraz PPM do BitmapImage.
        /// </summary>
        /// <param name="ppm">Obiekt PpmImage.</param>
        /// <returns>BitmapImage lub null.</returns>
        private BitmapImage? ConvertPpmToBitmapImage(PpmImage ppm)
        {
            if (ppm == null)
                return null;

            WriteableBitmap wb = new WriteableBitmap(ppm.Width, ppm.Height, 96, 96, PixelFormats.Bgra32, null);
            byte[] pixels = new byte[ppm.Width * ppm.Height * 4];

            for (int y = 0; y < ppm.Height; y++)
            {
                for (int x = 0; x < ppm.Width; x++)
                {
                    ushort r = ppm.Pixels[y, x, 0];
                    ushort g = ppm.Pixels[y, x, 1];
                    ushort b = ppm.Pixels[y, x, 2];
                    byte rByte = ClampByte(r);
                    byte gByte = ClampByte(g);
                    byte bByte = ClampByte(b);
                    int idx = (y * ppm.Width + x) * 4;
                    pixels[idx + 0] = bByte; // B
                    pixels[idx + 1] = gByte; // G
                    pixels[idx + 2] = rByte; // R
                    pixels[idx + 3] = 255;    // A
                }
            }

            wb.WritePixels(new Int32Rect(0, 0, ppm.Width, ppm.Height), pixels, ppm.Width * 4, 0);
            return WriteableBitmapToBitmapImage(wb);
        }

        /// <summary>
        /// Konwertuje obraz PBM do BitmapImage.
        /// </summary>
        /// <param name="pbm">Obiekt PbmImage.</param>
        /// <returns>BitmapImage lub null.</returns>
        private BitmapImage? ConvertPbmToBitmapImage(PbmImage pbm)
        {
            if (pbm == null)
                return null;

            WriteableBitmap wb = new WriteableBitmap(pbm.Width, pbm.Height, 96, 96, PixelFormats.Bgra32, null);
            byte[] pixels = new byte[pbm.Width * pbm.Height * 4];

            for (int y = 0; y < pbm.Height; y++)
            {
                for (int x = 0; x < pbm.Width; x++)
                {
                    bool isBlack = pbm.Pixels[y, x];
                    byte colorByte = isBlack ? (byte)0 : (byte)255;
                    int idx = (y * pbm.Width + x) * 4;
                    pixels[idx + 0] = colorByte; // B
                    pixels[idx + 1] = colorByte; // G
                    pixels[idx + 2] = colorByte; // R
                    pixels[idx + 3] = 255;       // A
                }
            }

            wb.WritePixels(new Int32Rect(0, 0, pbm.Width, pbm.Height), pixels, pbm.Width * 4, 0);
            return WriteableBitmapToBitmapImage(wb);
        }

        /// <summary>
        /// Asynchronicznie dodaje wartości RGB do obrazu.
        /// </summary>
        /// <param name="image">Obiekt BitmapImage.</param>
        /// <param name="addR">Wartość do dodania do kanału R.</param>
        /// <param name="addG">Wartość do dodania do kanału G.</param>
        /// <param name="addB">Wartość do dodania do kanału B.</param>
        /// <returns>Przetworzony obraz BitmapImage lub null.</returns>
        public async Task<BitmapImage?> AddRGBAsync(BitmapImage image, int addR, int addG, int addB)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            return await ApplyPixelOperationAsync(image, (r, g, b) =>
            {
                return (
                    ClampByte(r + addR),
                    ClampByte(g + addG),
                    ClampByte(b + addB)
                );
            });
        }

        /// <summary>
        /// Asynchronicznie odejmuje wartości RGB od obrazu.
        /// </summary>
        /// <param name="image">Obiekt BitmapImage.</param>
        /// <param name="subR">Wartość do odjęcia od kanału R.</param>
        /// <param name="subG">Wartość do odjęcia od kanału G.</param>
        /// <param name="subB">Wartość do odjęcia od kanału B.</param>
        /// <returns>Przetworzony obraz BitmapImage lub null.</returns>
        public async Task<BitmapImage?> SubtractRGBAsync(BitmapImage image, int subR, int subG, int subB)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            return await ApplyPixelOperationAsync(image, (r, g, b) =>
            {
                return (
                    ClampByte(r - subR),
                    ClampByte(g - subG),
                    ClampByte(b - subB)
                );
            });
        }

        /// <summary>
        /// Asynchronicznie mnoży wartości RGB przez podane współczynniki.
        /// </summary>
        /// <param name="image">Obiekt BitmapImage.</param>
        /// <param name="mulR">Współczynnik mnożenia dla kanału R.</param>
        /// <param name="mulG">Współczynnik mnożenia dla kanału G.</param>
        /// <param name="mulB">Współczynnik mnożenia dla kanału B.</param>
        /// <returns>Przetworzony obraz BitmapImage lub null.</returns>
        public async Task<BitmapImage?> MultiplyRGBAsync(BitmapImage image, double mulR, double mulG, double mulB)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            return await ApplyPixelOperationAsync(image, (r, g, b) =>
            {
                return (
                    ClampByte((int)(r * mulR)),
                    ClampByte((int)(g * mulG)),
                    ClampByte((int)(b * mulB))
                );
            });
        }

        /// <summary>
        /// Asynchronicznie dzieli wartości RGB przez podane współczynniki.
        /// </summary>
        /// <param name="image">Obiekt BitmapImage.</param>
        /// <param name="divR">Współczynnik dzielenia dla kanału R.</param>
        /// <param name="divG">Współczynnik dzielenia dla kanału G.</param>
        /// <param name="divB">Współczynnik dzielenia dla kanału B.</param>
        /// <returns>Przetworzony obraz BitmapImage lub null.</returns>
        public async Task<BitmapImage?> DivideRGBAsync(BitmapImage image, double divR, double divG, double divB)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            return await ApplyPixelOperationAsync(image, (r, g, b) =>
            {
                return (
                    divR != 0 ? ClampByte((int)(r / divR)) : r,
                    divG != 0 ? ClampByte((int)(g / divG)) : g,
                    divB != 0 ? ClampByte((int)(b / divB)) : b
                );
            });
        }

        /// <summary>
        /// Asynchronicznie zmienia jasność obrazu.
        /// </summary>
        /// <param name="image">Obiekt BitmapImage.</param>
        /// <param name="brightness">Wartość zmiany jasności.</param>
        /// <returns>Przetworzony obraz BitmapImage lub null.</returns>
        public async Task<BitmapImage?> ChangeBrightnessAsync(BitmapImage image, int brightness)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            return await ApplyPixelOperationAsync(image, (r, g, b) =>
            {
                return (
                    ClampByte(r + brightness),
                    ClampByte(g + brightness),
                    ClampByte(b + brightness)
                );
            });
        }

        /// <summary>
        /// Asynchronicznie konwertuje obraz do odcieni szarości.
        /// </summary>
        /// <param name="image">Obiekt BitmapImage.</param>
        /// <param name="method">Metoda konwersji do odcieni szarości.</param>
        /// <returns>Przetworzony obraz BitmapImage lub null.</returns>
        public async Task<BitmapImage?> ConvertToGrayscaleAsync(BitmapImage image, GrayscaleMethod method)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

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

        /// <summary>
        /// Asynchronicznie aplikuje filtr medianowy na obrazie.
        /// </summary>
        /// <param name="image">Obiekt BitmapImage.</param>
        /// <returns>Przetworzony obraz BitmapImage lub null.</returns>
        public async Task<BitmapImage?> MedianFilterAsync(BitmapImage image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            return await Task.Run(() =>
            {
                WriteableBitmap wb = ConvertToBgra32(image);
                int width = wb.PixelWidth;
                int height = wb.PixelHeight;
                int stride = width * 4;
                byte[] pixels = new byte[height * stride];
                wb.CopyPixels(pixels, stride, 0);
                byte[] newPixels = new byte[height * stride];

                int filterSize = 3;
                int offset = filterSize / 2;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int[] rValues = new int[filterSize * filterSize];
                        int[] gValues = new int[filterSize * filterSize];
                        int[] bValues = new int[filterSize * filterSize];
                        int count = 0;

                        for (int fy = -offset; fy <= offset; fy++)
                        {
                            for (int fx = -offset; fx <= offset; fx++)
                            {
                                int yy = y + fy;
                                int xx = x + fx;

                                if (yy >= 0 && yy < height && xx >= 0 && xx < width)
                                {
                                    int idx = (yy * width + xx) * 4;
                                    bValues[count] = pixels[idx + 0];
                                    gValues[count] = pixels[idx + 1];
                                    rValues[count] = pixels[idx + 2];
                                    count++;
                                }
                            }
                        }

                        Array.Sort(rValues, 0, count);
                        Array.Sort(gValues, 0, count);
                        Array.Sort(bValues, 0, count);

                        int medianIndex = count / 2;
                        newPixels[(y * width + x) * 4 + 2] = (byte)rValues[medianIndex];
                        newPixels[(y * width + x) * 4 + 1] = (byte)gValues[medianIndex];
                        newPixels[(y * width + x) * 4 + 0] = (byte)bValues[medianIndex];
                        newPixels[(y * width + x) * 4 + 3] = pixels[(y * width + x) * 4 + 3];
                    }
                }

                wb.WritePixels(new Int32Rect(0, 0, width, height), newPixels, stride, 0);
                return WriteableBitmapToBitmapImage(wb);
            });
        }

        /// <summary>
        /// Asynchronicznie aplikuje filtr Sobela na obrazie.
        /// </summary>
        /// <param name="image">Obiekt BitmapImage.</param>
        /// <returns>Przetworzony obraz BitmapImage lub null.</returns>
        public async Task<BitmapImage?> SobelFilterAsync(BitmapImage image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            return await Task.Run(() =>
            {
                WriteableBitmap wb = ConvertToBgra32(image);
                int width = wb.PixelWidth;
                int height = wb.PixelHeight;
                int stride = width * 4;
                byte[] pixels = new byte[height * stride];
                wb.CopyPixels(pixels, stride, 0);
                byte[] newPixels = new byte[height * stride];

                int[,] gx = new int[,]
                {
                    { -1, 0, 1 },
                    { -2, 0, 2 },
                    { -1, 0, 1 }
                };

                int[,] gy = new int[,]
                {
                    { -1, -2, -1 },
                    {  0,  0,  0 },
                    {  1,  2,  1 }
                };

                for (int y = 1; y < height - 1; y++)
                {
                    for (int x = 1; x < width - 1; x++)
                    {
                        int sumX = 0;
                        int sumY = 0;

                        for (int fy = -1; fy <= 1; fy++)
                        {
                            for (int fx = -1; fx <= 1; fx++)
                            {
                                int idx = ((y + fy) * width + (x + fx)) * 4;
                                byte r = pixels[idx + 2];
                                byte g = pixels[idx + 1];
                                byte b = pixels[idx + 0];
                                byte gray = (byte)(0.299 * r + 0.587 * g + 0.114 * b); // Standardowa konwersja
                                sumX += gx[fy + 1, fx + 1] * gray;
                                sumY += gy[fy + 1, fx + 1] * gray;
                            }
                        }

                        int magnitude = (int)Math.Sqrt(sumX * sumX + sumY * sumY);
                        magnitude = magnitude > 255 ? 255 : magnitude;
                        byte magByte = ClampByte(magnitude);
                        newPixels[(y * width + x) * 4 + 2] = magByte;
                        newPixels[(y * width + x) * 4 + 1] = magByte;
                        newPixels[(y * width + x) * 4 + 0] = magByte;
                        newPixels[(y * width + x) * 4 + 3] = pixels[(y * width + x) * 4 + 3];
                    }
                }

                // Obsługa granic obrazu - kopiowanie oryginalnych pikseli
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (y == 0 || y == height - 1 || x == 0 || x == width - 1)
                        {
                            int idx = (y * width + x) * 4;
                            newPixels[idx + 0] = pixels[idx + 0];
                            newPixels[idx + 1] = pixels[idx + 1];
                            newPixels[idx + 2] = pixels[idx + 2];
                            newPixels[idx + 3] = pixels[idx + 3];
                        }
                    }
                }

                wb.WritePixels(new Int32Rect(0, 0, width, height), newPixels, stride, 0);
                return WriteableBitmapToBitmapImage(wb);
            });
        }

        /// <summary>
        /// Asynchronicznie aplikuje filtr High-Pass na obrazie.
        /// </summary>
        /// <param name="image">Obiekt BitmapImage.</param>
        /// <returns>Przetworzony obraz BitmapImage lub null.</returns>
        public async Task<BitmapImage?> HighPassFilterAsync(BitmapImage image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            return await Task.Run(() =>
            {
                WriteableBitmap wb = ConvertToBgra32(image);
                int width = wb.PixelWidth;
                int height = wb.PixelHeight;
                int stride = width * 4;
                byte[] pixels = new byte[height * stride];
                wb.CopyPixels(pixels, stride, 0);
                byte[] newPixels = new byte[height * stride];

                // Predefined High-Pass mask 3x3
                int[,] mask = new int[,]
                {
                    { -1, -1, -1 },
                    { -1,  9, -1 },
                    { -1, -1, -1 }
                };

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int sumR = 0;
                        int sumG = 0;
                        int sumB = 0;

                        for (int fy = -1; fy <= 1; fy++)
                        {
                            for (int fx = -1; fx <= 1; fx++)
                            {
                                int yy = y + fy;
                                int xx = x + fx;

                                if (yy >= 0 && yy < height && xx >= 0 && xx < width)
                                {
                                    int idx = (yy * width + xx) * 4;
                                    int weight = mask[fy + 1, fx + 1];
                                    sumR += pixels[idx + 2] * weight;
                                    sumG += pixels[idx + 1] * weight;
                                    sumB += pixels[idx + 0] * weight;
                                }
                            }
                        }

                        newPixels[(y * width + x) * 4 + 2] = ClampByte(sumR);
                        newPixels[(y * width + x) * 4 + 1] = ClampByte(sumG);
                        newPixels[(y * width + x) * 4 + 0] = ClampByte(sumB);
                        newPixels[(y * width + x) * 4 + 3] = pixels[(y * width + x) * 4 + 3];
                    }
                }

                wb.WritePixels(new Int32Rect(0, 0, width, height), newPixels, stride, 0);
                return WriteableBitmapToBitmapImage(wb);
            });
        }

        /// <summary>
        /// Asynchronicznie aplikuje filtr Gaussa na obrazie przy użyciu separowalnej konwolucji.
        /// </summary>
        /// <param name="image">Obiekt BitmapImage.</param>
        /// <returns>Przetworzony obraz BitmapImage lub null.</returns>
        public async Task<BitmapImage?> GaussianBlurAsync(BitmapImage image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            return await Task.Run(() =>
            {
                WriteableBitmap wb = ConvertToBgra32(image);
                int width = wb.PixelWidth;
                int height = wb.PixelHeight;
                int stride = width * 4;
                byte[] pixels = new byte[height * stride];
                wb.CopyPixels(pixels, stride, 0);
                byte[] newPixels = new byte[height * stride];

                // Definicja 1D Gaussa 5x1
                double[] gaussianKernel = new double[5] { 1, 4, 6, 4, 1 };
                double kernelSum = 16; // Suma jednowymiarowego kernelu

                // Pierwsza konwolucja (pozioma)
                byte[] tempPixels = new byte[height * stride];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        double sumR = 0, sumG = 0, sumB = 0;
                        for (int k = -2; k <= 2; k++)
                        {
                            int xx = x + k;
                            if (xx >= 0 && xx < width)
                            {
                                int idx = (y * width + xx) * 4;
                                sumR += pixels[idx + 2] * gaussianKernel[k + 2];
                                sumG += pixels[idx + 1] * gaussianKernel[k + 2];
                                sumB += pixels[idx + 0] * gaussianKernel[k + 2];
                            }
                        }

                        int idxNew = (y * width + x) * 4;
                        tempPixels[idxNew + 2] = (byte)(sumR / kernelSum);
                        tempPixels[idxNew + 1] = (byte)(sumG / kernelSum);
                        tempPixels[idxNew + 0] = (byte)(sumB / kernelSum);
                        tempPixels[idxNew + 3] = pixels[idxNew + 3];
                    }
                }

                // Druga konwolucja (pionowa)
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        double sumR = 0, sumG = 0, sumB = 0;
                        for (int k = -2; k <= 2; k++)
                        {
                            int yy = y + k;
                            if (yy >= 0 && yy < height)
                            {
                                int idx = (yy * width + x) * 4;
                                sumR += tempPixels[idx + 2] * gaussianKernel[k + 2];
                                sumG += tempPixels[idx + 1] * gaussianKernel[k + 2];
                                sumB += tempPixels[idx + 0] * gaussianKernel[k + 2];
                            }
                        }

                        int idxNew = (y * width + x) * 4;
                        newPixels[idxNew + 2] = (byte)(sumR / kernelSum);
                        newPixels[idxNew + 1] = (byte)(sumG / kernelSum);
                        newPixels[idxNew + 0] = (byte)(sumB / kernelSum);
                        newPixels[idxNew + 3] = tempPixels[idxNew + 3];
                    }
                }

                wb.WritePixels(new Int32Rect(0, 0, width, height), newPixels, stride, 0);
                return WriteableBitmapToBitmapImage(wb);
            });
        }

        /// <summary>
        /// Ogranicza wartość do zakresu 0-255.
        /// </summary>
        /// <param name="value">Wartość do ograniczenia.</param>
        /// <returns>Wartość ograniczona do 0-255.</returns>
        private byte ClampByte(int value) => (byte)Math.Max(0, Math.Min(255, value));

        /// <summary>
        /// Konwertuje WriteableBitmap do BitmapImage.
        /// </summary>
        /// <param name="writeableBitmap">Obiekt WriteableBitmap do konwersji.</param>
        /// <returns>BitmapImage.</returns>
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

        /// <summary>
        /// Asynchronicznie aplikuje operację na każdym pikselu obrazu.
        /// </summary>
        /// <param name="image">Obiekt BitmapImage.</param>
        /// <param name="operation">Funkcja operacji na pikselu.</param>
        /// <returns>Przetworzony obraz BitmapImage lub null.</returns>
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

        /// <summary>
        /// Konwertuje BitmapImage do WriteableBitmap w formacie Bgra32.
        /// </summary>
        /// <param name="bitmapImage">Obiekt BitmapImage do konwersji.</param>
        /// <returns>WriteableBitmap w formacie Bgra32.</returns>
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

        /// <summary>
        /// Konwertuje ImageBase do PgmImage.
        /// </summary>
        /// <param name="image">Obiekt ImageBase do konwersji.</param>
        /// <returns>Obiekt PgmImage.</returns>
        public PgmImage ConvertToPgmImage(ImageBase image)
        {
            PgmImage pgm = new PgmImage
            {
                MaxVal = 255
            };

            if (image is PpmImage ppm)
            {
                pgm.Width = ppm.Width;
                pgm.Height = ppm.Height;
                pgm.Pixels = new ushort[ppm.Height, ppm.Width];

                for (int i = 0; i < ppm.Height; i++)
                {
                    for (int j = 0; j < ppm.Width; j++)
                    {
                        pgm.Pixels[i, j] = (ushort)(0.299 * ppm.Pixels[i, j, 0] + 0.587 * ppm.Pixels[i, j, 1] + 0.114 * ppm.Pixels[i, j, 2]);
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
                        pgm.Pixels[i, j] = pbm.Pixels[i, j] ? (ushort)0 : (ushort)255;
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Unsupported image type for conversion to PGM.");
            }

            return pgm;
        }

        /// <summary>
        /// Konwertuje ImageBase do PpmImage.
        /// </summary>
        /// <param name="image">Obiekt ImageBase do konwersji.</param>
        /// <returns>Obiekt PpmImage.</returns>
        public PpmImage ConvertToPpmImage(ImageBase image)
        {
            PpmImage ppm = new PpmImage
            {
                MaxVal = 255
            };

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

        /// <summary>
        /// Konwertuje ImageBase do PbmImage.
        /// </summary>
        /// <param name="image">Obiekt ImageBase do konwersji.</param>
        /// <returns>Obiekt PbmImage.</returns>
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
                        double gray = 0.299 * ppm.Pixels[i, j, 0] + 0.587 * ppm.Pixels[i, j, 1] + 0.114 * ppm.Pixels[i, j, 2];
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
