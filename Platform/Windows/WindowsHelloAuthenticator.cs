using System.Security.Cryptography;
using System.Text;
using Avalonia.Controls;
using PasswordManager.CustomExceptions;
using PasswordManager.Service.Platform;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;

namespace PasswordManager.Platform.Windows;

/// <summary>
/// Protects data with a key derived from a Windows Hello (TPM-backed) RSA signature of a fixed challenge.
/// Hello signatures are deterministic (RSA PKCS#1 v1.5), so the same key is derived on every unlock.
/// Blob layout: salt(32) | nonce(12) | tag(16) | ciphertext.
/// </summary>
public class WindowsHelloAuthenticator(ITopLevelProvider topLevelProvider) : IBiometricAuthenticator
{
    // Unique name, so the Hello key cannot collide with credentials of other unpackaged apps.
    private const string CredentialName = "PasswordManager.WindowsHello.7f3c2a9e-4b1d-4e8a-9c6f-2d5b8e1a0c47";
    private const int KeySize = 32;
    private const int SaltSize = 32;
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const int HeaderSize = SaltSize + NonceSize + TagSize;
    private static readonly byte[] Challenge = Encoding.UTF8.GetBytes("PasswordManager.WindowsHello.v2");
    private static readonly byte[] KeyInfo = Encoding.UTF8.GetBytes("PasswordManager biometric key v2");

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

    public async Task<byte[]?> ProtectAsync(byte[] plaintext, byte[] associatedData,
        Func<string, Task<bool>> confirmWeakProtection)
    {
        BringWindowToFront();
        KeyCredentialRetrievalResult result = await CallHelloAsync(() =>
            KeyCredentialManager.RequestCreateAsync(CredentialName, KeyCredentialCreationOption.ReplaceExisting)
                .AsTask());
        if (IsCancelled(result.Status)) return null;
        EnsureSuccess(result.Status);
        if (!await ConfirmHardwareBackedAsync(result.Credential, confirmWeakProtection))
        {
            await RemoveAsync();
            return null;
        }

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[]? key = await DeriveKeyAsync(result.Credential, salt);
        if (key is null) return null;

        try
        {
            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
            byte[] tag = new byte[TagSize];
            byte[] cipherText = new byte[plaintext.Length];

            using (AesGcm aes = new(key, TagSize))
            {
                aes.Encrypt(nonce, plaintext, cipherText, tag, associatedData);
            }

            return [..salt, ..nonce, ..tag, ..cipherText];
        }
        catch (CryptographicException exception)
        {
            throw new BiometricException("Unable to protect data with Windows Hello.", exception);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(key);
        }
    }

    public async Task<byte[]?> UnprotectAsync(byte[] protectedData, byte[] associatedData)
    {
        if (protectedData.Length < HeaderSize)
        {
            throw new BiometricKeyInvalidatedException();
        }

        BringWindowToFront();
        KeyCredentialRetrievalResult result = await CallHelloAsync(() =>
            KeyCredentialManager.OpenAsync(CredentialName).AsTask());
        if (IsCancelled(result.Status)) return null;
        EnsureSuccess(result.Status);

        byte[] salt = protectedData.AsSpan(0, SaltSize).ToArray();
        byte[]? key = await DeriveKeyAsync(result.Credential, salt);
        if (key is null) return null;

        byte[] plaintext = new byte[protectedData.Length - HeaderSize];
        try
        {
            ReadOnlySpan<byte> nonce = protectedData.AsSpan(SaltSize, NonceSize);
            ReadOnlySpan<byte> tag = protectedData.AsSpan(SaltSize + NonceSize, TagSize);
            ReadOnlySpan<byte> cipherText = protectedData.AsSpan(HeaderSize);

            using (AesGcm aes = new(key, TagSize))
            {
                aes.Decrypt(nonce, cipherText, tag, plaintext, associatedData);
            }

            return plaintext;
        }
        catch (AuthenticationTagMismatchException exception)
        {
            // Different Hello key (e.g. Hello was reset) or tampered data / metadata.
            CryptographicOperations.ZeroMemory(plaintext);
            throw new BiometricKeyInvalidatedException(exception);
        }
        catch (CryptographicException exception)
        {
            CryptographicOperations.ZeroMemory(plaintext);
            throw new BiometricException("Unable to unlock data with Windows Hello.", exception);
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
            // Key does not exist (or is already gone), nothing to remove.
        }
    }

    private static async Task<byte[]?> DeriveKeyAsync(KeyCredential credential, byte[] salt)
    {
        KeyCredentialOperationResult signResult = await CallHelloAsync(() =>
            credential.RequestSignAsync(CryptographicBuffer.CreateFromByteArray(Challenge)).AsTask());
        if (IsCancelled(signResult.Status)) return null;
        EnsureSuccess(signResult.Status);

        CryptographicBuffer.CopyToByteArray(signResult.Result, out byte[] signature);
        try
        {
            return HKDF.DeriveKey(HashAlgorithmName.SHA256, signature, KeySize, salt, KeyInfo);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(signature);
        }
    }

    /// <summary>
    /// Checks that the key is protected by a TPM. Hello falls back to a software key on machines without
    /// a (usable) TPM; such a key is far easier to extract, so the user has to explicitly accept it.
    /// </summary>
    /// <remarks>
    /// Key attestation proves that this particular key lives in a TPM, but it often fails on devices that
    /// do have a TPM (e.g. firmware TPMs without an attestation identity key certificate, or Hello not
    /// managed by Windows Hello for Business). In that case the presence of a ready TPM is checked instead:
    /// Windows Hello stores its keys in the TPM whenever one is usable.
    /// </remarks>
    /// <returns><c>true</c> when the key is TPM-backed or the user accepted the weaker protection.</returns>
    private static async Task<bool> ConfirmHardwareBackedAsync(KeyCredential credential,
        Func<string, Task<bool>> confirmWeakProtection)
    {
        KeyCredentialAttestationResult attestation =
            await CallHelloAsync(() => credential.GetAttestationAsync().AsTask());

        if (attestation.Status == KeyCredentialAttestationStatus.Success) return true;
        if (TpmDetector.IsTpmAvailable()) return true;

        return await confirmWeakProtection(
            "No TPM security chip was found on this device, so the Windows Hello key is not protected by hardware.\n" +
            "Your master password would be less protected on this device. Turn on Windows Hello anyway?");
    }

    /// <summary>Wraps WinRT failures (COMException etc.) so callers only have to handle <see cref="BiometricException"/>.</summary>
    private static async Task<T> CallHelloAsync<T>(Func<Task<T>> call)
    {
        try
        {
            return await call();
        }
        catch (Exception exception) when (exception is not BiometricException)
        {
            throw new BiometricException("Windows Hello failed.", exception);
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
