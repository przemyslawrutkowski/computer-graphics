using ComputerGraphics.MVVM;

namespace ComputerGraphics.Models
{
    public class RGBColor : ViewModelBase
    {
        private byte _red;
        public byte Red
        {
            get => _red;
            set
            {
                _red = value;
                RaisePropertyChanged();
            }
        }

        private byte _green;
        public byte Green
        {
            get => _green;
            set
            {
                _green = value;
                RaisePropertyChanged();
            }
        }

        private byte _blue;
        public byte Blue
        {
            get => _blue;
            set
            {
                _blue = value;
                RaisePropertyChanged();
            }
        }

        public void UpdateFrom(RGBColor other)
        {
            Red = other.Red;
            Green = other.Green;
            Blue = other.Blue;
        }
    }
}
