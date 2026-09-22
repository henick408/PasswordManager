using System.Windows.Controls;
using PasswordManager.Dto;

namespace PasswordManager.ViewModels;

public class AddEditPasswordViewModel : ViewModelBase
{
    private string selectedCategory;
    public PasswordEntry Password { get; }
    public IList<string> Categories { get; }

    public string SelectedCategory
    {
        get => selectedCategory;
        set
        {
            SetField(ref selectedCategory, value);
            Password.Category = selectedCategory;
        }
    }

    public AddEditPasswordViewModel(IList<string> categories, PasswordEntry? existingPassword)
    {
        Categories = categories;
        Password = existingPassword ?? new PasswordEntry();
        SelectedCategory = existingPassword != null ? existingPassword.Category : Categories[0];
    }

    

    
}