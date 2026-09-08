using System.Buffers.Text;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using PasswordManager.Dto;
using Supabase.Gotrue;

namespace PasswordManager.Service;

public class AuthService(Supabase.Client supabase)
{
    public async Task<Session?> SignUp(UserRequest request)
    {
        Session? session = await supabase.Auth.SignUp(request.Email, request.Password);
        return session;
    }

    public async Task<Session?> SignIn(UserRequest request)
    {
        Session? session = await supabase.Auth.SignIn(request.Email, request.Password);
        return session;
    }

    public User? GetCurrentUser()
    {
        return supabase.Auth.CurrentUser ?? throw new Exception("niezalogowany");
    }

    public async Task CreatePassword(PasswordEntry passwordEntry)
    {
        if (supabase.Auth.CurrentSession is null)
        {
            throw new Exception("niezalogowany");
        }
        var options = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        string json = JsonSerializer.Serialize(passwordEntry, options);
        Password password = new()
        {
            Content = json
        };
        await supabase.From<Password>().Insert(password);
    }
}
