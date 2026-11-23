using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PasswordManager.CustomExceptions;
using PasswordManager.Dto;
using PasswordManager.Service.Platform;

namespace PasswordManager.Service;

public class BiometricLoginService(IBiometricAuthenticator authenticator) : IBiometricLoginService
{
    private const int CurrentVersion = 2;

    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PasswordManager",
        "biometric.json");

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private const int EmailSaltSize = 16;

    // The email is not stored in plain text: only a salted SHA-256 hash, so reading the file
    // does not reveal which account is enrolled. The email itself lives inside the encrypted Data.
    private record BiometricVault(int Version, string EmailSalt, string EmailHash, string Data);

    private BiometricVault? cachedVault;
    private bool isCacheLoaded;

    public string DisplayName => authenticator.DisplayName;

    public bool IsEnabled => GetVault() is not null;

    public bool IsEnabledFor(string? email)
    {
        BiometricVault? vault = GetVault();
        if (vault is null || email is null) return false;
        try
        {
            byte[] expected = Convert.FromBase64String(vault.EmailHash);
            byte[] actual = HashEmail(Convert.FromBase64String(vault.EmailSalt), email);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public Task<bool> IsAvailableAsync() => authenticator.IsAvailableAsync();

    public async Task<bool> EnableAsync(UserRequest credentials, Func<string, Task<bool>> confirmWeakProtection)
    {
        byte[] plaintext = JsonSerializer.SerializeToUtf8Bytes(credentials, JsonOptions);
        try
        {
            byte[] emailSalt = RandomNumberGenerator.GetBytes(EmailSaltSize);
            string emailSaltBase64 = Convert.ToBase64String(emailSalt);
            string emailHashBase64 = Convert.ToBase64String(HashEmail(emailSalt, credentials.Email));

            byte[]? protectedData = await authenticator.ProtectAsync(
                plaintext, BuildAssociatedData(CurrentVersion, emailSaltBase64, emailHashBase64),
                confirmWeakProtection);
            if (protectedData is null) return false;

            SaveVault(new BiometricVault(CurrentVersion, emailSaltBase64, emailHashBase64,
                Convert.ToBase64String(protectedData)));
            return true;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintext);
        }
    }

    public async Task DisableAsync()
    {
        DeleteVaultFile();
        await authenticator.RemoveAsync();
    }

    public async Task<UserRequest?> UnlockCredentialsAsync()
    {
        BiometricVault? vault = GetVault();
        if (vault is null) return null;

        byte[] protectedData;
        try
        {
            protectedData = Convert.FromBase64String(vault.Data);
        }
        catch (FormatException exception)
        {
            await DisableAsync();
            throw new BiometricKeyInvalidatedException(exception);
        }

        byte[]? plaintext;
        try
        {
            plaintext = await authenticator.UnprotectAsync(protectedData,
                BuildAssociatedData(vault.Version, vault.EmailSalt, vault.EmailHash));
        }
        catch (BiometricKeyInvalidatedException)
        {
            await DisableAsync();
            throw;
        }
        if (plaintext is null) return null;

        try
        {
            UserRequest? credentials = JsonSerializer.Deserialize<UserRequest>(plaintext, JsonOptions);
            if (credentials is null
                || string.IsNullOrEmpty(credentials.Password)
                || !IsEnabledFor(credentials.Email))
            {
                throw new JsonException("Unexpected biometric payload.");
            }
            return credentials;
        }
        catch (JsonException exception)
        {
            await DisableAsync();
            throw new BiometricKeyInvalidatedException(exception);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintext);
        }
    }

    // Binds the encrypted payload to the metadata stored in plain text next to it,
    // so swapping the version or email hash in the file is detected on decryption.
    private static byte[] BuildAssociatedData(int version, string emailSalt, string emailHash) =>
        Encoding.UTF8.GetBytes($"{version}|{emailSalt}|{emailHash}");

    private static byte[] HashEmail(byte[] salt, string email) =>
        SHA256.HashData([..salt, ..Encoding.UTF8.GetBytes(email.Trim().ToLowerInvariant())]);

    private BiometricVault? GetVault()
    {
        if (!isCacheLoaded)
        {
            cachedVault = LoadVault();
            isCacheLoaded = true;
        }
        return cachedVault;
    }

    private static BiometricVault? LoadVault()
    {
        if (!File.Exists(FilePath)) return null;
        try
        {
            BiometricVault? vault = JsonSerializer.Deserialize<BiometricVault>(File.ReadAllText(FilePath), JsonOptions);
            if (vault is null
                || vault.Version != CurrentVersion
                || string.IsNullOrWhiteSpace(vault.EmailSalt)
                || string.IsNullOrWhiteSpace(vault.EmailHash)
                || string.IsNullOrWhiteSpace(vault.Data))
            {
                // Outdated (e.g. v1) or malformed file: its Hello key is gone/unusable, start over.
                TryDelete(FilePath);
                return null;
            }
            return vault;
        }
        catch (JsonException)
        {
            TryDelete(FilePath);
            return null;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    private void SaveVault(BiometricVault vault)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        string tempPath = FilePath + ".tmp";
        try
        {
            File.WriteAllText(tempPath, JsonSerializer.Serialize(vault, JsonOptions));
            File.Move(tempPath, FilePath, overwrite: true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            TryDelete(tempPath);
            throw new BiometricException("Unable to save biometric sign-in data.", exception);
        }
        cachedVault = vault;
        isCacheLoaded = true;
    }

    private void DeleteVaultFile()
    {
        cachedVault = null;
        isCacheLoaded = true;
        TryDelete(FilePath);
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // Best effort: an undecryptable leftover file is harmless.
        }
    }
}
