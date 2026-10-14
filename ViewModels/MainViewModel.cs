using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PasswordManager.Dto;
using PasswordManager.Service;

namespace PasswordManager.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public readonly IAuthService authService;
    private readonly IPasswordService passwordService;
    
    private string? selectedCategory;
    private ObservableCollection<PasswordControlViewModel> passwords;
    private ObservableCollection<PasswordControlViewModel> passwordsFromDatabase;
    private PasswordControlViewModel? selectedPassword;
    private string passwordSearch;
    
    // Mock data for AXAML preview
    public MainViewModel() { }
    public MainViewModel(IAuthService authService, IPasswordService passwordService)
    {
        this.authService = authService;
        this.passwordService = passwordService;
    }
    
    public IList<string> Categories { get; } = new List<string>
    {
        "Social", "Work", "Finance", "Shopping", "Entertainment", "Other"
    };

    public string PasswordSearch
    {
        get => passwordSearch;
        set
        {
            if (SetProperty(ref passwordSearch, value))
            {
                Passwords = GetFilteredPasswords();
            }
        }
    }

    public string? SelectedCategory
    {
        get => selectedCategory;
        set
        {
            if (SetProperty(ref selectedCategory, value))
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
            if (SetProperty(ref selectedPassword, value))
            {
                OnPropertyChanged(nameof(IsPasswordSelected));
            }
        }
    }

    public bool IsPasswordSelected => SelectedPassword != null;

    public ObservableCollection<PasswordControlViewModel> Passwords
    {
        get => passwords;
        set => SetProperty(ref passwords, value);
    }

    public ObservableCollection<PasswordControlViewModel> PasswordsFromDatabase
    {
        get => passwordsFromDatabase;
        set
        {
            if (SetProperty(ref passwordsFromDatabase, value))
            {
                Passwords = GetFilteredPasswords();
            }
        }
    }
    
    private ObservableCollection<PasswordControlViewModel> GetFilteredPasswords()
    {
        IList<PasswordControlViewModel> filtered = PasswordsFromDatabase;

        if (SelectedCategory is not null)
        {
            filtered = filtered.Where(password => password.PasswordEntry.Category == SelectedCategory).ToList();
        }

        if (!string.IsNullOrEmpty(PasswordSearch))
        {
            filtered = filtered.Where(password => password.PasswordEntry.Name.Contains(PasswordSearch, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return new ObservableCollection<PasswordControlViewModel>(filtered);
    }

    public async Task ListPasswords()
    {
        IList<PasswordEntry> passwordsEntries = await passwordService.GetPasswords();
        PasswordsFromDatabase = new ObservableCollection<PasswordControlViewModel>(
            passwordsEntries.Select(entry => new PasswordControlViewModel(entry))
            );
    }
    

    // public async Task CreatePassword()
    // {
    //     var dialog = new AddEditPasswordDialog(Categories) {Owner = Application.Current.MainWindow};
    //     if (dialog.ShowDialog() == true)
    //     {
    //         var createdPassword = await passwordService.CreatePassword(dialog.PasswordEntry);
    //         await ListPasswords();
    //         MessageBox.Show("Password added");
    //         SelectedPassword = new PasswordControlViewModel(createdPassword);
    //     }
    // }
    //
    // public async Task UpdatePassword()
    // {
    //     var dialog = new AddEditPasswordDialog(Categories, SelectedPassword!.PasswordEntry) {Owner = Application.Current.MainWindow};
    //     if (dialog.ShowDialog() == true)
    //     {
    //         var selected = SelectedPassword;
    //         await passwordService.UpdatePassword(dialog.PasswordEntry);
    //         await ListPasswords();
    //         MessageBox.Show("Password updated");
    //         SelectedPassword = selected;
    //     }
    // }
    //
    // public async Task DeletePassword()
    // {
    //     var messageBoxResult = MessageBox.Show($"Delete {SelectedPassword!.PasswordEntry.Name}?\nThis action cannot be undone.", "Delete password", MessageBoxButton.YesNo, MessageBoxImage.Question);
    //     if (messageBoxResult == MessageBoxResult.Yes)
    //     {
    //         await passwordService.DeletePassword(SelectedPassword.PasswordEntry);
    //         await ListPasswords();
    //         MessageBox.Show("Password deleted");
    //         SelectedPassword = null;
    //     }
    // }

    public void ClearSelectedCategory()
    { 
        SelectedCategory = null;
    }

    public Task LogOut()
    {
        return authService.SignOut();
    }
    
}