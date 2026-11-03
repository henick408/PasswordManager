using PasswordManager.Dto;

namespace PasswordManager.Service;

public interface IBiometricLoginService
{
    string DisplayName { get; }
    bool IsEnabled { get; }
    string? EnrolledEmail { get; }

    Task<bool> IsAvailableAsync();

    Task<bool> EnableAsync(UserRequest credentials);

    Task DisableAsync();
    
    Task<UserRequest?> UnlockCredentialsAsync();
}
