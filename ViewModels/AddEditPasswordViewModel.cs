using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Dto;

namespace PasswordManager.ViewModels;

public partial class AddEditPasswordViewModel : ObservableObject
{
    
    [ObservableProperty]
    public partial PasswordEntry Password { get; private set; }
    
    [ObservableProperty]
    public partial IList<string> Categories { get; private set; }

    [ObservableProperty] 
    public partial bool IsGeneratorExpanded { get; set; }

    [ObservableProperty]
    public partial int PasswordGeneratorLength { get; set; } = 16;

    [ObservableProperty]
    public partial bool IsPasswordRevealed { get; set; }

    [ObservableProperty]
    public partial string GeneratedPassword { get; set; } = "";

    [ObservableProperty]
    public partial bool IsLowercase { get; set; } = true;
    
    [ObservableProperty]
    public partial bool IsUppercase { get; set; }
    
    [ObservableProperty]
    public partial bool IsNumerics { get; set; }
    
    [ObservableProperty]
    public partial bool IsSpecialSigns { get; set; }
    
    public bool IsLowercaseEnabled => !IsLowercase || GetCheckedCount() > 1;
    public bool IsUppercaseEnabled => !IsUppercase || GetCheckedCount() > 1;
    public bool IsNumericsEnabled => !IsNumerics || GetCheckedCount() > 1;
    public bool IsSpecialSignsEnabled => !IsSpecialSigns || GetCheckedCount() > 1;
    
    partial void OnIsLowercaseChanged(bool value) => OnSelectionChanged();
    partial void OnIsUppercaseChanged(bool value) => OnSelectionChanged();
    partial void OnIsNumericsChanged(bool value) => OnSelectionChanged();
    partial void OnIsSpecialSignsChanged(bool value) => OnSelectionChanged();
    
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

    public AddEditPasswordViewModel()
    {
        IsGeneratorExpanded = true;
        Categories = new List<string> { "Category1", "Category2", "Category3" };
        
        Password = new PasswordEntry
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
        Password = passwordEntry ?? new PasswordEntry();
        Categories = categories;
    }

    [RelayCommand]
    public void CopyGeneratedPasswordToClipboard()
    {
        throw new NotImplementedException();
    }

    [RelayCommand]
    public void GeneratePassword()
    {
        throw new NotImplementedException();
    }
    
    [RelayCommand]
    public void FillRandomPassword()
    {
        throw new NotImplementedException();
    }
    
}