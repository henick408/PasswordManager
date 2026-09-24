using PasswordManager.CustomExceptions;
using PasswordManager.Model;
using Supabase.Postgrest;

namespace PasswordManager.Repository;

public class PasswordRepository(Supabase.Client supabase)
{
    public async Task<EncryptedPassword> InsertPassword(EncryptedPassword password)
    {
        EnsureThatLoggedIn();
        var response = await supabase.From<EncryptedPassword>().Insert(password,
            new QueryOptions { Returning = QueryOptions.ReturnType.Representation });
        return response.Models.Single();
    }

    public async Task<EncryptedPassword> UpdatePassword(EncryptedPassword password)
    {
        EnsureThatLoggedIn();
        var response = await supabase.From<EncryptedPassword>().Update(password,
            new QueryOptions { Returning = QueryOptions.ReturnType.Representation });
        return response.Models.Single();
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
