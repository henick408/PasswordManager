using PasswordManager.Dto;

namespace PasswordManager.ViewModels;

public class PasswordControlViewModel : ViewModelBase
{

    private PasswordEntry passwordEntry;

    public PasswordEntry PasswordEntry
    {
        get => passwordEntry;
        set => SetField(ref passwordEntry, value);
    }

    public PasswordControlViewModel(PasswordEntry passwordEntry)
    {
        this.PasswordEntry = passwordEntry;
    }
    
}