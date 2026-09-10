using PasswordManager.Model;

namespace PasswordManager.Repository;

public class PasswordRepository(Supabase.Client supabase)
{
    public async Task InsertPassword(string encryptedPassword)
    {
        if (supabase.Auth.CurrentSession is null)
        {
            throw new Exception("niezalogowany");
        }
        Password password = new()
        {
            Content = encryptedPassword
        };
        await supabase.From<Password>().Insert(password);
    }

    public async Task<IList<Password>> ListPasswords()
    {
        if (supabase.Auth.CurrentSession is null)
        {
            throw new Exception("niezalogowany");
        }
        IList<Password> passwords = (await supabase.From<Password>().Get()).Models;

        return passwords;
    }
}
