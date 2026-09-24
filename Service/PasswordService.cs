using PasswordManager.Dto;
using PasswordManager.Repository;
using PasswordManager.Model;
using PasswordManager.CustomExceptions;

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
            ?? throw new PasswordNotFoundException(id);
        return encryptionService.Decrypt(encryptedPassword);
    }

    public async Task<PasswordEntry> CreatePassword(PasswordEntry passwordEntry)
    {
        EncryptedPassword encryptedPassword = encryptionService.Encrypt(passwordEntry);
        EncryptedPassword insertedPassword = await passwordRepository.InsertPassword(encryptedPassword);
        return encryptionService.Decrypt(insertedPassword);
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
