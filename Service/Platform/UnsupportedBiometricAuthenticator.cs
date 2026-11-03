namespace PasswordManager.Service.Platform;

public class UnsupportedBiometricAuthenticator : IBiometricAuthenticator
{
    public string DisplayName => "Biometric sign-in";

    public Task<bool> IsAvailableAsync() => Task.FromResult(false);

    public Task<byte[]?> ProtectAsync(byte[] plaintext) => throw new PlatformNotSupportedException();

    public Task<byte[]?> UnprotectAsync(byte[] protectedData) => throw new PlatformNotSupportedException();

    public Task RemoveAsync() => Task.CompletedTask;
}
