using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordGenerator;
using PasswordManager.Dto;
using PasswordManager.Utils;

namespace PasswordManager.ViewModels;

public partial class AddEditPasswordViewModel : ViewModelBase
{
    /// <summary>Category assigned when the user does not choose one.</summary>
    public const string DefaultCategory = "Other";

    [ObservableProperty]
    public partial PasswordEntry PasswordEntry { get; private set; }

    /// <summary>
    /// Edited name. Kept here rather than bound to <see cref="PasswordEntry"/> directly, so the validation
    /// error can be cleared while typing; written back to the entry in <see cref="TryPrepareForSave"/>.
    /// </summary>
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    partial void OnNameChanged(string value)
    {
        NameError = string.Empty;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNameError))]
    public partial string NameError { get; set; } = string.Empty;

    public bool IsNameError => !string.IsNullOrWhiteSpace(NameError);
    
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
        Name = PasswordEntry.Name;
    }
    
    public AddEditPasswordViewModel(IList<string> categories, PasswordEntry? passwordEntry = null)
    {
        PasswordEntry = passwordEntry ?? new PasswordEntry();
        Name = PasswordEntry.Name;
        Categories = categories;
    }

    /// <summary>
    /// Validates the form and completes the entry before saving: the name is required (trimmed),
    /// a missing category becomes <see cref="DefaultCategory"/>.
    /// </summary>
    /// <returns><c>true</c> when the entry can be saved; otherwise the errors are shown in the form.</returns>
    public bool TryPrepareForSave()
    {
        string name = Name.Trim();
        if (name.Length == 0)
        {
            NameError = "Enter a name.";
            return false;
        }

        PasswordEntry.Name = name;
        if (string.IsNullOrWhiteSpace(PasswordEntry.Category))
        {
            PasswordEntry.Category = DefaultCategory;
        }
        return true;
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

    /// <summary>Copies the generated password; it is removed from the clipboard after 30 seconds.</summary>
    [RelayCommand]
    private async Task CopyGeneratedPasswordToClipboard()
    {
        await ClipboardHelper.SetSensitiveTextAsync(GeneratedPassword);
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