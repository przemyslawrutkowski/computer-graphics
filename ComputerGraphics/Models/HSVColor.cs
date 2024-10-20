using ComputerGraphics.MVVM;

namespace ComputerGraphics.Models
{
    public class HSVColor : ViewModelBase
    {
        public double Hue { get; set; }
        public double Saturation { get; set; }
        public double Value { get; set; }

        public HSVColor()
        {
            Hue = 0;
            Saturation = 0;
            Value = 0;
        }
    }
}
