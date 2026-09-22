using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PasswordManager.ViewModels;

namespace PasswordManager;

public partial class MainWindow : Window
{
    private readonly MainViewModel viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        this.DataContext = this.viewModel;
    }

    private async void ListPasswordsButton_OnClick(object sender, RoutedEventArgs e)
    {
        // to jest tymczasowe, tak naprawde ten przycisk ma usunąć filtry
        await viewModel.ListPasswords();
    }

    private void PasswordListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is DependencyObject source &&
            ItemsControl.ContainerFromElement((ListBox)sender, source) is null)
        {
            ((ListBox)sender).SelectedItem = null;
        }
    }

    private async void SignInButton_OnClick(object sender, RoutedEventArgs e)
    {
        await viewModel.SignIn();
    }

    private async void CreatePassword_OnClick(object sender, RoutedEventArgs e)
    {
        await viewModel.CreatePassword();
    }

    private async void EditPassword_OnClick(object sender, RoutedEventArgs e)
    {
        await viewModel.UpdatePassword();
    }

    private void PasswordListBox_PreviewRightMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }
}