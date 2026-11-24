using System.Security.Cryptography;

namespace PasswordManager.Service.Crypto;

/// <summary>Result of an AES-256-GCM encryption: 96-bit nonce, 128-bit tag and the ciphertext.</summary>
/// <param name="Nonce">Random 96-bit nonce, unique per encryption.</param>
/// <param name="Tag">128-bit authentication tag covering the ciphertext and the associated data.</param>
/// <param name="CipherText">Encrypted data, same length as the plaintext.</param>
public sealed record SealedData(byte[] Nonce, byte[] Tag, byte[] CipherText);

/// <summary>
/// AES-256-GCM helpers. Every call uses a fresh random 96-bit nonce and authenticates
/// the given associated data (AAD), so ciphertexts cannot be moved to another context
/// (e.g. another entry or user) without failing authentication.
/// </summary>
public static class AesGcmBox
{
    /// <summary>AES-256 key length in bytes.</summary>
    public const int KeySize = 32;

    /// <summary>AES-GCM nonce length in bytes (96 bits, the recommended size).</summary>
    public const int NonceSize = 12;

    /// <summary>AES-GCM authentication tag length in bytes (128 bits, full length).</summary>
    public const int TagSize = 16;

    /// <summary>Encrypts and authenticates <paramref name="plaintext"/> with a fresh random nonce.</summary>
    /// <param name="key">32-byte AES-256 key.</param>
    /// <param name="plaintext">Data to encrypt; not modified (the caller is responsible for zeroing it).</param>
    /// <param name="associatedData">Context that is authenticated but not encrypted; the same AAD is required to decrypt.</param>
    /// <returns>Nonce, tag and ciphertext.</returns>
    /// <exception cref="CryptographicException">The key is not <see cref="KeySize"/> bytes long.</exception>
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

    /// <summary>Decrypts and verifies data produced by <see cref="Seal"/>.</summary>
    /// <param name="key">32-byte AES-256 key used for encryption.</param>
    /// <param name="data">Nonce, tag and ciphertext.</param>
    /// <param name="associatedData">The same AAD that was passed to <see cref="Seal"/>.</param>
    /// <returns>The plaintext; the caller should zero it after use.</returns>
    /// <exception cref="AuthenticationTagMismatchException">Wrong key, wrong AAD or tampered data.</exception>
    /// <exception cref="CryptographicException">Malformed input.</exception>
    public static byte[] Open(byte[] key, SealedData data, ReadOnlySpan<byte> associatedData) =>
        Open(key, data.Nonce, data.Tag, data.CipherText, associatedData);

    /// <summary>
    /// Decrypts and verifies data given as separate parts (e.g. separate database columns).
    /// Nothing is returned unless the tag matches; on failure the output buffer is zeroed.
    /// </summary>
    /// <param name="key">32-byte AES-256 key used for encryption.</param>
    /// <param name="nonce">12-byte nonce.</param>
    /// <param name="tag">16-byte authentication tag.</param>
    /// <param name="cipherText">Encrypted data.</param>
    /// <param name="associatedData">The same AAD that was used for encryption.</param>
    /// <returns>The plaintext; the caller should zero it after use.</returns>
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
    /// <param name="key">32-byte AES-256 key.</param>
    /// <param name="plaintext">Data to encrypt.</param>
    /// <param name="associatedData">Context that is authenticated but not encrypted.</param>
    /// <returns>The blob, convenient to store in a single column or file.</returns>
    /// <exception cref="CryptographicException">The key is not <see cref="KeySize"/> bytes long.</exception>
    public static byte[] SealBlob(byte[] key, ReadOnlySpan<byte> plaintext, ReadOnlySpan<byte> associatedData)
    {
        SealedData sealedData = Seal(key, plaintext, associatedData);
        return [..sealedData.Nonce, ..sealedData.Tag, ..sealedData.CipherText];
    }

    /// <summary>Decrypts a blob produced by <see cref="SealBlob"/>.</summary>
    /// <param name="key">32-byte AES-256 key used for encryption.</param>
    /// <param name="blob"><c>nonce | tag | ciphertext</c>.</param>
    /// <param name="associatedData">The same AAD that was used for encryption.</param>
    /// <returns>The plaintext; the caller should zero it after use.</returns>
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

    /// <summary>Rejects keys other than AES-256, so a wrong buffer cannot silently select AES-128/192.</summary>
    /// <exception cref="CryptographicException">The key is not <see cref="KeySize"/> bytes long.</exception>
    private static void EnsureKeySize(byte[] key)
    {
        if (key.Length != KeySize)
        {
            throw new CryptographicException($"AES-256 key must be {KeySize} bytes.");
        }
    }
}
