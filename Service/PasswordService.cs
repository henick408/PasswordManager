using System.Text.Encodings.Web;
using System.Text.Json;
using PasswordManager.Dto;
using PasswordManager.Repository;
using PasswordManager.Model;

namespace PasswordManager.Service;

public class PasswordService(PasswordRepository passwordRepository, EncryptionService encryptionService)
{
    private readonly JsonSerializerOptions jsonSerializerOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public async Task<IList<PasswordEntry>> GetPasswords()
    {
        IList<Password> encryptedPasswords = await passwordRepository.ListPasswords();
        IList<Password> decryptedPasswords = encryptedPasswords
            .Select(password => new Password
            {
                Id = password.Id,
                Content = encryptionService.Decrypt(password.Content)
            })
            .ToList();
        IList<PasswordEntry> passwordEntries = decryptedPasswords
            .Select(password =>
            {
                PasswordEntry passwordEntry = JsonSerializer.Deserialize<PasswordEntry>(password.Content, jsonSerializerOptions)!;
                passwordEntry.Id = password.Id;
                return passwordEntry;
            })
            .ToList();
        return passwordEntries;
    }

    public async Task CreatePassword(PasswordEntry passwordEntry)
    {
        string jsonPassword = JsonSerializer.Serialize(passwordEntry, jsonSerializerOptions);
        string encryptedPassword = encryptionService.Encrypt(jsonPassword);

        await passwordRepository.InsertPassword(encryptedPassword);
    }
}
