using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace TypeMaster
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;
        public App()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<MainWindow>(provider => new MainWindow
            {
                DataContext = provider.GetRequiredService<MainViewModel>()
                
            }); 

            services.AddSingleton<MainViewModel>();
            services.AddSingleton<HomeViewModel>();
            services.AddSingleton<TypeTestViewModel>();

            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<WikipediaService>();
            services.AddSingleton<DataSaveLoadService>();

            services.AddSingleton<Func<Type, BaseViewModel>>(serviceProvider => viewModelType => (BaseViewModel)serviceProvider.GetRequiredService(viewModelType));
            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            var dataSaveLoadService = _serviceProvider.GetRequiredService<DataSaveLoadService>();
            var wikipediaService = _serviceProvider.GetRequiredService<WikipediaService>();
            dataSaveLoadService.SaveWikipediaPageInfos(wikipediaService.scores);

            base.OnExit(e);
        }
    }
}
