using System.Security.Cryptography;

namespace PasswordManager.Model;

/// <summary>
/// In-memory state of the signed-in user (singleton). Filled by <c>AuthService.SignInAsync</c>
/// and cleared on sign-out. The master password itself is never stored here.
/// </summary>
public class UserSession
{
    /// <summary>
    /// Random AES-256 key that encrypts vault entries (unwrapped from <see cref="WrappedVaultKey"/>).
    /// Zeroed by <see cref="Clear"/>.
    /// </summary>
    public byte[]? VaultKey { get; set; }

    /// <summary>Supabase user id (auth.uid()), bound into the AAD of every ciphertext.</summary>
    public string? UserId { get; set; }

    /// <summary>E-mail used to sign in; needed to re-derive the keys when verifying the master password.</summary>
    public string? Email { get; set; }

    /// <summary>
    /// Vault key as stored in user_keys (<c>nonce | tag | ciphertext</c>).
    /// Used to verify the master password offline, without contacting Supabase.
    /// </summary>
    public byte[]? WrappedVaultKey { get; set; }

    /// <summary><c>true</c> when the vault key is available, i.e. entries can be encrypted and decrypted.</summary>
    public bool IsUnlocked => VaultKey is not null && UserId is not null;

    /// <summary>Zeroes the vault key in memory and forgets all session data.</summary>
    public void Clear()
    {
        if (VaultKey is not null)
        {
            CryptographicOperations.ZeroMemory(VaultKey);
        }
        VaultKey = null;
        UserId = null;
        Email = null;
        WrappedVaultKey = null;
    }
}
