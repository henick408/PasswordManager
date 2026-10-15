using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
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
        PasswordList.AddHandler(
            PointerPressedEvent,
            PasswordList_PointerPressed,
            RoutingStrategies.Tunnel);
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        await viewModel.authService.SignInAsync(new UserRequest("test@test.com", "testtest"));
        await viewModel.ListPasswords();
        //await viewModel.authService.SignOut();
    }

    private async void AllPasswordsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await viewModel.ListPasswords();
    }

    private void LogoutButton_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
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