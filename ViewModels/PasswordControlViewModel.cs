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

    public PasswordControlViewModel() {}
    
    public PasswordControlViewModel(PasswordEntry passwordEntry)
    {
        PasswordEntry = passwordEntry;
    }
    
}