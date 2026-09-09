using System.Text.Encodings.Web;
using System.Text.Json;
using PasswordManager.Dto;
using PasswordManager.Repository;
using PasswordManager.Model;

namespace PasswordManager.Service;

public class PasswordService(PasswordRepository passwordRepository, EncryptionService encryptionService)
{
    private readonly JsonSerializerOptions jsonSerializerOptions = new () { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public async Task<IList<PasswordEntry>> GetPasswords()
    {
        IList<string> encryptedPasswords = await passwordRepository.ListPasswords();
        IList<string> decryptedPasswords = encryptedPasswords.Select(encryptionService.Decrypt).ToList();

        IList<PasswordEntry> passwordEntries = decryptedPasswords
                                        .Select(password => JsonSerializer.Deserialize<PasswordEntry>(password, jsonSerializerOptions)!)
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
