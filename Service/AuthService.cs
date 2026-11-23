using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PasswordManager.CustomExceptions;
using PasswordManager.Dto;
using PasswordManager.Model;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;

namespace PasswordManager.Service;

public class AuthService(Supabase.Client supabase, UserSession userSession) : IAuthService
{
    public async Task SignUpAsync(UserRequest request)
    {
        try
        {
            Session? session = await supabase.Auth.SignUp(request.Email, request.Password);
            if (session is not null)
            {
                userSession.EncryptionHash = await DeriveVaultKeyAsync(request);
                userSession.Email = request.Email;
            }
        }
        catch (GotrueException exception)
        {
            AuthError? error = JsonSerializer.Deserialize<AuthError>(exception.Message);
            throw new SupabaseAuthException(error ?? new AuthError(), exception.Reason);
        }
    }

    public async Task SignInAsync(UserRequest request)
    {
        try
        {
            Session? session = await supabase.Auth.SignIn(request.Email, request.Password);
            if (session is not null)
            {
                userSession.EncryptionHash = await DeriveVaultKeyAsync(request);
                userSession.Email = request.Email;
            }
        }
        catch (GotrueException exception)
        {
            AuthError? error = JsonSerializer.Deserialize<AuthError>(exception.Message);
            throw new SupabaseAuthException(error ?? new AuthError(), exception.Reason);
        }
    }

    public string? CurrentEmail => userSession.Email;

    public async Task<bool> VerifyMasterPasswordAsync(string password)
    {
        string? email = userSession.Email;
        byte[]? expected = userSession.EncryptionHash;
        if (email is null || expected is null) return false;

        byte[] actual = await DeriveVaultKeyAsync(new UserRequest(email, password));
        try
        {
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(actual);
        }
    }

    public void SignOut()
    {
        supabase.Auth.SignOut();
    }

    // Single place where the vault key is derived from credentials. Async so that a slow KDF
    // (or fetching a salt from the server) can be introduced later without touching callers.
    private Task<byte[]> DeriveVaultKeyAsync(UserRequest request) => Task.FromResult(HashCredentials(request));

    private byte[] HashCredentials(UserRequest request)
    {
        StringBuilder transformedEmail = new(request.Email.Length);
        for (int i = 0; i < request.Email.Length; i++)
        {
            transformedEmail.Append(ShiftChar(request.Email[i], i));
        }
        StringBuilder transformedPassword = new(request.Password.Length);
        for (int i = 0; i < request.Password.Length; i++)
        {
            transformedPassword.Append(ShiftChar(request.Password[i], i));
        }
        string transformedCredentials = $"{transformedEmail.Length}:{transformedEmail}{transformedPassword.Length}:{transformedPassword}";
        return SHA256.HashData(Encoding.UTF8.GetBytes(transformedCredentials));
    }

    private char ShiftChar(char character, int position)
    {
        return (char)(33 + (character - 33 + position) % 93);
    }
}
