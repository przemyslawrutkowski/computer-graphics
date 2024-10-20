using ComputerGraphics.Models;
using ComputerGraphics.Services;
using ComputerGraphics.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace ComputerGraphics
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddTransient<MainWindow>();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<GraphicsOperationsViewModel>();
            services.AddTransient<ColorSpacesViewModel>();

            services.AddTransient<IElementFactory, ElementFactory>();
            services.AddTransient<IElementUpdater, ElementUpdater>();
            services.AddTransient<IImageService, ImageService>();
            services.AddTransient<IColorService, ColorService>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow?.Show();
        }
    }
}
