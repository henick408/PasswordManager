using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.CustomExceptions;
using PasswordManager.Dto;
using PasswordManager.Service;
using static Supabase.Gotrue.Exceptions.FailureHint;

namespace PasswordManager.ViewModels;

public partial class LoginRegisterViewModel : ViewModelBase
{
    private readonly IAuthService authService;
    
    public event Action? SignInSuccess;
    public event Action? RegisterSwap;

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

    partial void OnEmailChanged(string value)
    {
        EmailError = string.Empty;
        if (IsAuthError)
        {
            AuthError = string.Empty;
            AuthErrorDetails = string.Empty;
        }
    }
    
    [ObservableProperty]
    public partial string MasterPassword { get; set; } = string.Empty;

    partial void OnMasterPasswordChanged(string value)
    {
        MasterPasswordError = string.Empty;
    }
    
    [ObservableProperty]
    public partial string RepeatMasterPassword { get; set; } = string.Empty;
    
    partial void OnRepeatMasterPasswordChanged(string value)
    {
        RepeatMasterPasswordError = string.Empty;
    }
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmailError))]
    public partial string EmailError { get; set; } = string.Empty;

    public bool IsEmailError => !string.IsNullOrWhiteSpace(EmailError);
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMasterPasswordError))]
    public partial string MasterPasswordError { get; set; } = string.Empty;

    public bool IsMasterPasswordError => !string.IsNullOrWhiteSpace(MasterPasswordError);
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRepeatMasterPasswordError))]
    public partial string RepeatMasterPasswordError { get; set; } = string.Empty;

    public bool IsRepeatMasterPasswordError => !string.IsNullOrWhiteSpace(RepeatMasterPasswordError);

    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(IsAuthError))]
    public partial string AuthError { get; set; } = string.Empty;

    public bool IsAuthError => !string.IsNullOrWhiteSpace(AuthError);

    [ObservableProperty] 
    public partial string AuthErrorDetails { get; set; } = string.Empty;

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
        RegisterSwap?.Invoke();
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
        catch (SupabaseAuthException)
        {
            AuthError = "Unable to sign in";
            AuthErrorDetails = "The email or master password is incorrect";
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
        catch (SupabaseAuthException exception) when (exception.Reason == Reason.UserAlreadyRegistered)
        {
            AuthError = "Account already exists";
            AuthErrorDetails = "An account with this email or username already exists.";
            return false;
        }
        catch (SupabaseAuthException)
        {
            AuthError = "Error occured";
            AuthErrorDetails = string.Empty;
            return false;
        }

    }

    private void ClearCredentials()
    {
        Email = string.Empty;
        MasterPassword = string.Empty;
        RepeatMasterPassword = string.Empty;
        EmailError = string.Empty;
        MasterPasswordError = string.Empty;
        RepeatMasterPasswordError = string.Empty;
        AuthError = string.Empty;
        AuthErrorDetails = string.Empty;
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
        if (!IsRegister)
        {
            EmailError = string.Empty;
            return true;
        }
        
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
        if (!IsRegister)
        {
            MasterPasswordError = string.Empty;
            return true;
        }

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
            return false;
        }

        if (!MasterPassword.Equals(RepeatMasterPassword))
        {
            RepeatMasterPasswordError = "Passwords do not match";
            return false;
        }
        
        RepeatMasterPasswordError = string.Empty;
        return true;
    }
    
}