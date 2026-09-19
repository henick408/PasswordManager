using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows;
using PasswordManager.Dto;
using PasswordManager.Service;

namespace PasswordManager.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly AuthService authService;
    private readonly PasswordService passwordService;
    
    private string selectedCategory;
    private ObservableCollection<PasswordControlViewModel> passwords;
    private PasswordControlViewModel? selectedPassword;
    private bool isPasswordSelected;
    
    public IList<string> Categories { get; } = new List<string>
    {
        "Social", "Work", "Finance", "Shopping", "Entertainment", "Other"
    };

    public string SelectedCategory
    {
        get => selectedCategory;
        set => SetField(ref selectedCategory, value);
    }

    public PasswordControlViewModel? SelectedPassword
    {
        get => selectedPassword;
        set
        {
            if (SetField(ref selectedPassword, value))
            {
                OnPropertyChanged(nameof(IsPasswordSelected));
            }
        }
    }

    public bool IsPasswordSelected => SelectedPassword != null;

    private IList<PasswordEntry> entries = new List<PasswordEntry>
    {
        new PasswordEntry
        {
            Url = "github.com",
            Name = "Github",
            Username = "github@mail.com",
            Category = "Social",
            Notes = "hujhujhuj"
        },
        new PasswordEntry
        {
            Url = "google.com",
            Name = "Google",
            Username = "google@mail.com"
        },
        new PasswordEntry
        {
            Url = "facebook.com",
            Name = "Facebook",
            Username = "facebook@mail.com"
        },
        new PasswordEntry
        {
            Url = "usos.com",
            Name = "Usos",
            Username = "usos@mail.com"
        },
    };

    public ObservableCollection<PasswordControlViewModel> Passwords
    {
        get => passwords;
        set => SetField(ref passwords, value);
    } 

    public MainViewModel(AuthService authService, PasswordService passwordService)
    {
        this.authService = authService;
        this.passwordService = passwordService;
        // Passwords = new ObservableCollection<PasswordControlViewModel>(entries.Select(pass => new PasswordControlViewModel(pass))
        //     .ToList());
    }

    public async Task ListPasswords()
    {
        IList<PasswordEntry> passwordsEntries = await passwordService.GetPasswords();
        Passwords = new ObservableCollection<PasswordControlViewModel>(
            passwordsEntries.Select(entry => new PasswordControlViewModel(entry))
            );
    }

    public async Task SignIn()
    {
        var user = new UserRequest
        {
            Email = "test@test.com",
            Password = "testtest"
        };
        // sign in jest tutaj tylko tymczasowo
        await authService.SignIn(user);
    }
}