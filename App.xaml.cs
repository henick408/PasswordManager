using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Core.Config;

namespace PasswordManager;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        ServiceCollection services = new();
        await services.ConfigureSupabase(configuration);
        services.AddServices();
        services.AddRepositories();
        services.AddSingleton<MainWindow>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        MainWindow mainWindow = serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}

