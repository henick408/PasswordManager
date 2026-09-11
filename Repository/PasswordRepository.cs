using PasswordManager.Dto;
using PasswordManager.Model;

namespace PasswordManager.Repository;

public class PasswordRepository(Supabase.Client supabase)
{
    public async Task InsertPassword(Password password)
    {
        if (supabase.Auth.CurrentSession is null)
        {
            throw new Exception("niezalogowany");
        }
        await supabase.From<Password>().Insert(password);
    }

    public async Task UpdatePassword(Password password)
    {
        if (supabase.Auth.CurrentSession is null)
        {
            throw new Exception("niezalogowany");
        }
        await supabase.From<Password>().Update(password);
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

    public async Task<Password?> GetPassword(long id)
    {
        if (supabase.Auth.CurrentSession is null)
        {
            throw new Exception("niezalogowany");
        }
        Password? password = await supabase
        .From<Password>()
        .Where(password => password.Id == id)
        .Single();

        return password;
    }

}
