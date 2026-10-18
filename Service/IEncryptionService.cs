using PasswordManager.Dto;
using PasswordManager.Model;

namespace PasswordManager.Service;

public interface IEncryptionService
{
    public EncryptedPassword Encrypt(PasswordEntry decryptedPassword);
    public PasswordEntry Decrypt(EncryptedPassword encryptedPassword);
}