using System.Windows;
using System.Windows.Controls;
using PasswordManager.Dto;
using PasswordManager.ViewModels;

namespace PasswordManager.Views.Windows;

public partial class AddEditPasswordDialog : Window
{
    private readonly AddEditPasswordViewModel viewModel;
    private bool isSyncing;

    public AddEditPasswordDialog(IList<string> categories, PasswordEntry? existingPassword = null)
    {
        InitializeComponent();
        viewModel = new AddEditPasswordViewModel(categories, existingPassword);
        DataContext = viewModel;
        if (existingPassword != null)
        {
            HeaderBlock.Text = "Edit password";
            HiddenPasswordBox.Password = existingPassword.Password;
        }
    }
    
    public PasswordEntry Password { get; private set; }

    private void GeneratorExpander_OnClick(object sender, RoutedEventArgs e)
    {
        GeneratorExpander.IsExpanded = !GeneratorExpander.IsExpanded;

        GenerateButton.Content = !GeneratorExpander.IsExpanded ? "Generate" : "Hide";
    }

    private void SaveChangesButton_OnClick(object sender, RoutedEventArgs e)
    {
        viewModel.Password.Password = HiddenPasswordBox.Password;
        viewModel.Password.Category = viewModel.SelectedCategory;
        Password = viewModel.Password;
        DialogResult = true;
    }


    private void HiddenPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (isSyncing) return;

        isSyncing = true;
        RevealedTextBox.Text = HiddenPasswordBox.Password;
        isSyncing = false;
    }
    
    private void RevealedTextBox_OnPasswordChanged(object sender, TextChangedEventArgs e)
    {
        if (isSyncing) return;

        isSyncing = true;
        HiddenPasswordBox.Password = RevealedTextBox.Text;
        isSyncing = false;
    }
    
    private void RevealButton_OnChecked(object sender, RoutedEventArgs e)
    {
        RevealedTextBox.Visibility = Visibility.Visible;
        HiddenPasswordBox.Visibility = Visibility.Collapsed;
    }

    private void RevealButton_OnUnchecked(object sender, RoutedEventArgs e)
    {
        HiddenPasswordBox.Visibility = Visibility.Visible;
        RevealedTextBox.Visibility = Visibility.Collapsed;
    }
}