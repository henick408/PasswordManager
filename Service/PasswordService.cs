using System.Text.Encodings.Web;
using System.Text.Json;
using PasswordManager.Dto;
using PasswordManager.Repository;

namespace PasswordManager.Service;

public class PasswordService(PasswordRepository passwordRepository, EncryptionService encryptionService)
{
    public async Task<IList<PasswordEntry>> GetPasswords()
    {
        IList<string> encryptedPasswords = await passwordRepository.ListPasswords();
        IList<string> decryptedPasswords = encryptedPasswords.Select(encryptionService.Decrypt).ToList();

        var options = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        IList<PasswordEntry> passwordEntries = decryptedPasswords
                                        .Select(password => JsonSerializer.Deserialize<PasswordEntry>(password, options)!)
                                        .ToList();
        return passwordEntries;
    }
}
