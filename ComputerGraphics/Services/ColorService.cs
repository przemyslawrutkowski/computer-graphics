using ComputerGraphics.Models;
using System.Windows.Media;

namespace ComputerGraphics.Services
{
    public interface IColorService
    {
        Color ConvertFromRGB(RGBColor rgb);
        Color ConvertFromCMYK(CMYKColor cmyk);
        Color ConvertFromHSV(HSVColor hsv);
        RGBColor ConvertToRGB(Color color);
        CMYKColor ConvertToCMYK(Color color);
        HSVColor ConvertToHSV(Color color);
    }

    public class ColorService : IColorService
    {
        public Color ConvertFromRGB(RGBColor rgb)
        {
            return Color.FromRgb(rgb.Red, rgb.Green, rgb.Blue);
        }

        public Color ConvertFromCMYK(CMYKColor cmyk)
        {
            double c = cmyk.Cyan / 100;
            double m = cmyk.Magenta / 100;
            double y = cmyk.Yellow / 100;
            double k = cmyk.Black / 100;

            byte r = (byte)(255 * (1 - c) * (1 - k));
            byte g = (byte)(255 * (1 - m) * (1 - k));
            byte b = (byte)(255 * (1 - y) * (1 - k));

            return Color.FromRgb(r, g, b);
        }

        public Color ConvertFromHSV(HSVColor hsv)
        {
            double h = hsv.Hue;
            double s = hsv.Saturation / 100;
            double v = hsv.Value / 100;

            double c = v * s;
            double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            double m = v - c;

            double rPrime = 0;
            double gPrime = 0;
            double bPrime = 0;

            if (hsv.Hue >= 0 && hsv.Hue < 60)
            {
                rPrime = c;
                gPrime = x;
                bPrime = 0;
            }
            else if (hsv.Hue >= 60 && hsv.Hue < 120)
            {
                rPrime = x;
                gPrime = c;
                bPrime = 0;
            }
            else if (hsv.Hue >= 120 && hsv.Hue < 180)
            {
                rPrime = 0;
                gPrime = c;
                bPrime = x;
            }
            else if (hsv.Hue >= 180 && hsv.Hue < 240)
            {
                rPrime = 0;
                gPrime = x;
                bPrime = c;
            }
            else if (hsv.Hue >= 240 && hsv.Hue < 300)
            {
                rPrime = x;
                gPrime = 0;
                bPrime = c;
            }
            else if (hsv.Hue >= 300 && hsv.Hue < 360)
            {
                rPrime = c;
                gPrime = 0;
                bPrime = x;
            }

            byte r = (byte)((rPrime + m) * 255);
            byte g = (byte)((gPrime + m) * 255);
            byte b = (byte)((bPrime + m) * 255);

            return Color.FromRgb(r, g, b);
        }


        public RGBColor ConvertToRGB(Color color)
        {
            return new RGBColor
            {
                Red = color.R,
                Green = color.G,
                Blue = color.B
            };
        }

        public CMYKColor ConvertToCMYK(Color color)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double k = 1 - Math.Max(r, Math.Max(g, b));
            double c = (1 - r - k) / (1 - k);
            double m = (1 - g - k) / (1 - k);
            double y = (1 - b - k) / (1 - k);

            return new CMYKColor
            {
                Cyan = c * 100,
                Magenta = m * 100,
                Yellow = y * 100,
                Black = k * 100
            };
        }

        public HSVColor ConvertToHSV(Color color)
        {
            double rPrime = color.R / 255.0;
            double gPrime = color.G / 255.0;
            double bPrime = color.B / 255.0;

            double cMax = Math.Max(rPrime, Math.Max(gPrime, bPrime));
            double cMin = Math.Min(rPrime, Math.Min(gPrime, bPrime));
            double delta = cMax - cMin;

            double h = 0;
            if (delta != 0)
            {
                if (cMax == rPrime)
                {
                    h = 60 * ((gPrime - bPrime) / delta % 6);
                }
                else if (cMax == gPrime)
                {
                    h = 60 * ((bPrime - rPrime) / delta + 2);
                }
                else if (cMax == bPrime)
                {
                    h = 60 * ((rPrime - gPrime) / delta + 4);
                }
            }

            double s = cMax == 0 ? 0 : delta / cMax;
            double v = cMax;

            return new HSVColor
            {
                Hue = h,
                Saturation = s * 100,
                Value = v * 100
            };
        }
    }
}
