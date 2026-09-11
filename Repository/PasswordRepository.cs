using PasswordManager.Dto;
using PasswordManager.Model;

namespace PasswordManager.Repository;

public class PasswordRepository(Supabase.Client supabase)
{
    public async Task InsertPassword(EncryptedPassword password)
    {
        if (supabase.Auth.CurrentSession is null)
        {
            throw new Exception("niezalogowany");
        }
        await supabase.From<EncryptedPassword>().Insert(password);
    }

    public async Task UpdatePassword(EncryptedPassword password)
    {
        if (supabase.Auth.CurrentSession is null)
        {
            throw new Exception("niezalogowany");
        }
        await supabase.From<EncryptedPassword>().Update(password);
    }

    public async Task<IList<EncryptedPassword>> ListPasswords()
    {
        if (supabase.Auth.CurrentSession is null)
        {
            throw new Exception("niezalogowany");
        }
        IList<EncryptedPassword> passwords = (await supabase.From<EncryptedPassword>().Get()).Models;

        return passwords;
    }

    public async Task<EncryptedPassword?> GetPassword(long id)
    {
        if (supabase.Auth.CurrentSession is null)
        {
            throw new Exception("niezalogowany");
        }
        EncryptedPassword? password = await supabase
        .From<EncryptedPassword>()
        .Where(password => password.Id == id)
        .Single();

        return password;
    }

    public async Task DeletePassword(long id)
    {
        if (supabase.Auth.CurrentSession is null)
        {
            throw new Exception("niezalogowany");
        }
        await supabase.From<EncryptedPassword>().Where(password => password.Id == id).Delete();
    }

}
