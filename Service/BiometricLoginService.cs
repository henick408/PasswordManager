using System.Security.Cryptography;
using System.Text.Json;
using PasswordManager.CustomExceptions;
using PasswordManager.Dto;
using PasswordManager.Service.Platform;

namespace PasswordManager.Service;

public class BiometricLoginService(IBiometricAuthenticator authenticator) : IBiometricLoginService
{
    private const int CurrentVersion = 1;

    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PasswordManager",
        "biometric.json");

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private record BiometricVault(int Version, string Email, string Data);

    public string DisplayName => authenticator.DisplayName;

    public bool IsEnabled => LoadVault() is not null;

    public string? EnrolledEmail => LoadVault()?.Email;

    public Task<bool> IsAvailableAsync() => authenticator.IsAvailableAsync();

    public async Task<bool> EnableAsync(UserRequest credentials)
    {
        byte[] plaintext = JsonSerializer.SerializeToUtf8Bytes(credentials, JsonOptions);
        try
        {
            byte[]? protectedData = await authenticator.ProtectAsync(plaintext);
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
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
        await authenticator.RemoveAsync();
    }

    public async Task<UserRequest?> UnlockCredentialsAsync()
    {
        BiometricVault? vault = LoadVault();
        if (vault is null) return null;

        byte[]? plaintext;
        try
        {
            plaintext = await authenticator.UnprotectAsync(Convert.FromBase64String(vault.Data));
        }
        catch (BiometricKeyInvalidatedException)
        {
            await DisableAsync();
            throw;
        }
        if (plaintext is null) return null;

        try
        {
            return JsonSerializer.Deserialize<UserRequest>(plaintext, JsonOptions);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintext);
        }
    }

    private static BiometricVault? LoadVault()
    {
        if (!File.Exists(FilePath)) return null;
        try
        {
            BiometricVault? vault = JsonSerializer.Deserialize<BiometricVault>(File.ReadAllText(FilePath), JsonOptions);
            return vault?.Version == CurrentVersion ? vault : null;
        }
        catch (Exception exception) when (exception is JsonException or IOException)
        {
            return null;
        }
    }

    private static void SaveVault(BiometricVault vault)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(vault, JsonOptions));
    }
}
