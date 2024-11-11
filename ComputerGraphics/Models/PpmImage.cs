using System.IO;
using System.Text;

namespace ComputerGraphics.Models
{
    public class PpmImage : ImageBase
    {
        public ushort[,,] Pixels { get; set; } = null!;
        public int Width { get; set; }
        public int Height { get; set; }
        public int MaxVal { get; set; }

        public override void Load(string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            string magicNumber = ReadNextToken(fs);
            bool isBinary = magicNumber == "P6";

            string widthToken = ReadNextToken(fs);
            string heightToken = ReadNextToken(fs);
            string maxValToken = ReadNextToken(fs);

            Width = int.Parse(widthToken);
            Height = int.Parse(heightToken);
            MaxVal = int.Parse(maxValToken);

            Pixels = new ushort[Height, Width, 3];

            if (isBinary)
            {
                using var br = new BinaryReader(fs, Encoding.ASCII, leaveOpen: true);

                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            if (fs.Position >= fs.Length)
                                throw new EndOfStreamException($"Unexpected end of file at pixel ({i}, {j}, {k}).");

                            Pixels[i, j, k] = MaxVal < 256 ? br.ReadByte() : ReadBigEndianUShort(br);
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
                        for (int k = 0; k < 3; k++)
                        {
                            string pixelToken = ReadNextToken(sr);
                            if (pixelToken == null)
                                throw new FormatException("Not enough pixel data.");
                            Pixels[i, j, k] = ushort.Parse(pixelToken);
                        }
                    }
                }
            }
        }

        public override void Save(string filePath, bool isBinary)
        {
            if (isBinary)
            {
                using var bw = new BinaryWriter(File.Open(filePath, FileMode.Create));
                bw.Write(Encoding.ASCII.GetBytes($"P6\n{Width} {Height}\n{MaxVal}\n"));

                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            if (MaxVal < 256)
                                bw.Write((byte)Pixels[i, j, k]);
                            else
                                WriteBigEndianUShort(bw, Pixels[i, j, k]);
                        }
                    }
                }
            }
            else
            {
                using var writer = new StreamWriter(filePath);
                writer.WriteLine("P3");
                writer.WriteLine($"{Width} {Height}");
                writer.WriteLine($"{MaxVal}");

                for (int i = 0; i < Height; i++)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int j = 0; j < Width; j++)
                    {
                        sb.Append($"{Pixels[i, j, 0]} {Pixels[i, j, 1]} {Pixels[i, j, 2]} ");
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

        private ushort ReadBigEndianUShort(BinaryReader br)
        {
            byte high = br.ReadByte();
            byte low = br.ReadByte();
            return (ushort)((high << 8) | low);
        }

        private void WriteBigEndianUShort(BinaryWriter bw, ushort value)
        {
            bw.Write((byte)(value >> 8));
            bw.Write((byte)(value & 0xFF));
        }
    }
}
