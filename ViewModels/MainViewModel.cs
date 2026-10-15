using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia.Enums;
using PasswordManager.Dto;
using PasswordManager.Service;

namespace PasswordManager.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public readonly IAuthService authService;
    private readonly IPasswordService passwordService;
    private readonly IDialogService dialogService;
    
    private string? selectedCategory;
    private ObservableCollection<PasswordControlViewModel> passwords;
    private ObservableCollection<PasswordControlViewModel> passwordsFromDatabase;
    private PasswordControlViewModel? selectedPassword;
    private string passwordSearch;
    
    // Mock data for AXAML preview
    public MainViewModel() { }
    public MainViewModel(IAuthService authService, IPasswordService passwordService, IDialogService dialogService)
    {
        this.authService = authService;
        this.passwordService = passwordService;
        this.dialogService = dialogService;
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

    [ObservableProperty]
    public partial bool IsPasswordRevealed { get; set; }

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

    private ObservableCollection<PasswordControlViewModel> PasswordsFromDatabase
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
    

    [RelayCommand]
    private async Task CreatePassword()
    {
        var dialogPassword = await dialogService.ShowAddEditPassword(Categories);
        if (dialogPassword is not null)
        {
            var createdPassword = await passwordService.CreatePassword(dialogPassword);
            await ListPasswords();
            //MessageBox.Show("Password added");
            SelectedPassword = new PasswordControlViewModel(createdPassword);
        }
    }
    
    [RelayCommand]
    private async Task UpdatePassword()
    {
        //var dialog = new AddEditPasswordDialog(Categories, SelectedPassword!.PasswordEntry) {Owner = Application.Current.MainWindow};
        var dialogPassword = await dialogService.ShowAddEditPassword(Categories, new PasswordEntry(SelectedPassword!.PasswordEntry));
        if (dialogPassword is not null)
        {
            //var selected = SelectedPassword;
            var updatedPassword = await passwordService.UpdatePassword(dialogPassword);
            await ListPasswords();
            //MessageBox.Show("Password updated");
            SelectedPassword = new PasswordControlViewModel(updatedPassword);
        }
    }
    
    [RelayCommand]
    private async Task DeletePassword()
    {
        //var messageBoxResult = MessageBox.Show($"Delete {SelectedPassword!.PasswordEntry.Name}?\nThis action cannot be undone.", "Delete password", MessageBoxButton.YesNo, MessageBoxImage.Question);
        var messageBoxResult = await dialogService.Confirm($"Delete {SelectedPassword!.PasswordEntry.Name}?\nThis action cannot be undone.", "Delete password", Icon.Question);
        if (messageBoxResult)
        {
            await passwordService.DeletePassword(SelectedPassword.PasswordEntry);
            await ListPasswords();
            //MessageBox.Show("Password deleted");
            SelectedPassword = null;
        }
    }

    public void ClearSelectedCategory()
    { 
        SelectedCategory = null;
    }

    public Task LogOut()
    {
        return authService.SignOut();
    }
    
}