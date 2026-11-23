using System.Security.Cryptography;

namespace PasswordManager.Service.Crypto;

/// <summary>Result of an AES-256-GCM encryption: 96-bit nonce, 128-bit tag and the ciphertext.</summary>
public sealed record SealedData(byte[] Nonce, byte[] Tag, byte[] CipherText);

/// <summary>
/// AES-256-GCM helpers. Every call uses a fresh random 96-bit nonce and authenticates
/// the given associated data (AAD), so ciphertexts cannot be moved to another context
/// (e.g. another entry or user) without failing authentication.
/// </summary>
public static class AesGcmBox
{
    public const int KeySize = 32;
    public const int NonceSize = 12;
    public const int TagSize = 16;

    public static SealedData Seal(byte[] key, ReadOnlySpan<byte> plaintext, ReadOnlySpan<byte> associatedData)
    {
        EnsureKeySize(key);
        byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
        byte[] tag = new byte[TagSize];
        byte[] cipherText = new byte[plaintext.Length];

        using AesGcm aes = new(key, TagSize);
        aes.Encrypt(nonce, plaintext, cipherText, tag, associatedData);

        return new SealedData(nonce, tag, cipherText);
    }

    /// <exception cref="AuthenticationTagMismatchException">Wrong key, wrong AAD or tampered data.</exception>
    /// <exception cref="CryptographicException">Malformed input.</exception>
    public static byte[] Open(byte[] key, SealedData data, ReadOnlySpan<byte> associatedData) =>
        Open(key, data.Nonce, data.Tag, data.CipherText, associatedData);

    /// <exception cref="AuthenticationTagMismatchException">Wrong key, wrong AAD or tampered data.</exception>
    /// <exception cref="CryptographicException">Malformed input.</exception>
    public static byte[] Open(byte[] key, ReadOnlySpan<byte> nonce, ReadOnlySpan<byte> tag,
        ReadOnlySpan<byte> cipherText, ReadOnlySpan<byte> associatedData)
    {
        EnsureKeySize(key);
        if (nonce.Length != NonceSize || tag.Length != TagSize)
        {
            throw new CryptographicException("Invalid nonce or tag length.");
        }

        byte[] plaintext = new byte[cipherText.Length];
        try
        {
            using AesGcm aes = new(key, TagSize);
            aes.Decrypt(nonce, cipherText, tag, plaintext, associatedData);
            return plaintext;
        }
        catch
        {
            CryptographicOperations.ZeroMemory(plaintext);
            throw;
        }
    }

    /// <summary>Encrypts into a single blob: <c>nonce(12) | tag(16) | ciphertext</c>.</summary>
    public static byte[] SealBlob(byte[] key, ReadOnlySpan<byte> plaintext, ReadOnlySpan<byte> associatedData)
    {
        SealedData sealedData = Seal(key, plaintext, associatedData);
        return [..sealedData.Nonce, ..sealedData.Tag, ..sealedData.CipherText];
    }

    /// <summary>Decrypts a blob produced by <see cref="SealBlob"/>.</summary>
    /// <exception cref="AuthenticationTagMismatchException">Wrong key, wrong AAD or tampered data.</exception>
    /// <exception cref="CryptographicException">Malformed input.</exception>
    public static byte[] OpenBlob(byte[] key, ReadOnlySpan<byte> blob, ReadOnlySpan<byte> associatedData)
    {
        if (blob.Length < NonceSize + TagSize)
        {
            throw new CryptographicException("Encrypted blob is too short.");
        }

        return Open(key,
            blob[..NonceSize],
            blob.Slice(NonceSize, TagSize),
            blob[(NonceSize + TagSize)..],
            associatedData);
    }

    private static void EnsureKeySize(byte[] key)
    {
        if (key.Length != KeySize)
        {
            throw new CryptographicException($"AES-256 key must be {KeySize} bytes.");
        }
    }
}
