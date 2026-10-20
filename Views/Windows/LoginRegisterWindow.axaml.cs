using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
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
        this.viewModel.RegisterSwap += FocusEmail;
        this.mainWindowFactory = mainWindowFactory;
        Loaded += Window_OnOpened;
    }

    private void Window_OnOpened(object? sender, RoutedEventArgs e)
    {
        FocusEmail();
    }
    
    private void FocusEmail()
    {
        var textBox = this.GetVisualDescendants()
            .OfType<TextBox>()
            .FirstOrDefault();

        textBox?.Focus();
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