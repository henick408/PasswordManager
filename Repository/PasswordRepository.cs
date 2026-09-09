using System.Text.Encodings.Web;
using System.Text.Json;
using PasswordManager.Dto;
using PasswordManager.Model;

namespace PasswordManager.Repository;

public class PasswordRepository(Supabase.Client supabase)
{
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

    public async Task<IList<string>> ListPasswords()
    {
        if (supabase.Auth.CurrentSession is null)
        {
            throw new Exception("niezalogowany");
        }
        IList<Password> passwords = (await supabase.From<Password>().Get()).Models;

        IList<string> encryptedPasswords = passwords.Select(password => password.Content).ToList();
        return encryptedPasswords;
    }
}
