using ComputerGraphics.MVVM;

namespace ComputerGraphics.Models
{
    public class HSVColor : ViewModelBase
    {
        private double _hue;
        private double _saturation;
        private double _value;

        public double Hue
        {
            get => _hue;
            set
            {
                if (_hue != value)
                {
                    _hue = value;
                    RaisePropertyChanged();
                }
            }
        }

        public double Saturation
        {
            get => _saturation;
            set
            {
                if (_saturation != value)
                {
                    _saturation = value;
                    RaisePropertyChanged();
                }
            }
        }

        public double Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    RaisePropertyChanged();
                }
            }
        }
        public void UpdateFrom(HSVColor other)
        {
            Hue = other.Hue;
            Saturation = other.Saturation;
            Value = other.Value;
        }
    }
}
