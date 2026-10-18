using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Dto;
using PasswordManager.Service;

namespace PasswordManager.ViewModels;

public partial class LoginRegisterViewModel : ViewModelBase
{
    private readonly IAuthService authService;
    
    public event Action? SignInSuccess;

    private static string emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HeaderText))]
    [NotifyPropertyChangedFor(nameof(ButtonText))]
    [NotifyPropertyChangedFor(nameof(FooterText))]
    [NotifyPropertyChangedFor(nameof(HyperlinkText))]
    public partial bool IsRegister { get; set; }

    public string HeaderText    => IsRegister ? "Create your account" : "Sign in";
    public string ButtonText    => IsRegister ? "Create account" : "Sign in";
    public string FooterText    => IsRegister ? "Already have an account?" : "Don't have an account?";
    public string HyperlinkText => IsRegister ? "Sign in" : "Create one";

    [ObservableProperty] 
    public partial string Email { get; set; } = string.Empty;
    
    [ObservableProperty]
    public partial string MasterPassword { get; set; } = string.Empty;
    
    [ObservableProperty]
    public partial string RepeatMasterPassword { get; set; } = string.Empty;
    
    [ObservableProperty]
    public partial string EmailError { get; set; } = string.Empty;
    
    [ObservableProperty]
    public partial string MasterPasswordError { get; set; } = string.Empty;
    
    [ObservableProperty]
    public partial string RepeatMasterPasswordError { get; set; } = string.Empty;

    public LoginRegisterViewModel() { }
    
    public LoginRegisterViewModel(IAuthService authService)
    {
        this.authService = authService;
    }

    [RelayCommand]
    private void SwapIsRegister()
    {
        ClearCredentials();
        IsRegister = !IsRegister;
    }

    [RelayCommand]
    private async Task LoginAttempt()
    {
        if (IsRegister)
        {
            if (await SignUp()) AfterSuccessfulRegister();
            return;
        }

        if(!await SignIn()) return;
        SignInSuccess?.Invoke();
    }

    private async Task<bool> SignIn()
    {
        bool isValidMail = ValidateEmail();
        bool isValidPassword = ValidateMasterPassword();
        if (!isValidMail || !isValidPassword) return false;
        UserRequest request = new UserRequest(Email, MasterPassword);
        try
        {
            await authService.SignInAsync(request);
            return true;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            return false;
        }
    }

    private async Task<bool> SignUp()
    {
        bool isValidMail = ValidateEmail();
        bool isValidPassword = ValidateMasterPassword();
        bool isPasswordsMatch = ValidateRepeatMasterPassword();
        if (!isValidMail || !isValidPassword || !isPasswordsMatch) return false;
        UserRequest request = new UserRequest(Email, MasterPassword);
        try
        {
            await authService.SignUpAsync(request);
            return true;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            return false;
        }

    }

    private void ClearCredentials()
    {
        Email = string.Empty;
        MasterPassword = string.Empty;
        RepeatMasterPassword = string.Empty;
    }

    private void AfterSuccessfulRegister()
    {
        authService.SignOut();
        ClearCredentials();
        IsRegister = false;
    }

    private bool ValidateEmail()
    {
        string email = Email.Trim();
        if (string.IsNullOrEmpty(email))
        {
            EmailError = "Enter your email.";
            return false;
        }
        if (!IsRegister) return true;
        
        if (email.Length > 254)
        {
            EmailError = "Email is too long.";
            return false;
        }

        if (!Regex.IsMatch(email, emailRegex))
        {
            EmailError = "Enter a valid email address.";
            return false;
        }

        EmailError = string.Empty;
        return true;
    }

    private bool ValidateMasterPassword()
    {
        
        if (string.IsNullOrWhiteSpace(MasterPassword))
        {
            MasterPasswordError = "Enter a master password.";
            return false;
        }
        if (!IsRegister) return true;

        if (!IsProperMasterPassword(out string error))
        {
            MasterPasswordError = error;
            return false;
        }

        if (!MasterPassword.Equals(RepeatMasterPassword))
        {
            MasterPasswordError = "Passwords do not match.";
            return false;
        }

        MasterPasswordError = string.Empty;
        return true;
    }

    private bool IsProperMasterPassword(out string error)
    {
        error = string.Empty;
        string[] patterns = ["[a-z]", "[A-Z]", "[0-9]", "[^a-zA-Z0-9]"];
        if (!patterns.All(pattern => Regex.IsMatch(MasterPassword, pattern)))
        {
            error = "Include uppercase, lowercase, a number, and a symbol.";
            return false;
        }

        if (MasterPassword.Length < 12)
        {
            error = "Use at least 12 characters.";
            return false;
        }

        return true;
    }

    private bool ValidateRepeatMasterPassword()
    {
        if (string.IsNullOrWhiteSpace(RepeatMasterPassword))
        {
            RepeatMasterPasswordError = "Confirm your master password.";
        }

        if (!MasterPassword.Equals(RepeatMasterPassword))
        {
            Console.WriteLine($"{MasterPassword} {RepeatMasterPassword}");
            RepeatMasterPasswordError = "Passwords do not match";
            return false;
        }
        
        RepeatMasterPasswordError = string.Empty;
        return true;
    }
    
}