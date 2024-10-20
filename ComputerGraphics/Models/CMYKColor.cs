using ComputerGraphics.MVVM;

namespace ComputerGraphics.Models
{
    public class CMYKColor : ViewModelBase
    {
        public double Cyan { get; set; }
        public double Magenta { get; set; }
        public double Yellow { get; set; }
        public double Black { get; set; }

        public CMYKColor()
        {
            Cyan = 0;
            Magenta = 0;
            Yellow = 0;
            Black = 0;
        }
    }
}
