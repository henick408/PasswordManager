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

    public async Task<IList<PasswordEntry>> GetPasswords()
    {
        if (supabase.Auth.CurrentSession is null)
        {
            throw new Exception("niezalogowany");
        }
        IList<Password> passwords = (await supabase.From<Password>().Get()).Models;

        var options = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        IList<PasswordEntry> passwordEntries = passwords
                                        .Select(password => JsonSerializer.Deserialize<PasswordEntry>(password.Content, options)!)
                                        .ToList();
        return passwordEntries;
    }
}
