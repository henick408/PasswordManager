using PasswordManager.Dto;

namespace PasswordManager.Model;

public class UserSession
{
    public byte[]? EncryptionHash { get; set; }

    public UserRequest? Credentials { get; set; }

    public void Clear()
    {
        EncryptionHash = null;
        Credentials = null;
    }
}
