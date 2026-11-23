using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PasswordManager.ViewModels;

public partial class MasterPasswordDialogViewModel : ViewModelBase
{
    /// <summary>Raised with the entered password, or <c>null</c> when the dialog was cancelled.</summary>
    public event Action<string?>? CloseRequested;

    public string Title { get; }
    public string Message { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    public partial string Password { get; set; } = string.Empty;

    // Design-time constructor
    public MasterPasswordDialogViewModel() : this("Confirm master password", "Enter your master password to continue.") { }

    public MasterPasswordDialogViewModel(string title, string message)
    {
        Title = title;
        Message = message;
    }

    private bool CanConfirm() => !string.IsNullOrEmpty(Password);

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private void Confirm()
    {
        string password = Password;
        Password = string.Empty;
        CloseRequested?.Invoke(password);
    }

    [RelayCommand]
    private void Cancel()
    {
        Password = string.Empty;
        CloseRequested?.Invoke(null);
    }
}
