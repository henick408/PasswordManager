namespace PasswordManager.Core.Model;

public class UserSession
{
    public byte[]? EncryptionHash { get; set; }

    public void Clear()
    {
        EncryptionHash = null;
    }
}
