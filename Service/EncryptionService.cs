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

    public string Decrypt(string cipherTextString)
    {
        EnsureThatLoggedIn();
        byte[] key = session.EncryptionHash!;
        IList<string> cipherDataBase64 = cipherTextString.Split(':');
        if (cipherDataBase64.Count != 3)
        {
            throw new Exception("Data is not correct");
        }
        IList<byte[]> cipherData = cipherDataBase64
            .Select(Convert.FromBase64String)
            .ToList();

        byte[] nonce = cipherData[0];
        byte[] tag = cipherData[1];
        byte[] cipherText = cipherData[2];
        byte[] plainText = new byte[cipherText.Length];

        using (AesGcm aes = new(key, tag.Length))
        {
            aes.Decrypt(nonce, cipherText, tag, plainText);
        }

        return Encoding.UTF8.GetString(plainText);
    }

    private void EnsureThatLoggedIn()
    {
        if (session.EncryptionHash is null)
        {
            throw new Exception("Niezalogowany");
        }
    }
}