using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Config;
using PasswordManager.Service;
using PasswordManager.Views.Windows;
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
            var result = MessageBox.Show("Connection problems. Could not connect to the server",
                "Cannot connect",
                MessageBoxButton.OK,
                MessageBoxImage.Error
                ); 
            Shutdown(1); 
            return;
        }

        services.AddServices();
        services.AddRepositories();
        services.AddTransient<MainWindow>();
        services.AddTransient<LoginRegisterScreen>();
        services.AddSingleton<Func<MainWindow>>(sp => () => sp.GetRequiredService<MainWindow>());
        services.AddSingleton<Func<LoginRegisterScreen>>(sp => () => sp.GetRequiredService<LoginRegisterScreen>());
        services.AddViewModels();

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        var loginWindow = serviceProvider.GetRequiredService<LoginRegisterScreen>();
        loginWindow.Show();
    }
}

