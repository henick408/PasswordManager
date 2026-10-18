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

    public event Action? SignedOut;
    
    
    // Mock data for AXAML preview
    public MainViewModel()
    {
        Passwords = new ObservableCollection<PasswordControlViewModel>
        {
            new(new PasswordEntry
            {
                Name = "Name1",
                Url = "Url1",
                Username = "Username1",
                Password = "Password1",
                Category = Categories[0],
                Notes = "Notes1\nNotes1"
            }),
            new(new PasswordEntry
            {
                Name = "Name2",
                Url = "Url2",
                Username = "Username2",
                Password = "Password2",
                Category = Categories[1],
                Notes = "Notes2\nNotes2"
            }),
            new(new PasswordEntry
            {
                Name = "Name3",
                Url = "Url3",
                Username = "Username3",
                Password = "Password3",
                Category = Categories[2],
                Notes = "Notes3\nNotes3"
            })
        };
    }
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
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                Passwords = GetFilteredPasswords();
            }
        }
    }
    

    public string? SelectedCategory
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                Passwords = GetFilteredPasswords();
            }
        }
    }

    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(IsPasswordSelected))]
    public partial PasswordControlViewModel? SelectedPassword { get; set; }

    public bool IsPasswordSelected => SelectedPassword != null;
    
    [ObservableProperty]
    public partial ObservableCollection<PasswordControlViewModel> Passwords { get; set; }
    
    [ObservableProperty]
    private partial ObservableCollection<PasswordControlViewModel> PasswordsFromDatabase { get; set; }

    partial void OnPasswordsFromDatabaseChanged(ObservableCollection<PasswordControlViewModel> value)
    {
        Passwords = GetFilteredPasswords();
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
            SelectedPassword = new PasswordControlViewModel(createdPassword);
        }
    }
    
    [RelayCommand]
    private async Task UpdatePassword()
    {
        var dialogPassword = await dialogService.ShowAddEditPassword(Categories, new PasswordEntry(SelectedPassword!.PasswordEntry));
        if (dialogPassword is not null)
        {
            var updatedPassword = await passwordService.UpdatePassword(dialogPassword);
            await ListPasswords();
            SelectedPassword = new PasswordControlViewModel(updatedPassword);
        }
    }
    
    [RelayCommand]
    private async Task DeletePassword()
    {
        var messageBoxResult = await dialogService.Confirm($"Delete {SelectedPassword!.PasswordEntry.Name}?\nThis action cannot be undone.", "Delete password", Icon.Question);
        if (messageBoxResult)
        {
            await passwordService.DeletePassword(SelectedPassword.PasswordEntry);
            await ListPasswords();
            SelectedPassword = null;
        }
    }

    [RelayCommand]
    private  async Task OnWindowOpened()
    {
        await ListPasswords();
    }

    [RelayCommand]
    private void ClearSelectedCategory()
    { 
        SelectedCategory = null;
    }

    [RelayCommand]
    private void SignOut()
    {
        authService.SignOut();
        SignedOut?.Invoke();
    }
    
}