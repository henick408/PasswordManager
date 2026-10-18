using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using PasswordManager.Dto;
using PasswordManager.ViewModels;

namespace PasswordManager.Views.Windows;

public partial class MainWindow : Window
{
    private readonly MainViewModel viewModel;
    private readonly Func<LoginRegisterWindow> loginRegisterWindowFactory;

    public MainWindow()
    {
        InitializeComponent();
    }
    
    public MainWindow(MainViewModel viewModel, Func<LoginRegisterWindow> loginRegisterWindowFactory)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        DataContext = this.viewModel;
        PasswordList.AddHandler(
            PointerPressedEvent,
            PasswordList_PointerPressed,
            RoutingStrategies.Tunnel);
        this.loginRegisterWindowFactory = loginRegisterWindowFactory;
        viewModel.SignedOut += OnSignedOut;
        Opened += MainWindow_Opened;
    }
    
    private async void AllPasswordsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await viewModel.ListPasswords();
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