using PasswordManager.CustomExceptions;

namespace PasswordManager.Service.Platform;

public class UnsupportedBiometricAuthenticator : IBiometricAuthenticator
{
    public string DisplayName => "Biometric sign-in";

    public Task<bool> IsAvailableAsync() => Task.FromResult(false);

    public Task<byte[]?> ProtectAsync(byte[] plaintext, byte[] associatedData) =>
        throw new BiometricException("Biometric sign-in is not supported on this platform.");

    public Task<byte[]?> UnprotectAsync(byte[] protectedData, byte[] associatedData) =>
        throw new BiometricException("Biometric sign-in is not supported on this platform.");

    public Task RemoveAsync() => Task.CompletedTask;
}
