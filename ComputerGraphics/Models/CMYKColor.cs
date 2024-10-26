using ComputerGraphics.MVVM;

namespace ComputerGraphics.Models
{
    public class CMYKColor : ViewModelBase
    {
        private double _cyan;
        public double Cyan
        {
            get => _cyan;
            set
            {
                _cyan = value;
                RaisePropertyChanged();
            }
        }

        private double _magenta;
        public double Magenta
        {
            get => _magenta;
            set
            {
                _magenta = value;
                RaisePropertyChanged();
            }
        }

        private double _yellow;
        public double Yellow
        {
            get => _yellow;
            set
            {
                _yellow = value;
                RaisePropertyChanged();
            }
        }

        private double _black;
        public double Black
        {
            get => _black;
            set
            {
                _black = value;
                RaisePropertyChanged();
            }
        }

        public void UpdateFrom(CMYKColor other)
        {
            Cyan = other.Cyan;
            Magenta = other.Magenta;
            Yellow = other.Yellow;
            Black = other.Black;
        }
    }
}
