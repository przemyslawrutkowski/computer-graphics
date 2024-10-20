using ComputerGraphics.MVVM;

namespace ComputerGraphics.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase? _selectedViewModel;

        public GraphicsOperationsViewModel GraphicsOperationsViewModel { get; }
        public ColorSpacesViewModel ColorSpacesViewModel { get; }
        public ViewModelBase? SelectedViewModel
        {
            get => _selectedViewModel;
            private set
            {
                _selectedViewModel = value;
                RaisePropertyChanged();
            }
        }
        public DelegateCommand SelectViewModelCommand { get; }

        public MainWindowViewModel(GraphicsOperationsViewModel graphicsOperationsViewModel,
            ColorSpacesViewModel colorSpacesViewModel)
        {
            GraphicsOperationsViewModel = graphicsOperationsViewModel;
            ColorSpacesViewModel = colorSpacesViewModel;
            SelectedViewModel = GraphicsOperationsViewModel;
            SelectViewModelCommand = new DelegateCommand(SelectViewModel);
        }

        private void SelectViewModel(object? parameter)
        {
            SelectedViewModel = parameter as ViewModelBase;
        }
    }
}
