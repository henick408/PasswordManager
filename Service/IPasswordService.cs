using PasswordManager.Dto;

namespace PasswordManager.Service;

public interface IPasswordService
{
    public Task<IList<PasswordEntry>> GetPasswords();
    public Task<PasswordEntry> GetPassword(long id);
    public Task<PasswordEntry> CreatePassword(PasswordEntry passwordEntry);
    public Task<PasswordEntry> UpdatePassword(PasswordEntry passwordEntry);
    public Task DeletePassword(PasswordEntry passwordEntry);
}