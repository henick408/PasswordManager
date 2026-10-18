using Avalonia.Controls;
using Avalonia.Interactivity;
using PasswordManager.ViewModels;

namespace PasswordManager.Views.Windows;

public partial class AddEditPasswordDialog : Window
{

    public AddEditPasswordDialog()
    {
        InitializeComponent();
    }
    
    public AddEditPasswordDialog(AddEditPasswordViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void SaveChangesButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}