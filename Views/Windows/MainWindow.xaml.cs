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

    private async void AllPasswordsButton_OnClick(object sender, RoutedEventArgs e)
    {
        viewModel.ClearSelectedCategory();
    }

    private void PasswordListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        var listBox = (ListBox)sender;

        if (e.OriginalSource is not DependencyObject source)
        {
            return;
        }

        if (ItemsControl.ContainerFromElement(listBox, source) is ListBoxItem container)
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

    private async void SignInButton_OnClick(object sender, RoutedEventArgs e)
    {
        await viewModel.SignIn();
        await viewModel.ListPasswords();
    }

    private async void CreatePassword_OnClick(object sender, RoutedEventArgs e)
    {
        await viewModel.CreatePassword();
    }

    private async void EditPassword_OnClick(object sender, RoutedEventArgs e)
    {
        await viewModel.UpdatePassword();
    }
    private async void DeletePassword_OnClick(object sender, RoutedEventArgs e)
    {
        await viewModel.DeletePassword();
    }

    private void PasswordListBox_PreviewRightMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }
}