namespace ComputerGraphics.Models
{
    public abstract class ImageBase
    {
        public abstract void Load(string filePath);
        public abstract void Save(string filePath, bool isBinary);
    }
}
