using System.Windows;
using System.Windows.Controls;
using PasswordManager.ViewModels;

namespace PasswordManager.Views.UserControls;

public partial class PasswordControl : UserControl
{
    public PasswordControl()
    {
        InitializeComponent();
    }

    private void CopyUsername_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not PasswordControlViewModel viewModel)
        {
            return;
        }
        Clipboard.SetText(viewModel.PasswordEntry.Username);
    }

    private void CopyPassword_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not PasswordControlViewModel viewModel)
        {
            return;
        }
        Clipboard.SetText(viewModel.PasswordEntry.Password);
    }

    private void CopyUrl_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not PasswordControlViewModel viewModel)
        {
            return;
        }
        Clipboard.SetText(viewModel.PasswordEntry.Url);
    }
}