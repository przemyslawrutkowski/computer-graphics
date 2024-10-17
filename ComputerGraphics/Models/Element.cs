using ComputerGraphics.MVVM;
using System.Windows;

namespace ComputerGraphics.Models
{
    public class Element : ViewModelBase
    {
        private double _x;
        public double X
        {
            get => _x;
            set
            {
                _x = value;
                RaisePropertyChanged();
            }
        }

        private double _y;
        public double Y
        {
            get => _y;
            set
            {
                _y = value;
                RaisePropertyChanged();
            }
        }

        public UIElement UIElement { get; set; }

        public Element(UIElement uiElement)
        {
            UIElement = uiElement;
        }

    }
}
