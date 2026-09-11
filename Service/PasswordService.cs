using System.Text.Encodings.Web;
using System.Text.Json;
using PasswordManager.Dto;
using PasswordManager.Repository;
using PasswordManager.Model;

namespace PasswordManager.Service;

public class PasswordService(PasswordRepository passwordRepository, EncryptionService encryptionService)
{
    public async Task<IList<PasswordEntry>> GetPasswords()
    {
        IList<EncryptedPassword> encryptedPasswords = await passwordRepository.ListPasswords();
        return encryptedPasswords
            .Select(encryptionService.Decrypt)
            .ToList();
    }

    public async Task<PasswordEntry> GetPassword(long id)
    {
        EncryptedPassword encryptedPassword = await passwordRepository.GetPassword(id)
            ?? throw new Exception("Password z takim id nie istnieje");
        return encryptionService.Decrypt(encryptedPassword);
    }

    public async Task CreatePassword(PasswordEntry passwordEntry)
    {
        EncryptedPassword encryptedPassword = encryptionService.Encrypt(passwordEntry);
        await passwordRepository.InsertPassword(encryptedPassword);
    }

    public async Task UpdatePassword(PasswordEntry passwordEntry)
    {
        EncryptedPassword encryptedPassword = encryptionService.Encrypt(passwordEntry);
        await passwordRepository.UpdatePassword(encryptedPassword);
    }

    public Task DeletePassword(PasswordEntry passwordEntry)
    {
        return passwordRepository.DeletePassword(passwordEntry.Id);
    }
}
