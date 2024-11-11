using System.IO;
using System.Text;

namespace ComputerGraphics.Models
{
    public class PbmImage : ImageBase
    {
        public bool[,] Pixels { get; set; } = null!;
        public int Width { get; set; }
        public int Height { get; set; }

        public override void Load(string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            string magicNumber = ReadNextToken(fs);
            bool isBinary = magicNumber == "P4";

            string widthToken = ReadNextToken(fs);
            string heightToken = ReadNextToken(fs);

            Width = int.Parse(widthToken);
            Height = int.Parse(heightToken);

            Pixels = new bool[Height, Width];

            if (isBinary)
            {
                using var br = new BinaryReader(fs, Encoding.ASCII, leaveOpen: true);

                int rowBytes = (Width + 7) / 8;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < rowBytes; j++)
                    {
                        if (fs.Position >= fs.Length)
                            throw new EndOfStreamException($"Unexpected end of file at row {i}.");

                        byte b = br.ReadByte();
                        for (int bit = 0; bit < 8; bit++)
                        {
                            int col = j * 8 + bit;
                            if (col < Width)
                                Pixels[i, col] = ((b >> (7 - bit)) & 1) == 0;
                        }
                    }
                }
            }
            else
            {
                using var sr = new StreamReader(fs, Encoding.ASCII, leaveOpen: true);
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        string pixelToken = ReadNextToken(sr);
                        if (pixelToken == null)
                            throw new FormatException("Not enough pixel data.");
                        Pixels[i, j] = pixelToken == "0";
                    }
                }
            }
        }

        public override void Save(string filePath, bool isBinary)
        {
            if (isBinary)
            {
                using var bw = new BinaryWriter(File.Open(filePath, FileMode.Create));
                bw.Write(Encoding.ASCII.GetBytes($"P4\n{Width} {Height}\n"));

                int rowBytes = (Width + 7) / 8;
                for (int i = 0; i < Height; i++)
                {
                    byte b = 0;
                    for (int j = 0; j < Width; j++)
                    {
                        if (!Pixels[i, j])
                            b |= (byte)(1 << (7 - (j % 8)));

                        if ((j + 1) % 8 == 0 || j == Width - 1)
                        {
                            bw.Write(b);
                            b = 0;
                        }
                    }
                }
            }
            else
            {
                using var writer = new StreamWriter(filePath);
                writer.WriteLine("P1");
                writer.WriteLine($"{Width} {Height}");

                for (int i = 0; i < Height; i++)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int j = 0; j < Width; j++)
                    {
                        sb.Append($"{(Pixels[i, j] ? "0" : "1")} ");
                    }
                    writer.WriteLine(sb.ToString().Trim());
                }
            }
        }

        private string ReadNextToken(FileStream fs)
        {
            StringBuilder sb = new StringBuilder();
            int ch;

            while (true)
            {
                ch = fs.ReadByte();
                if (ch == -1)
                    throw new EndOfStreamException("Unexpected end of file.");

                char c = (char)ch;

                if (char.IsWhiteSpace(c))
                    continue;

                if (c == '#')
                {
                    while (ch != -1 && c != '\n')
                    {
                        ch = fs.ReadByte();
                        if (ch == -1)
                            throw new EndOfStreamException("Unexpected end of file.");
                        c = (char)ch;
                    }
                    continue;
                }

                sb.Append(c);
                break;
            }

            while (true)
            {
                ch = fs.ReadByte();
                if (ch == -1)
                    break;

                char c = (char)ch;

                if (char.IsWhiteSpace(c) || c == '#')
                {
                    if (c == '#')
                    {
                        while (ch != -1 && c != '\n')
                        {
                            ch = fs.ReadByte();
                            if (ch == -1)
                                break;
                            c = (char)ch;
                        }
                    }
                    break;
                }

                sb.Append(c);
            }

            return sb.ToString();
        }

        private string ReadNextToken(StreamReader reader)
        {
            StringBuilder sb = new StringBuilder();
            int ch;

            while (true)
            {
                ch = reader.Read();
                if (ch == -1)
                    throw new EndOfStreamException("Unexpected end of file.");

                char c = (char)ch;

                if (char.IsWhiteSpace(c))
                    continue;

                if (c == '#')
                {
                    reader.ReadLine();
                    continue;
                }

                sb.Append(c);
                break;
            }

            while (true)
            {
                ch = reader.Peek();
                if (ch == -1)
                    break;

                char c = (char)ch;

                if (char.IsWhiteSpace(c) || c == '#')
                    break;

                sb.Append((char)reader.Read());
            }

            return sb.ToString();
        }
    }
}
