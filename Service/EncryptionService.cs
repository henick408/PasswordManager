using System.Security.Cryptography;
using System.Text;
using PasswordManager.CustomExceptions;
using PasswordManager.Dto;
using PasswordManager.Model;
using PasswordManager.Service.Crypto;

namespace PasswordManager.Service;

/// <summary>
/// Encrypts vault entries with AES-256-GCM using the session's random vault key.
/// The associated data binds each ciphertext to its owner and entry, so the server
/// (or anyone bypassing RLS) cannot swap ciphertexts between rows or users unnoticed.
/// </summary>
public class EncryptionService(UserSession session) : IEncryptionService
{
    /// <summary>
    /// Ciphertext format written to <see cref="EncryptedPassword.Version"/>:
    /// AES-256-GCM with the vault key and AAD <c>PasswordManager.entry.v2|{userId}|{entryId}</c>.
    /// </summary>
    public const short CurrentVersion = 2;
    private const string EntryAadPrefix = "PasswordManager.entry.v2|";

    /// <summary>
    /// Encrypts an entry. A new entry (<see cref="PasswordEntry.Id"/> == <see cref="Guid.Empty"/>)
    /// gets a UUIDv7 id here, because the id must be known before encryption to be bound into the AAD.
    /// </summary>
    /// <param name="decryptedPassword">Entry to encrypt; its <c>Id</c> may be assigned by this method.</param>
    /// <returns>Row ready to be inserted or updated (Base64 content, nonce and tag).</returns>
    /// <exception cref="UnauthenticatedUserException">The vault is locked.</exception>
    public EncryptedPassword Encrypt(PasswordEntry decryptedPassword)
    {
        EnsureThatLoggedIn();
        if (decryptedPassword.Id == Guid.Empty)
        {
            // New entry: UUIDv7 is random but time-ordered (index friendly, sortable by creation).
            decryptedPassword.Id = Guid.CreateVersion7();
        }

        byte[] plainText = Encoding.UTF8.GetBytes(decryptedPassword.ToString());
        try
        {
            SealedData sealedData = AesGcmBox.Seal(session.VaultKey!, plainText,
                BuildAssociatedData(decryptedPassword.Id));

            return new EncryptedPassword
            {
                Id = decryptedPassword.Id,
                Version = CurrentVersion,
                Content = Convert.ToBase64String(sealedData.CipherText),
                Nonce = Convert.ToBase64String(sealedData.Nonce),
                Tag = Convert.ToBase64String(sealedData.Tag)
            };
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plainText);
        }
    }

    /// <summary>Decrypts and authenticates a row from the password table.</summary>
    /// <param name="encryptedPassword">Row loaded from the database.</param>
    /// <returns>Decrypted entry with <see cref="PasswordEntry.Id"/> copied from the row.</returns>
    /// <exception cref="UnauthenticatedUserException">The vault is locked.</exception>
    /// <exception cref="CryptographicException">
    /// Unsupported version, malformed data, or a ciphertext that does not belong to this entry/user.
    /// </exception>
    public PasswordEntry Decrypt(EncryptedPassword encryptedPassword)
    {
        EnsureThatLoggedIn();
        if (encryptedPassword.Version != CurrentVersion)
        {
            throw new CryptographicException($"Unsupported entry version {encryptedPassword.Version}.");
        }

        byte[] plainText;
        try
        {
            plainText = AesGcmBox.Open(session.VaultKey!,
                Convert.FromBase64String(encryptedPassword.Nonce),
                Convert.FromBase64String(encryptedPassword.Tag),
                Convert.FromBase64String(encryptedPassword.Content),
                BuildAssociatedData(encryptedPassword.Id));
        }
        catch (FormatException exception)
        {
            throw new CryptographicException("Entry is not valid Base64.", exception);
        }

        try
        {
            PasswordEntry decryptedPassword = PasswordEntry.FromJson(Encoding.UTF8.GetString(plainText));
            decryptedPassword.Id = encryptedPassword.Id;
            return decryptedPassword;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plainText);
        }
    }

    /// <summary>AAD binding a ciphertext to the current user and the given entry.</summary>
    private byte[] BuildAssociatedData(Guid entryId) =>
        Encoding.UTF8.GetBytes($"{EntryAadPrefix}{session.UserId}|{entryId:D}");

    private void EnsureThatLoggedIn()
    {
        if (!session.IsUnlocked)
        {
            throw new UnauthenticatedUserException();
        }
    }
}
