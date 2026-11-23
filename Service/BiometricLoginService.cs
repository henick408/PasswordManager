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

    private record BiometricVault(int Version, string Email, string Data);

    private BiometricVault? cachedVault;
    private bool isCacheLoaded;

    public string DisplayName => authenticator.DisplayName;

    public bool IsEnabled => GetVault() is not null;

    public string? EnrolledEmail => GetVault()?.Email;

    public bool IsEnabledFor(string? email) =>
        email is not null && string.Equals(EnrolledEmail, email, StringComparison.OrdinalIgnoreCase);

    public Task<bool> IsAvailableAsync() => authenticator.IsAvailableAsync();

    public async Task<bool> EnableAsync(UserRequest credentials)
    {
        byte[] plaintext = JsonSerializer.SerializeToUtf8Bytes(credentials, JsonOptions);
        try
        {
            byte[]? protectedData =
                await authenticator.ProtectAsync(plaintext, BuildAssociatedData(CurrentVersion, credentials.Email));
            if (protectedData is null) return false;

            SaveVault(new BiometricVault(CurrentVersion, credentials.Email, Convert.ToBase64String(protectedData)));
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
            plaintext = await authenticator.UnprotectAsync(protectedData, BuildAssociatedData(vault.Version, vault.Email));
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
                || !string.Equals(credentials.Email, vault.Email, StringComparison.OrdinalIgnoreCase))
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
    // so swapping the email or version in the file is detected on decryption.
    private static byte[] BuildAssociatedData(int version, string email) =>
        Encoding.UTF8.GetBytes($"{version}|{email.ToLowerInvariant()}");

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
                || string.IsNullOrWhiteSpace(vault.Email)
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
