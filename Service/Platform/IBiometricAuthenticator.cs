namespace PasswordManager.Service.Platform;

public interface IBiometricAuthenticator
{
    string DisplayName { get; }

    Task<bool> IsAvailableAsync();
    
    Task<byte[]?> ProtectAsync(byte[] plaintext);
    
    Task<byte[]?> UnprotectAsync(byte[] protectedData);

    Task RemoveAsync();
}
