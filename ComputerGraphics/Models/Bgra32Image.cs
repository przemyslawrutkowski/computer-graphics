using System.IO;
using System.Windows.Media.Imaging;

namespace ComputerGraphics.Models
{
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
