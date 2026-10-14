using Avalonia.Controls;
using Avalonia.Interactivity;
using PasswordManager.Dto;
using PasswordManager.ViewModels;

namespace PasswordManager.Views.Windows;

public partial class MainWindow : Window
{
    private readonly MainViewModel viewModel;

    public MainWindow()
    {
        InitializeComponent();
    }
    
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

    private async void AllPasswordsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await viewModel.ListPasswords();
    }

    private void LogoutButton_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void EditPassword_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void DeletePassword_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void RevealPassword_OnClick(object? sender, RoutedEventArgs e)
    {
        PasswordTextBox.RevealPassword = !PasswordTextBox.RevealPassword;
    }

    private void CreatePassword_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}