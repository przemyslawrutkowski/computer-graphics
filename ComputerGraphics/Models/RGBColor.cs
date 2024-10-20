using ComputerGraphics.MVVM;

namespace ComputerGraphics.Models
{
    public class RGBColor : ViewModelBase
    {
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }

        public RGBColor()
        {
            Red = 0;
            Green = 0;
            Blue = 0;
        }
    }
}
