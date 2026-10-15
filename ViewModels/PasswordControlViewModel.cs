using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Dto;
using PasswordManager.Utils;

namespace PasswordManager.ViewModels;

public partial class PasswordControlViewModel : ViewModelBase
{

    [ObservableProperty]
    public partial PasswordEntry PasswordEntry { get; set; }

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

    [RelayCommand]
    private async Task CopyUsername()
    {
        IClipboard? clipboard = ClipboardHelper.GetClipboard();
        if (clipboard is not null)
        {
            await clipboard.SetTextAsync(PasswordEntry.Username);
        }
    }
    
    [RelayCommand]
    private async Task CopyPassword()
    {
        IClipboard? clipboard = ClipboardHelper.GetClipboard();
        if (clipboard is not null)
        {
            await clipboard.SetTextAsync(PasswordEntry.Password);
        }
    }
    
    [RelayCommand]
    private async Task CopyUrl()
    {
        IClipboard? clipboard = ClipboardHelper.GetClipboard();
        if (clipboard is not null)
        {
            await clipboard.SetTextAsync(PasswordEntry.Url);
        }
    }
    
}