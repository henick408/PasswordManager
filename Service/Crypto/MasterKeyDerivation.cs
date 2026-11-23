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

    public DerivedKeys(byte[] authKey, byte[] kek)
    {
        AuthKey = authKey;
        Kek = kek;
    }

    public string AuthKeyBase64 => Convert.ToBase64String(AuthKey);

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
    public const short CurrentVersion = 1;

    private const int KeySize = 32;
    private const int Argon2MemoryKiB = 64 * 1024;
    private const int Argon2Iterations = 3;
    private const int Argon2Parallelism = 4;

    private const string SaltPrefix = "PasswordManager.kdf.v1|";
    private static readonly byte[] AuthKeyInfo = "PasswordManager.auth.v1"u8.ToArray();
    private static readonly byte[] KekInfo = "PasswordManager.kek.v1"u8.ToArray();

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

    // Supabase stores emails in lower case; trimming avoids a different salt for " a@b.c".
    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
