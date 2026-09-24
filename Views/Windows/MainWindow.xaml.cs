using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PasswordManager.ViewModels;

namespace PasswordManager.Views.Windows;

public partial class MainWindow : Window
{
    private readonly MainViewModel viewModel;
    private readonly Func<LoginRegisterScreen> loginRegisterScreenFactory;

    public MainWindow(MainViewModel viewModel, Func<LoginRegisterScreen> loginRegisterScreenFactory)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        this.loginRegisterScreenFactory = loginRegisterScreenFactory;
        DataContext = this.viewModel;
        Loaded += MainWindow_OnLoaded;
    }

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        await viewModel.ListPasswords();
    }

    private void AllPasswordsButton_OnClick(object sender, RoutedEventArgs e)
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

    private async void LogoutButton_OnClick(object sender, RoutedEventArgs e)
    {
        await viewModel.LogOut();
        var loginWindow = loginRegisterScreenFactory();
        Application.Current.MainWindow = loginWindow;
        loginWindow.Show();
        Close();
    }
}