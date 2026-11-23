using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using PasswordManager.ViewModels;

namespace PasswordManager.Views.Windows;

public partial class MasterPasswordDialog : Window
{
    public MasterPasswordDialog()
    {
        InitializeComponent();
    }

    public MasterPasswordDialog(MasterPasswordDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseRequested += password => Close(password);
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        this.GetVisualDescendants().OfType<TextBox>().FirstOrDefault()?.Focus();
    }
}
