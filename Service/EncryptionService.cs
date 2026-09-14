using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using PasswordManager.CustomExceptions;
using PasswordManager.Dto;
using PasswordManager.Model;

namespace PasswordManager.Service;

public class EncryptionService
{
    private readonly UserSession session;

    public EncryptionService(UserSession session)
    {
        this.session = session;
    }

    public EncryptedPassword Encrypt(PasswordEntry decryptedPassword)
    {
        EnsureThatLoggedIn();
        byte[] key = session.EncryptionHash!;
        byte[] plainText = Encoding.UTF8.GetBytes(decryptedPassword.ToString());
        byte[] cipherText = new byte[plainText.Length];

        byte[] nonce = new byte[12];
        byte[] tag = new byte[16];

        RandomNumberGenerator.Fill(nonce);

        using (AesGcm aes = new(key, tag.Length)) {
            aes.Encrypt(nonce, plainText, cipherText, tag);
        }

        string nonceBase64 = Convert.ToBase64String(nonce);
        string tagBase64 = Convert.ToBase64String(tag);
        string cipherTextBase64 = Convert.ToBase64String(cipherText);
        return new EncryptedPassword
        {
            Id = decryptedPassword.Id,
            Content = cipherTextBase64,
            Nonce = nonceBase64,
            Tag = tagBase64
        };
    }

    public PasswordEntry Decrypt(EncryptedPassword encryptedPassword)
    {
        EnsureThatLoggedIn();
        byte[] key = session.EncryptionHash!;
        byte[] cipherText = Convert.FromBase64String(encryptedPassword.Content);
        byte[] nonce = Convert.FromBase64String(encryptedPassword.Nonce);
        byte[] tag = Convert.FromBase64String(encryptedPassword.Tag);

        byte[] plainText = new byte[cipherText.Length];

        using (AesGcm aes = new(key, tag.Length))
        {
            aes.Decrypt(nonce, cipherText, tag, plainText);
        }
        string json = Encoding.UTF8.GetString(plainText);

        PasswordEntry decryptedPassword = PasswordEntry.FromJson(json);
        decryptedPassword.Id = encryptedPassword.Id;

        return decryptedPassword;
    }

    private void EnsureThatLoggedIn()
    {
        if (session.EncryptionHash is null)
        {
            throw new UnauthenticatedUserException();
        }
    }
}