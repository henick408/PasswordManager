using PasswordManager.Dto;

namespace PasswordManager.Service;

public interface IBiometricLoginService
{
    string DisplayName { get; }
    bool IsEnabled { get; }

    bool IsEnabledFor(string? email);

    Task<bool> IsAvailableAsync();

    /// <param name="confirmWeakProtection">Asks the user whether to continue when the key is not hardware protected.</param>
    Task<bool> EnableAsync(UserRequest credentials, Func<string, Task<bool>> confirmWeakProtection);

    Task DisableAsync();
    
    Task<UserRequest?> UnlockCredentialsAsync();
}
