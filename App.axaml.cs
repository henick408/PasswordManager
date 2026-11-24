using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PasswordManager.Config;
using PasswordManager.Utils;
using PasswordManager.Views.Windows;

namespace PasswordManager;

public class App : Application
{
    /// <summary>Prevents stacking several "cannot connect" dialogs when multiple requests fail at once.</summary>
    private bool isConnectionErrorShown;

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
            Dispatcher.UIThread.UnhandledException += OnUnhandledException;

            services.AddTransient<MainWindow>();
            services.AddTransient<LoginRegisterWindow>();
            services.AddSingleton<Func<MainWindow>>(sp => sp.GetRequiredService<MainWindow>);
            services.AddSingleton<Func<LoginRegisterWindow>>(sp => sp.GetRequiredService<LoginRegisterWindow>);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            desktop.MainWindow = serviceProvider.GetRequiredService<LoginRegisterWindow>();
            base.OnFrameworkInitializationCompleted();
        }
    }

    /// <summary>
    /// Last line of defence for requests made from commands (loading, adding, editing, deleting entries):
    /// a lost connection shows an error dialog instead of crashing the app. Sign-in and sign-up
    /// handle connection errors themselves and show them in the sign-in window.
    /// Any other unhandled exception is left alone.
    /// </summary>
    private async void OnUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
    {
        if (!NetworkErrors.IsConnectionProblem(e.Exception)) return;

        e.Handled = true;
        if (isConnectionErrorShown) return;

        isConnectionErrorShown = true;
        try
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "Cannot connect",
                    "Could not connect to the server. Check your internet connection and try again.",
                    ButtonEnum.Ok, Icon.Error)
                .ShowAsync();
        }
        finally
        {
            isConnectionErrorShown = false;
        }
    }
}
