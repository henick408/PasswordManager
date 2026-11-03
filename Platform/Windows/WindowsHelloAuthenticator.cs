using System.Security.Cryptography;
using System.Text;
using Avalonia.Controls;
using PasswordManager.CustomExceptions;
using PasswordManager.Service.Platform;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;

namespace PasswordManager.Platform.Windows;

public class WindowsHelloAuthenticator(ITopLevelProvider topLevelProvider) : IBiometricAuthenticator
{
    private const string CredentialName = "PasswordManager";
    private const int KeySize = 32;
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private static readonly byte[] Challenge = Encoding.UTF8.GetBytes("PasswordManager.WindowsHello.v1");
    private static readonly byte[] KeyInfo = Encoding.UTF8.GetBytes("PasswordManager vault key");

    public string DisplayName => "Windows Hello";

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            return await KeyCredentialManager.IsSupportedAsync();
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<byte[]?> ProtectAsync(byte[] plaintext)
    {
        BringWindowToFront();
        KeyCredentialRetrievalResult result =
            await KeyCredentialManager.RequestCreateAsync(CredentialName, KeyCredentialCreationOption.ReplaceExisting);
        if (IsCancelled(result.Status)) return null;
        EnsureSuccess(result.Status);

        byte[]? key = await DeriveKeyAsync(result.Credential);
        if (key is null) return null;

        try
        {
            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
            byte[] tag = new byte[TagSize];
            byte[] cipherText = new byte[plaintext.Length];

            using (AesGcm aes = new(key, TagSize))
            {
                aes.Encrypt(nonce, plaintext, cipherText, tag);
            }

            return [..nonce, ..tag, ..cipherText];
        }
        finally
        {
            CryptographicOperations.ZeroMemory(key);
        }
    }

    public async Task<byte[]?> UnprotectAsync(byte[] protectedData)
    {
        if (protectedData.Length < NonceSize + TagSize)
        {
            throw new BiometricKeyInvalidatedException();
        }

        BringWindowToFront();
        KeyCredentialRetrievalResult result = await KeyCredentialManager.OpenAsync(CredentialName);
        if (result.Status == KeyCredentialStatus.NotFound) throw new BiometricKeyInvalidatedException();
        if (IsCancelled(result.Status)) return null;
        EnsureSuccess(result.Status);

        byte[]? key = await DeriveKeyAsync(result.Credential);
        if (key is null) return null;

        try
        {
            ReadOnlySpan<byte> nonce = protectedData.AsSpan(0, NonceSize);
            ReadOnlySpan<byte> tag = protectedData.AsSpan(NonceSize, TagSize);
            ReadOnlySpan<byte> cipherText = protectedData.AsSpan(NonceSize + TagSize);
            byte[] plaintext = new byte[cipherText.Length];

            using (AesGcm aes = new(key, TagSize))
            {
                aes.Decrypt(nonce, cipherText, tag, plaintext);
            }

            return plaintext;
        }
        catch (AuthenticationTagMismatchException exception)
        {
            // A different Hello key than the one used for encryption (e.g. Hello was reset).
            throw new BiometricKeyInvalidatedException(exception);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(key);
        }
    }

    public async Task RemoveAsync()
    {
        try
        {
            await KeyCredentialManager.DeleteAsync(CredentialName);
        }
        catch (Exception)
        {
            // Key does not exist, nothing to remove.
        }
    }

    private static async Task<byte[]?> DeriveKeyAsync(KeyCredential credential)
    {
        KeyCredentialOperationResult signResult =
            await credential.RequestSignAsync(CryptographicBuffer.CreateFromByteArray(Challenge));
        if (IsCancelled(signResult.Status)) return null;
        EnsureSuccess(signResult.Status);

        CryptographicBuffer.CopyToByteArray(signResult.Result, out byte[] signature);
        try
        {
            return HKDF.DeriveKey(HashAlgorithmName.SHA256, signature, KeySize, info: KeyInfo);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(signature);
        }
    }

    private void BringWindowToFront()
    {
        if (topLevelProvider.Current is Window window)
        {
            window.Activate();
        }
    }

    private static bool IsCancelled(KeyCredentialStatus status) =>
        status is KeyCredentialStatus.UserCanceled or KeyCredentialStatus.UserPrefersPassword;

    private static void EnsureSuccess(KeyCredentialStatus status)
    {
        switch (status)
        {
            case KeyCredentialStatus.Success:
                return;
            case KeyCredentialStatus.NotFound:
                throw new BiometricKeyInvalidatedException();
            case KeyCredentialStatus.SecurityDeviceLocked:
                throw new BiometricException("Windows Hello is locked. Try again later.");
            default:
                throw new BiometricException($"Windows Hello failed ({status}).");
        }
    }
}
