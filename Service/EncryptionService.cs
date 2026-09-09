using System.Security.Cryptography;
using System.Text;
using PasswordManager.Model;

namespace PasswordManager.Service;

public class EncryptionService
{
    private readonly UserSession session;

    public EncryptionService(UserSession session)
    {
        Console.WriteLine(session.EncryptionHash);
        this.session = session;
    }

    public string Encrypt(string plainTextString)
    {
        EnsureThatLoggedIn();
        byte[] key = session.EncryptionHash!;
        byte[] plainText = Encoding.UTF8.GetBytes(plainTextString);
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

        return $"{nonceBase64}:{tagBase64}:{cipherTextBase64}";
    }

    private void EnsureThatLoggedIn()
    {
        if (session.EncryptionHash is null)
        {
            throw new Exception("Niezalogowany");
        }
    }
}
