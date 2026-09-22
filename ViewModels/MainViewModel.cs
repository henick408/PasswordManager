using System.Collections.ObjectModel;
using System.Windows;
using PasswordManager.Dto;
using PasswordManager.Service;
using PasswordManager.Views.Windows;

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
        MessageBox.Show("Signed in");
    }

    public async Task CreatePassword()
    {
        var dialog = new AddEditPasswordDialog(Categories);
        if (dialog.ShowDialog() == true)
        {
            await passwordService.CreatePassword(dialog.Password);
            MessageBox.Show($"Dodano hasło o Id={dialog.Password.Id}");
            await ListPasswords();
        }
    }

    public async Task UpdatePassword()
    {
        var dialog = new AddEditPasswordDialog(Categories, SelectedPassword!.PasswordEntry);
        if (dialog.ShowDialog() == true)
        {
            await passwordService.UpdatePassword(dialog.Password);
            MessageBox.Show($"Edytowano hasło o Name={dialog.Password.Id}");
            await ListPasswords();
        }
    }
}