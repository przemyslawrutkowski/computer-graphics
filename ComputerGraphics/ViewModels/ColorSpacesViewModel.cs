using ComputerGraphics.MVVM;

namespace ComputerGraphics.ViewModels
{
    public class ColorSpacesViewModel : ViewModelBase
    {
        private ViewModelBase? _selectedVM;

        public ColorConverterViewModel ColorConverterVM { get; }
        public RgbCubeViewModel RgbCubeVM { get; }
        public ViewModelBase? SelectedVM
        {
            get => _selectedVM;
            private set
            {
                _selectedVM = value;
                RaisePropertyChanged();
            }
        }
        public DelegateCommand SelectVMCommand { get; }

        public ColorSpacesViewModel(ColorConverterViewModel colorConverterVM, RgbCubeViewModel rgbCubeVM)
        {
            ColorConverterVM = colorConverterVM;
            RgbCubeVM = rgbCubeVM;
            SelectedVM = ColorConverterVM;
            SelectVMCommand = new DelegateCommand(SelectVM);
        }

        private void SelectVM(object? parameter)
        {
            SelectedVM = parameter as ViewModelBase;
        }
    }
}
