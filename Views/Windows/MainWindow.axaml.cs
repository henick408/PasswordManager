using Avalonia.Controls;
using Avalonia.Interactivity;
using PasswordManager.Dto;
using PasswordManager.ViewModels;

namespace PasswordManager.Views.Windows;

public partial class MainWindow : Window
{
    private readonly MainViewModel viewModel;
    
    
    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        viewModel = vm;
        DataContext = viewModel;
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        await viewModel.authService.SignIn(new UserRequest("test@test.com", "testtest"));
        await viewModel.ListPasswords();
        await viewModel.authService.SignOut();
    }
}