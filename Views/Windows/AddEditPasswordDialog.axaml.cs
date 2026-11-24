using Avalonia.Controls;
using Avalonia.Interactivity;
using PasswordManager.ViewModels;

namespace PasswordManager.Views.Windows;

public partial class AddEditPasswordDialog : Window
{
    private readonly AddEditPasswordViewModel? viewModel;

    public AddEditPasswordDialog()
    {
        InitializeComponent();
    }

    public AddEditPasswordDialog(AddEditPasswordViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        DataContext = viewModel;
    }

    /// <summary>Closes with <c>true</c> only when the entry is valid; otherwise the errors are shown in the form.</summary>
    private void SaveChangesButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (viewModel is null || viewModel.TryPrepareForSave())
        {
            Close(true);
        }
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}