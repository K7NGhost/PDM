using Microsoft.Extensions.DependencyInjection;
using PDM.Src.Models;
using PDM.Src.ViewModels;
using PDM.ui;
using PDM.ui.Pages;
using System.Configuration;
using System.Data;
using System.Windows;

namespace PDM
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
            var mainWindow = ServiceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Core Services
            services.AddSingleton<DatabaseManager>();
            services.AddSingleton<DatabaseManagerViewModel>();
            services.AddSingleton<PhoneListViewModel>();
            services.AddSingleton<PhoneDataViewModel>();
            services.AddSingleton<PopupEditViewModel>();

            // Views
            services.AddTransient<AddPhonePage>();
            services.AddSingleton<PhoneListPage>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<DashboardPage>();
        }
    }

}
