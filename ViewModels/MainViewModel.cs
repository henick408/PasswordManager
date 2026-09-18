using System.Collections.ObjectModel;
using System.Reactive.Linq;
using PasswordManager.Dto;

namespace PasswordManager.ViewModels;

public class MainViewModel : ViewModelBase
{
    private string selectedCategory;
    private ObservableCollection<PasswordControlViewModel> passwords;
    private PasswordControlViewModel selectedPassword;
    
    public IList<string> Categories { get; } = new List<string>
    {
        "Social", "Work", "Finance", "Shopping", "Entertainment", "Other"
    };

    public string SelectedCategory
    {
        get => selectedCategory;
        set => SetField(ref selectedCategory, value);
    }

    public PasswordControlViewModel SelectedPassword
    {
        get => selectedPassword;
        set => SetField(ref selectedPassword, value);
    }

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

    public MainViewModel()
    {
        Passwords = new ObservableCollection<PasswordControlViewModel>(entries.Select(pass => new PasswordControlViewModel(pass))
            .ToList());
    }
}