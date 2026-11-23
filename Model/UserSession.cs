namespace PasswordManager.Model;

public class UserSession
{
    public byte[]? EncryptionHash { get; set; }

    public string? Email { get; set; }

    public void Clear()
    {
        EncryptionHash = null;
        Email = null;
    }
}
