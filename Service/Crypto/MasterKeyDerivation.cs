using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace PasswordManager.Service.Crypto;

/// <summary>
/// Keys derived from the master password. Neither key can be computed from the other.
/// Dispose as soon as possible: both keys are zeroed.
/// </summary>
public sealed class DerivedKeys : IDisposable
{
    /// <summary>Sent to Supabase (as Base64) instead of the master password.</summary>
    public byte[] AuthKey { get; }

    /// <summary>Key-encryption key: wraps the random vault key. Never leaves the device.</summary>
    public byte[] Kek { get; }

    /// <summary>Takes ownership of both arrays: they are zeroed by <see cref="Dispose"/>.</summary>
    /// <param name="authKey">32-byte authentication key.</param>
    /// <param name="kek">32-byte key-encryption key.</param>
    public DerivedKeys(byte[] authKey, byte[] kek)
    {
        AuthKey = authKey;
        Kek = kek;
    }

    /// <summary>
    /// <see cref="AuthKey"/> as Base64, used as the Supabase account password.
    /// Contains only Base64 characters, so Supabase password character requirements must be disabled.
    /// </summary>
    public string AuthKeyBase64 => Convert.ToBase64String(AuthKey);

    /// <summary>Zeroes <see cref="AuthKey"/> and <see cref="Kek"/>.</summary>
    public void Dispose()
    {
        CryptographicOperations.ZeroMemory(AuthKey);
        CryptographicOperations.ZeroMemory(Kek);
    }
}

/// <summary>
/// Master password → keys:
/// <code>
/// salt      = SHA-256("PasswordManager.kdf.v1|" + normalized email)
/// masterKey = Argon2id(password, salt, m = 64 MiB, t = 3, p = 4)
/// authKey   = HKDF-Expand(masterKey, "PasswordManager.auth.v1")
/// kek       = HKDF-Expand(masterKey, "PasswordManager.kek.v1")
/// </code>
/// The parameters must be known before sign-in, so they are fixed per <see cref="CurrentVersion"/>.
/// Changing any of them requires a new version (and re-wrapping the vault key).
/// </summary>
public static class MasterKeyDerivation
{
    /// <summary>Version of the derivation parameters; stored as user_keys.kdf_version.</summary>
    public const short CurrentVersion = 1;

    /// <summary>Length of the master key and of each derived key, in bytes.</summary>
    private const int KeySize = 32;

    /// <summary>Argon2id memory cost: 64 MiB (in KiB).</summary>
    private const int Argon2MemoryKiB = 64 * 1024;

    /// <summary>Argon2id time cost (number of passes).</summary>
    private const int Argon2Iterations = 3;

    /// <summary>Argon2id parallelism (lanes).</summary>
    private const int Argon2Parallelism = 4;

    /// <summary>Domain separation for the salt, so it differs from other uses of SHA-256(email).</summary>
    private const string SaltPrefix = "PasswordManager.kdf.v1|";

    /// <summary>HKDF info for the authentication key.</summary>
    private static readonly byte[] AuthKeyInfo = "PasswordManager.auth.v1"u8.ToArray();

    /// <summary>HKDF info for the key-encryption key.</summary>
    private static readonly byte[] KekInfo = "PasswordManager.kek.v1"u8.ToArray();

    /// <summary>
    /// Derives the authentication key and the KEK from the master password.
    /// Deterministic: the same e-mail and password always give the same keys.
    /// Runs Argon2id on a thread-pool thread (takes noticeable time and 64 MiB of memory).
    /// </summary>
    /// <param name="email">Account e-mail; normalized and used to build the salt.</param>
    /// <param name="password">Master password; its UTF-8 copy is zeroed after use.</param>
    /// <returns>The derived keys; dispose them as soon as possible.</returns>
    public static async Task<DerivedKeys> DeriveAsync(string email, string password)
    {
        byte[] salt = SHA256.HashData(Encoding.UTF8.GetBytes(SaltPrefix + NormalizeEmail(email)));
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[]? masterKey = null;
        try
        {
            // Argon2 is CPU and memory heavy: keep it off the UI thread.
            masterKey = await Task.Run(() =>
            {
                using Argon2id argon2 = new(passwordBytes)
                {
                    Salt = salt,
                    MemorySize = Argon2MemoryKiB,
                    Iterations = Argon2Iterations,
                    DegreeOfParallelism = Argon2Parallelism
                };
                return argon2.GetBytes(KeySize);
            });

            byte[] authKey = HKDF.Expand(HashAlgorithmName.SHA256, masterKey, KeySize, AuthKeyInfo);
            byte[] kek = HKDF.Expand(HashAlgorithmName.SHA256, masterKey, KeySize, KekInfo);
            return new DerivedKeys(authKey, kek);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(passwordBytes);
            if (masterKey is not null)
            {
                CryptographicOperations.ZeroMemory(masterKey);
            }
        }
    }

    /// <summary>
    /// Supabase stores e-mails in lower case; trimming avoids a different salt for " a@b.c".
    /// </summary>
    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
