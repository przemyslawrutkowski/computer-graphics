using System.Windows.Controls;

namespace ComputerGraphics.Controls
{
    /// <summary>
    /// Interaction logic for ToolbarControl.xaml
    /// </summary>
    public partial class ToolbarControl : UserControl
    {
        public event EventHandler? SaveImageEventHandler;
        public ToolbarControl()
        {
            InitializeComponent();
        }

        private void SaveImage(object sender, System.Windows.RoutedEventArgs e)
        {
            SaveImageEventHandler?.Invoke(this, EventArgs.Empty);
        }
    }
}
