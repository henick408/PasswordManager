namespace PasswordManager.CustomExceptions;

/// <summary>
/// Signing in to Supabase succeeded, but the vault key could not be loaded or unwrapped
/// (e.g. the account password was reset by e-mail, so the key-encryption key no longer matches).
/// </summary>
/// <remarks>The user is signed out before this exception is thrown.</remarks>
public class VaultUnlockException : Exception
{
    /// <param name="message">Technical reason (not shown to the user).</param>
    /// <param name="innerException">Underlying error, e.g. a <see cref="System.Security.Cryptography.CryptographicException"/>.</param>
    public VaultUnlockException(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}
