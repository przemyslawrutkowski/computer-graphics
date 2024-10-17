using System.IO;
using System.Windows.Media.Imaging;

namespace ComputerGraphics.Services
{
    public interface IImageService
    {
        void SaveImage(RenderTargetBitmap bitmap);

    }
    public class ImageService : IImageService
    {
        public void SaveImage(RenderTargetBitmap bitmap)
        {

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
