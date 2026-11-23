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

    /// <summary>Loads a single entry of the signed-in user.</summary>
    /// <param name="id">Client-generated entry id.</param>
    /// <returns>The row, or <c>null</c> when it does not exist (or belongs to another user — hidden by RLS).</returns>
    /// <exception cref="UnauthenticatedUserException">No Supabase session.</exception>
    public async Task<EncryptedPassword?> GetPassword(Guid id)
    {
        EnsureThatLoggedIn();
        EncryptedPassword? password = await supabase
        .From<EncryptedPassword>()
        .Filter("id", Constants.Operator.Equals, id.ToString())
        .Single();

        return password;
    }

    /// <summary>Deletes an entry of the signed-in user. No-op when it does not exist.</summary>
    /// <param name="id">Client-generated entry id.</param>
    /// <exception cref="UnauthenticatedUserException">No Supabase session.</exception>
    public async Task DeletePassword(Guid id)
    {
        EnsureThatLoggedIn();
        await supabase.From<EncryptedPassword>().Filter("id", Constants.Operator.Equals, id.ToString()).Delete();
    }

    private void EnsureThatLoggedIn()
    {
        if (supabase.Auth.CurrentSession is null)
        {
            throw new UnauthenticatedUserException();
        }
    }

}
