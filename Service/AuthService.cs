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
                userSession.EncryptionHash = HashCredentials(request);
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
                userSession.EncryptionHash = HashCredentials(request);
            }
        }
        catch (GotrueException exception)
        {
            AuthError? error = JsonSerializer.Deserialize<AuthError>(exception.Message);
            throw new SupabaseAuthException(error ?? new AuthError(), exception.Reason);
        }
    }

    public void SignOut()
    {
        supabase.Auth.SignOut();
    }

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
