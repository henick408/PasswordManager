using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using PasswordManager.Service;
using PasswordManager.Utils;
using PasswordManager.ViewModels;

namespace PasswordManager.Views.Windows;

public partial class MainWindow : Window
{
    private readonly MainViewModel viewModel;
    private readonly Func<LoginRegisterWindow> loginRegisterWindowFactory;
    private readonly AutoLockService? autoLockService;

    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(MainViewModel viewModel, Func<LoginRegisterWindow> loginRegisterWindowFactory,
        AutoLockService autoLockService)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        DataContext = this.viewModel;
        PasswordList.AddHandler(
            PointerPressedEvent,
            PasswordList_PointerPressed,
            RoutingStrategies.Tunnel);
        this.loginRegisterWindowFactory = loginRegisterWindowFactory;
        this.autoLockService = autoLockService;
        viewModel.SignedOut += OnSignedOut;
        Opened += MainWindow_Opened;
        Closing += MainWindow_Closing;
        Closed += MainWindow_Closed;
        autoLockService.LockRequested += OnLockRequested;
    }

    /// <summary>Inactivity or OS session lock: sign out, which returns to the sign-in window.</summary>
    private async void OnLockRequested(object? sender, EventArgs e)
    {
        await viewModel.SignOutCommand.ExecuteAsync(null);
    }

    /// <summary>
    /// When the app is closed with a password still in the clipboard, postpones closing
    /// until the clipboard is cleared (clipboard operations are asynchronous).
    /// </summary>
    private async void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (!ClipboardHelper.HasSensitiveText || e.CloseReason == WindowCloseReason.OSShutdown) return;

        e.Cancel = true;
        await ClipboardHelper.ClearIfOursAsync();
        Close();
    }

    /// <summary>Stops auto-lock; the service is a singleton and must not keep this window alive.</summary>
    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        if (autoLockService is null) return;
        autoLockService.Stop();
        autoLockService.LockRequested -= OnLockRequested;
    }

    private void OnSignedOut()
    {
        var loginRegisterWindow = loginRegisterWindowFactory();
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = loginRegisterWindow;
        }
        loginRegisterWindow.Show();
        Close();
    }

    private async void MainWindow_Opened(object? sender, EventArgs e)
    {
        autoLockService?.Start();
        await viewModel.WindowOpenedCommand.ExecuteAsync(null);
    }
    
    
    private void PasswordList_PointerPressed(
        object? sender,
        PointerPressedEventArgs e)
    {
        if (sender is not ListBox listBox)
            return;

        var properties = e.GetCurrentPoint(listBox).Properties;

        if (!properties.IsLeftButtonPressed)
        {
            e.Handled = true;
            return;
        }

        var source = e.Source as Control;
        var container = source?.FindAncestorOfType<ListBoxItem>();

        if (container is not null)
        {
            listBox.SelectedItem = container.DataContext;
            container.Focus();
        }
        else
        {
            listBox.SelectedItem = null;
        }

        e.Handled = true;
    }
}