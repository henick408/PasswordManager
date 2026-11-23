using PasswordManager.Dto;

namespace PasswordManager.Service;

public interface IPasswordService
{
    public Task<IList<PasswordEntry>> GetPasswords();
    /// <summary>Loads and decrypts a single entry.</summary>
    /// <param name="id">Entry id (<see cref="PasswordEntry.Id"/>).</param>
    /// <exception cref="PasswordManager.CustomExceptions.PasswordNotFoundException">No such entry.</exception>
    /// <exception cref="System.Security.Cryptography.CryptographicException">The entry cannot be decrypted.</exception>
    public Task<PasswordEntry> GetPassword(Guid id);
    public Task<PasswordEntry> CreatePassword(PasswordEntry passwordEntry);
    public Task<PasswordEntry> UpdatePassword(PasswordEntry passwordEntry);
    public Task DeletePassword(PasswordEntry passwordEntry);
}