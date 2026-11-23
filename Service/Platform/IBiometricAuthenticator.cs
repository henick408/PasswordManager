namespace PasswordManager.Service.Platform;

public interface IBiometricAuthenticator
{
    string DisplayName { get; }

    Task<bool> IsAvailableAsync();

    /// <param name="confirmWeakProtection">
    /// Called with a warning when the key cannot be confirmed to be hardware (TPM) protected.
    /// Return <c>true</c> to continue anyway, <c>false</c> to abort.
    /// </param>
    /// <returns>Protected blob, or <c>null</c> when the user cancelled.</returns>
    Task<byte[]?> ProtectAsync(byte[] plaintext, byte[] associatedData, Func<string, Task<bool>> confirmWeakProtection);

    /// <returns>Plaintext, or <c>null</c> when the user cancelled the prompt.</returns>
    Task<byte[]?> UnprotectAsync(byte[] protectedData, byte[] associatedData);

    Task RemoveAsync();
}
