using ComputerGraphics.MVVM;

namespace ComputerGraphics.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase? _selectedVM;

        public GraphicsOperationsViewModel GraphicsOperationsVM { get; }
        public ColorSpacesViewModel ColorSpacesVM { get; }
        public FilesViewModel FilesVM { get; }
        public DigitalFiltersViewModel DigitalFiltersVM { get; }

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

        public MainWindowViewModel(GraphicsOperationsViewModel graphicsOperationsVM,
            ColorSpacesViewModel colorSpacesVM, FilesViewModel filesVM, DigitalFiltersViewModel digitalFiltersVM)
        {
            GraphicsOperationsVM = graphicsOperationsVM;
            ColorSpacesVM = colorSpacesVM;
            FilesVM = filesVM;
            DigitalFiltersVM = digitalFiltersVM;
            SelectedVM = GraphicsOperationsVM;
            SelectVMCommand = new DelegateCommand(SelectVM);
        }

        private void SelectVM(object? parameter)
        {
            SelectedVM = parameter as ViewModelBase;
        }
    }
}
