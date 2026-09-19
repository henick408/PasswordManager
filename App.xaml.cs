using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Config;

namespace PasswordManager;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ServiceCollection services = new();
        await services.ConfigureSupabase();
        services.AddServices();
        services.AddRepositories();
        services.AddSingleton<MainWindow>();
        services.AddViewModels();
        
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        MainWindow mainWindow = serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}

