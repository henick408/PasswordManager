using PasswordManager.Dto;

namespace PasswordManager.Service;

public interface IBiometricLoginService
{
    string DisplayName { get; }
    bool IsEnabled { get; }

    bool IsEnabledFor(string? email);

    Task<bool> IsAvailableAsync();

    Task<bool> EnableAsync(UserRequest credentials);

    Task DisableAsync();
    
    Task<UserRequest?> UnlockCredentialsAsync();
}
