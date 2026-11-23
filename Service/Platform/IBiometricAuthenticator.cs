namespace PasswordManager.Service.Platform;

public interface IBiometricAuthenticator
{
    string DisplayName { get; }

    Task<bool> IsAvailableAsync();

    /// <returns>Protected blob, or <c>null</c> when the user cancelled the prompt.</returns>
    Task<byte[]?> ProtectAsync(byte[] plaintext, byte[] associatedData);

    /// <returns>Plaintext, or <c>null</c> when the user cancelled the prompt.</returns>
    Task<byte[]?> UnprotectAsync(byte[] protectedData, byte[] associatedData);

    Task RemoveAsync();
}
