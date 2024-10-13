using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ComputerGraphics.Services
{
    public interface IImagesService
    {
        void SaveImage(Canvas canvas);

    }
    public class ImagesService : IImagesService
    {
        public void SaveImage(Canvas canvas)
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(canvas);
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.QualityLevel = 100;
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (FileStream stream = File.Create(@"C:\Users\rutko\source\repos\ComputerGraphicsSolution\image.jpeg"))
            {
                encoder.Save(stream);
            }
        }
    }
}
