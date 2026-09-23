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
    
    private string? selectedCategory;
    private ObservableCollection<PasswordControlViewModel> passwords;
    private ObservableCollection<PasswordControlViewModel> passwordsFromDatabase;
    private PasswordControlViewModel? selectedPassword;
    
    public IList<string> Categories { get; } = new List<string>
    {
        "Social", "Work", "Finance", "Shopping", "Entertainment", "Other"
    };

    public string? SelectedCategory
    {
        get => selectedCategory;
        set
        {
            if (SetField(ref selectedCategory, value))
            {
                Passwords = GetFilteredPasswords();
            }
        }
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

    public ObservableCollection<PasswordControlViewModel> PasswordsFromDatabase
    {
        get => passwordsFromDatabase;
        set
        {
            if (SetField(ref passwordsFromDatabase, value))
            {
                Passwords = GetFilteredPasswords();
            }
        }
    }
    
    private ObservableCollection<PasswordControlViewModel> GetFilteredPasswords()
    {
        return SelectedCategory is null
            ? PasswordsFromDatabase
            : new ObservableCollection<PasswordControlViewModel>(
                PasswordsFromDatabase.Where(p => p.PasswordEntry.Category == SelectedCategory));
    }

    public MainViewModel(AuthService authService, PasswordService passwordService)
    {
        this.authService = authService;
        this.passwordService = passwordService;
    }

    public async Task ListPasswords()
    {
        IList<PasswordEntry> passwordsEntries = await passwordService.GetPasswords();
        PasswordsFromDatabase = new ObservableCollection<PasswordControlViewModel>(
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
        var dialog = new AddEditPasswordDialog(Categories) {Owner = Application.Current.MainWindow};
        if (dialog.ShowDialog() == true)
        {
            var createdPassword = await passwordService.CreatePassword(dialog.PasswordEntry);
            await ListPasswords();
            MessageBox.Show("Password added");
            SelectedPassword = new PasswordControlViewModel(createdPassword);
        }
    }

    public async Task UpdatePassword()
    {
        var dialog = new AddEditPasswordDialog(Categories, SelectedPassword!.PasswordEntry) {Owner = Application.Current.MainWindow};
        if (dialog.ShowDialog() == true)
        {
            var selected = SelectedPassword;
            await passwordService.UpdatePassword(dialog.PasswordEntry);
            await ListPasswords();
            MessageBox.Show("Password updated");
            SelectedPassword = selected;
        }
    }

    public async Task DeletePassword()
    {
        var messageBoxResult = MessageBox.Show($"Delete {SelectedPassword!.PasswordEntry.Name}?\nThis action cannot be undone.", "Delete password", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (messageBoxResult == MessageBoxResult.Yes)
        {
            await passwordService.DeletePassword(SelectedPassword.PasswordEntry);
            await ListPasswords();
            MessageBox.Show("Password deleted");
            SelectedPassword = null;
        }
    }

    public void ClearSelectedCategory()
    {
        this.SelectedCategory = null;
    }
    
}