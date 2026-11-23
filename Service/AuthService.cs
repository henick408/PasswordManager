using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PasswordManager.CustomExceptions;
using PasswordManager.Dto;
using PasswordManager.Model;
using PasswordManager.Repository;
using PasswordManager.Service.Crypto;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;

namespace PasswordManager.Service;

/// <summary>
/// Zero-knowledge sign-in: the master password never leaves the device.
/// Supabase only receives <see cref="DerivedKeys.AuthKey"/>; the vault key is random and stored
/// on the server wrapped with <see cref="DerivedKeys.Kek"/>, which the server cannot derive.
/// </summary>
public class AuthService(Supabase.Client supabase, UserSession userSession, UserKeysRepository userKeysRepository)
    : IAuthService
{
    private const string VaultKeyAadPrefix = "PasswordManager.vault-key.v1|";

    /// <summary>
    /// Registers the account with the derived auth key as the Supabase password.
    /// The vault key is not created here but on the first <see cref="SignInAsync"/>.
    /// </summary>
    /// <param name="request">E-mail and master password.</param>
    /// <exception cref="SupabaseAuthException">Supabase rejected the registration.</exception>
    public async Task SignUpAsync(UserRequest request)
    {
        using DerivedKeys keys = await MasterKeyDerivation.DeriveAsync(request.Email, request.Password);
        try
        {
            // The vault key is created on the first sign-in: with e-mail confirmation enabled
            // there is no session yet, so user_keys could not be written here anyway.
            await supabase.Auth.SignUp(request.Email, keys.AuthKeyBase64);
        }
        catch (GotrueException exception)
        {
            throw ToSupabaseAuthException(exception);
        }
    }

    /// <summary>
    /// Derives the keys, signs in to Supabase with the auth key, then loads (or on the first
    /// sign-in creates) the vault key and fills <see cref="UserSession"/>.
    /// </summary>
    /// <param name="request">E-mail and master password.</param>
    /// <exception cref="SupabaseAuthException">Wrong credentials or another Supabase error.</exception>
    /// <exception cref="VaultUnlockException">
    /// Signed in, but the vault key cannot be loaded or unwrapped; the user is signed out again.
    /// </exception>
    public async Task SignInAsync(UserRequest request)
    {
        using DerivedKeys keys = await MasterKeyDerivation.DeriveAsync(request.Email, request.Password);

        Session? session;
        try
        {
            session = await supabase.Auth.SignIn(request.Email, keys.AuthKeyBase64);
        }
        catch (GotrueException exception)
        {
            throw ToSupabaseAuthException(exception);
        }

        string? userId = supabase.Auth.CurrentUser?.Id;
        if (session is null || userId is null)
        {
            throw new VaultUnlockException("Supabase did not return a session.");
        }

        try
        {
            (byte[] vaultKey, byte[] wrappedVaultKey) = await LoadOrCreateVaultKeyAsync(keys.Kek, userId);
            userSession.Clear();
            userSession.VaultKey = vaultKey;
            userSession.WrappedVaultKey = wrappedVaultKey;
            userSession.UserId = userId;
            userSession.Email = request.Email;
        }
        catch (Exception exception)
        {
            // Authenticated but the vault cannot be opened: do not leave a half signed-in state.
            SignOut();
            throw exception as VaultUnlockException
                  ?? new VaultUnlockException("Unable to unlock the vault.", exception);
        }
    }

    /// <summary>E-mail of the signed-in user, or <c>null</c> when signed out.</summary>
    public string? CurrentEmail => userSession.Email;

    /// <summary>
    /// Checks the master password offline by trying to unwrap the vault key kept in the session
    /// (no request to Supabase). Used e.g. before enabling Windows Hello.
    /// </summary>
    /// <param name="password">Master password to check.</param>
    /// <returns><c>true</c> if it is the master password of the signed-in user.</returns>
    public async Task<bool> VerifyMasterPasswordAsync(string password)
    {
        string? email = userSession.Email;
        string? userId = userSession.UserId;
        byte[]? wrappedVaultKey = userSession.WrappedVaultKey;
        if (email is null || userId is null || wrappedVaultKey is null) return false;

        using DerivedKeys keys = await MasterKeyDerivation.DeriveAsync(email, password);
        try
        {
            // Only the right password yields a KEK that passes the AES-GCM tag check.
            byte[] vaultKey = AesGcmBox.OpenBlob(keys.Kek, wrappedVaultKey, VaultKeyAad(userId));
            CryptographicOperations.ZeroMemory(vaultKey);
            return true;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    /// <summary>Signs out of Supabase and zeroes the vault key in <see cref="UserSession"/>.</summary>
    public void SignOut()
    {
        supabase.Auth.SignOut();
        userSession.Clear();
    }

    /// <summary>
    /// Unwraps the stored vault key with <paramref name="kek"/>, or generates and stores
    /// a new random one when the user has no user_keys row yet.
    /// </summary>
    /// <param name="kek">Key-encryption key derived from the master password.</param>
    /// <param name="userId">Supabase user id, bound into the AAD of the wrapped key.</param>
    /// <returns>The plain vault key and its wrapped form (as stored in user_keys).</returns>
    /// <exception cref="VaultUnlockException">Unsupported KDF version, corrupted data or wrong KEK.</exception>
    private async Task<(byte[] VaultKey, byte[] WrappedVaultKey)> LoadOrCreateVaultKeyAsync(byte[] kek, string userId)
    {
        byte[] aad = VaultKeyAad(userId);
        UserKeys? userKeys = await userKeysRepository.GetAsync();

        if (userKeys is null)
        {
            // First sign-in of this account: generate the vault key.
            byte[] newVaultKey = RandomNumberGenerator.GetBytes(AesGcmBox.KeySize);
            byte[] newWrapped = AesGcmBox.SealBlob(kek, newVaultKey, aad);
            await userKeysRepository.InsertAsync(new UserKeys
            {
                KdfVersion = MasterKeyDerivation.CurrentVersion,
                WrappedVaultKey = Convert.ToBase64String(newWrapped)
            });
            return (newVaultKey, newWrapped);
        }

        if (userKeys.KdfVersion != MasterKeyDerivation.CurrentVersion)
        {
            throw new VaultUnlockException($"Unsupported key derivation version {userKeys.KdfVersion}.");
        }

        byte[] wrapped;
        try
        {
            wrapped = Convert.FromBase64String(userKeys.WrappedVaultKey);
        }
        catch (FormatException exception)
        {
            throw new VaultUnlockException("The stored vault key is corrupted.", exception);
        }

        try
        {
            return (AesGcmBox.OpenBlob(kek, wrapped, aad), wrapped);
        }
        catch (CryptographicException exception)
        {
            throw new VaultUnlockException(
                "The vault key cannot be unlocked with this master password.", exception);
        }
    }

    /// <summary>AAD of the wrapped vault key: binds it to its owner, so it cannot be moved to another account.</summary>
    private static byte[] VaultKeyAad(string userId) => Encoding.UTF8.GetBytes(VaultKeyAadPrefix + userId);

    private static SupabaseAuthException ToSupabaseAuthException(GotrueException exception)
    {
        AuthError? error = null;
        try
        {
            error = JsonSerializer.Deserialize<AuthError>(exception.Message);
        }
        catch (JsonException)
        {
            // Not a JSON error body (e.g. network failure).
        }
        return new SupabaseAuthException(error ?? new AuthError(), exception.Reason);
    }
}
