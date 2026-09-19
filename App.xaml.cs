using System.Configuration;
using System.Data;
using System.Drawing;
using System.Net.WebSockets;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Config;
using Supabase.Realtime.Exceptions;

namespace PasswordManager;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ServiceCollection services = new();
        try
        {
            await services.ConfigureSupabase();
        }
        catch (RealtimeException ex) 
            when (ex.InnerException.ToString().Contains("WebSocketException"))
        {
            var result = MessageBox.Show("Connection problems. Could not connect to the database",
                "Cannot connect",
                MessageBoxButton.OK,
                MessageBoxImage.Error
                );
            if (result == MessageBoxResult.OK)
            {
                Shutdown(1);
                return;
            }
        }

        services.AddServices();
        services.AddRepositories();
        services.AddSingleton<MainWindow>();
        services.AddViewModels();
        
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        MainWindow mainWindow = serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}

