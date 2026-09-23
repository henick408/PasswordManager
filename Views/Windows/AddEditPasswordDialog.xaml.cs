using System.Windows;
using System.Windows.Controls;
using PasswordManager.Dto;

namespace PasswordManager.Views.Windows;

public partial class AddEditPasswordDialog : Window
{
    private bool isSyncing;
    public PasswordEntry PasswordEntry { get; }

    public AddEditPasswordDialog(IList<string> categories, PasswordEntry? existing = null)
    {
        InitializeComponent();
        CategoryComboBox.ItemsSource = categories;
        PasswordEntry = existing ?? new PasswordEntry();
        Loaded += (_, _) => NameBox.Focus();
        if (existing != null)
        {
            HeaderBlock.Text = "Edit password";
            NameBox.Text = existing.Name;
            UrlBox.Text = existing.Url;
            UsernameBox.Text = existing.Username;
            HiddenPasswordBox.Password = existing.Password;
            CategoryComboBox.SelectedItem = existing.Category;
            NotesBox.Text = existing.Notes;
        }
    }

    private void GeneratorExpander_OnClick(object sender, RoutedEventArgs e)
    {
        GeneratorExpander.IsExpanded = !GeneratorExpander.IsExpanded;

        GenerateButton.Content = !GeneratorExpander.IsExpanded ? "Generate" : "Hide";
    }

    private void SaveChangesButton_OnClick(object sender, RoutedEventArgs e)
    {
        string name = NameBox.Text;
        string url = UrlBox.Text.Trim();
        string username = UsernameBox.Text.Trim();
        string password = HiddenPasswordBox.Password.Trim();
        string? category = CategoryComboBox.SelectedItem as string;
        string notes = NotesBox.Text.Trim();
        
        List<string> errors = new();
        if (string.IsNullOrEmpty(name)) errors.Add("Name cannot be empty.");
        if (string.IsNullOrEmpty(url)) errors.Add("Url cannot be empty.");
        if (string.IsNullOrEmpty(username)) errors.Add("Username cannot be empty.");
        if (string.IsNullOrEmpty(password)) errors.Add("Password cannot be empty.");
        if (category == null) errors.Add("Category must be selected");

        if (errors.Count > 0)
        {
            MessageBox.Show(string.Join("\n", errors), "Missing data", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        PasswordEntry.Name = name;
        PasswordEntry.Url = url;
        PasswordEntry.Username = username;
        PasswordEntry.Password = password;
        PasswordEntry.Category = category!;
        PasswordEntry.Notes = notes;

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