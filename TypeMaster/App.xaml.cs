using AsyncAwaitBestPractices;
using Microsoft.Extensions.DependencyInjection;
using System;

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

            services.AddTransient<SearchArticlesViewModel>();
            services.AddTransient<TypeTestViewModel>();
            services.AddTransient<ScoreboardViewModel>();
            services.AddTransient<ChooseTextLengthViewModel>();


            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<WikipediaService>();
            services.AddSingleton<DataSaveLoadService>();
            services.AddSingleton<CryptographyService>();
            services.AddSingleton<LanguagesService>();
            services.AddSingleton<SettingsService>();
            services.AddSingleton<CurrentPageService>();
            services.AddSingleton<ColorsService>();
            services.AddSingleton<NetworkAvailabilityService>();

            services.AddSingleton<Func<Type, BaseViewModel>>(serviceProvider => viewModelType => (BaseViewModel)serviceProvider.GetRequiredService(viewModelType));
            _serviceProvider = services.BuildServiceProvider();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            var settingsService = _serviceProvider.GetRequiredService<SettingsService>();

            await settingsService.GetSettingsDataAsync();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            var wikipediaService = _serviceProvider.GetRequiredService<WikipediaService>();
            var settingsService = _serviceProvider.GetRequiredService<SettingsService>();

            settingsService.SaveSettingsData().SafeFireAndForget();
            wikipediaService.SaveScoresData().SafeFireAndForget();

            base.OnExit(e);
        }
    }
}
