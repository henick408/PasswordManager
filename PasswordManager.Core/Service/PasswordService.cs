using PasswordManager.Core.Dto;
using PasswordManager.Core.Repository;
using PasswordManager.Core.Model;
using PasswordManager.Core.CustomExceptions;

namespace PasswordManager.Core.Service;

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
            ?? throw new PasswordNotFoundException(id);
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
