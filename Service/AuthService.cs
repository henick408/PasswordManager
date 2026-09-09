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
        byte[] credentials = Encoding.UTF8.GetBytes(request.Email + request.Password);
        userSession.EncryptionHash = SHA256.HashData(credentials);
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
}
