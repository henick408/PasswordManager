using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Config;
using PasswordManager.Views.Windows;

namespace PasswordManager;

public class App : Application
{

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    public override void OnFrameworkInitializationCompleted()
    {
        ServiceCollection services = new();
        services.AddDependencies();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // try
            // {
            //     services.ConfigureSupabase();
            // }
            // catch (RealtimeException ex) 
            //     when (ex.InnerException.ToString().Contains("WebSocketException"))
            // {
            //     var result = MessageBox.Show("Connection problems. Could not connect to the server",
            //         "Cannot connect",
            //         MessageBoxButton.OK,
            //         MessageBoxImage.Error
            //     ); 
            //     desktop.Shutdown(1); 
            //     return;
            // }
            services.AddTransient<MainWindow>();
            services.AddTransient<LoginRegisterWindow>();
            services.AddSingleton<Func<MainWindow>>(sp => sp.GetRequiredService<MainWindow>);
            services.AddSingleton<Func<LoginRegisterWindow>>(sp => sp.GetRequiredService<LoginRegisterWindow>);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            desktop.MainWindow = serviceProvider.GetRequiredService<LoginRegisterWindow>();
            base.OnFrameworkInitializationCompleted();
        }
    }
}

