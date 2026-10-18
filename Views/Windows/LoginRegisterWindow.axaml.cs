using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using PasswordManager.ViewModels;

namespace PasswordManager.Views.Windows;

public partial class LoginRegisterWindow : Window
{
    private readonly LoginRegisterViewModel viewModel;
    private readonly Func<MainWindow> mainWindowFactory;

    public LoginRegisterWindow()
    {
        InitializeComponent();
    }
    
    public LoginRegisterWindow(LoginRegisterViewModel viewModel, Func<MainWindow> mainWindowFactory)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        DataContext = this.viewModel;
        this.viewModel.SignInSuccess += OnSignInSuccess;
        this.mainWindowFactory = mainWindowFactory;
    }
    
    private void OnSignInSuccess()
    {
        var mainWindow = mainWindowFactory();
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = mainWindow;
        }
        mainWindow.Show();
        Close();
    }
}