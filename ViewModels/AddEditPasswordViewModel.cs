using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordGenerator;
using PasswordManager.Dto;
using PasswordManager.Utils;

namespace PasswordManager.ViewModels;

public partial class AddEditPasswordViewModel : ViewModelBase
{
    
    [ObservableProperty]
    public partial PasswordEntry PasswordEntry { get; private set; }
    
    [ObservableProperty]
    public partial IList<string> Categories { get; private set; }

    [ObservableProperty] 
    public partial bool IsGeneratorExpanded { get; set; }

    [ObservableProperty]
    public partial double? PasswordGeneratorLength { get; set; } = 16;

    [ObservableProperty]
    public partial bool IsPasswordRevealed { get; set; }

    [ObservableProperty]
    public partial string GeneratedPassword { get; set; } = "";

    [ObservableProperty]
    public partial bool IsLowercase { get; set; } = true;
    
    [ObservableProperty]
    public partial bool IsUppercase { get; set; } = true;
    
    [ObservableProperty]
    public partial bool IsNumerics { get; set; } = true;
    
    [ObservableProperty]
    public partial bool IsSpecialSigns { get; set; } = true;
    
    public bool IsLowercaseEnabled => !IsLowercase || GetCheckedCount() > 1;
    public bool IsUppercaseEnabled => !IsUppercase || GetCheckedCount() > 1;
    public bool IsNumericsEnabled => !IsNumerics || GetCheckedCount() > 1;
    public bool IsSpecialSignsEnabled => !IsSpecialSigns || GetCheckedCount() > 1;
    
    public AddEditPasswordViewModel()
    {
        IsGeneratorExpanded = true;
        Categories = new List<string> { "Category1", "Category2", "Category3" };
        
        PasswordEntry = new PasswordEntry
        {
            Name = "Name",
            Url = "Url",
            Username = "Username",
            Password = "Password",
            Category = "Category",
            Notes = "Notes"
        };
    }
    
    public AddEditPasswordViewModel(IList<string> categories, PasswordEntry? passwordEntry = null)
    {
        PasswordEntry = passwordEntry ?? new PasswordEntry();
        Categories = categories;
    }
    
    partial void OnIsLowercaseChanged(bool value) => OnSelectionChanged();
    partial void OnIsUppercaseChanged(bool value) => OnSelectionChanged();
    partial void OnIsNumericsChanged(bool value) => OnSelectionChanged();
    partial void OnIsSpecialSignsChanged(bool value) => OnSelectionChanged();

    partial void OnPasswordGeneratorLengthChanged(double? value)
    {
        if (value is null || value < 8)
        {
            PasswordGeneratorLength = 8;
            return;
        }

        if (value > 64) PasswordGeneratorLength = 64;
    }
    
    private void OnSelectionChanged()
    {
        OnPropertyChanged(nameof(IsLowercaseEnabled));
        OnPropertyChanged(nameof(IsUppercaseEnabled));
        OnPropertyChanged(nameof(IsNumericsEnabled));
        OnPropertyChanged(nameof(IsSpecialSignsEnabled));
    }
    
    private int GetCheckedCount()
    {
        return (IsLowercase ? 1 : 0)
               + (IsUppercase ? 1 : 0)
               + (IsNumerics ? 1 : 0)
               + (IsSpecialSigns ? 1 : 0);
    }

    [RelayCommand]
    private async Task CopyGeneratedPasswordToClipboard()
    {
        IClipboard? clipboard = ClipboardHelper.GetClipboard();
        if (clipboard is not null)
        {
            await clipboard.SetTextAsync(GeneratedPassword);
        }
    }

    [RelayCommand]
    private void GeneratePassword()
    {
        using var generator = new Password(IsLowercase, IsUppercase, IsNumerics, IsSpecialSigns, (int)PasswordGeneratorLength!);
        GeneratedPassword = generator.Next();
    }
    
    [RelayCommand]
    private void FillRandomPassword()
    {
        PasswordEntry = new PasswordEntry(PasswordEntry) { Password = GeneratedPassword };
    }
    
}