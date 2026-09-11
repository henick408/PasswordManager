using PasswordManager.CustomExceptions;
using PasswordManager.Dto;
using PasswordManager.Model;

namespace PasswordManager.Repository;

public class PasswordRepository(Supabase.Client supabase)
{
    public async Task InsertPassword(EncryptedPassword password)
    {
        EnsureThatLoggedIn();
        await supabase.From<EncryptedPassword>().Insert(password);
    }

    public async Task UpdatePassword(EncryptedPassword password)
    {
        EnsureThatLoggedIn();
        await supabase.From<EncryptedPassword>().Update(password);
    }

    public async Task<IList<EncryptedPassword>> ListPasswords()
    {
        EnsureThatLoggedIn();
        IList<EncryptedPassword> passwords = (await supabase.From<EncryptedPassword>().Get()).Models;

        return passwords;
    }

    public async Task<EncryptedPassword?> GetPassword(long id)
    {
        EnsureThatLoggedIn();
        EncryptedPassword? password = await supabase
        .From<EncryptedPassword>()
        .Where(password => password.Id == id)
        .Single();

        return password;
    }

    public async Task DeletePassword(long id)
    {
        EnsureThatLoggedIn();
        await supabase.From<EncryptedPassword>().Where(password => password.Id == id).Delete();
    }

    private void EnsureThatLoggedIn()
    {
        if (supabase.Auth.CurrentSession is null)
        {
            throw new UnauthenticatedUserException();
        }
    }

}
