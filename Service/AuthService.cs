using System.Security.Cryptography;
using System.Text;
using PasswordManager.Dto;
using PasswordManager.Model;
using Supabase.Gotrue;

namespace PasswordManager.Service;

public class AuthService(Supabase.Client supabase, UserSession userSession)
{
    public async Task<Session?> SignUp(UserRequest request)
    {
        Session? session = await supabase.Auth.SignUp(request.Email, request.Password);
        return session;
    }

    public async Task<Session?> SignIn(UserRequest request)
    {
        Session? session = await supabase.Auth.SignIn(request.Email, request.Password);
        userSession.EncryptionHash = HashCredentials(request);
        return session;
    }

    public Task SignOut()
    {
        return supabase.Auth.SignOut();
    }

    public User? GetCurrentUser()
    {
        return supabase.Auth.CurrentUser ?? throw new Exception("niezalogowany");
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
