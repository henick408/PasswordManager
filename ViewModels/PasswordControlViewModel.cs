using PasswordManager.Dto;

namespace PasswordManager.ViewModels;

public class PasswordControlViewModel : ViewModelBase
{

    private PasswordEntry passwordEntry;
    public PasswordEntry PasswordEntry
    {
        get => passwordEntry;
        set => SetProperty(ref passwordEntry, value);
    }

    // Mock data for AXAML preview
    public PasswordControlViewModel()
    {
        PasswordEntry = new PasswordEntry
        {
            Id = 1,
            Category = "Category",
            Name = "Name",
            Username = "Username",
            Password = "Password",
            Notes = "Notes",
            Url = "Url"
        };
    }
    
    public PasswordControlViewModel(PasswordEntry passwordEntry)
    {
        PasswordEntry = passwordEntry;
    }
    
}