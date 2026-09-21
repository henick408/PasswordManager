using System.Windows;
using PasswordManager.Dto;
using PasswordManager.ViewModels;

namespace PasswordManager.Views.Windows;

public partial class AddEditPasswordDialog : Window
{

    public AddEditPasswordDialog()
    {
        InitializeComponent();
        
    }
    
    private void GeneratorExpander_OnClick(object sender, RoutedEventArgs e)
    {
        GeneratorExpander.IsExpanded = !GeneratorExpander.IsExpanded;

        GenerateButton.Content = !GeneratorExpander.IsExpanded ? "Generate" : "Hide";
    }

    private void SaveChangesButton_OnClick(object sender, RoutedEventArgs e)
    {
        
    }

    private void RevealPasswordButton_OnClick(object sender, RoutedEventArgs e)
    {
        
    }
}