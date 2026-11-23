using PasswordManager.CustomExceptions;
using PasswordManager.Model;
using Supabase.Postgrest;

namespace PasswordManager.Repository;

/// <summary>Access to the public.user_keys table. Requires an active Supabase session.</summary>
public class UserKeysRepository(Supabase.Client supabase)
{
    /// <summary>Loads the signed-in user's wrapped vault key.</summary>
    /// <returns>The signed-in user's keys (RLS returns only the own row), or <c>null</c> on first sign-in.</returns>
    /// <exception cref="UnauthenticatedUserException">No Supabase session.</exception>
    public async Task<UserKeys?> GetAsync()
    {
        EnsureThatLoggedIn();
        var response = await supabase.From<UserKeys>().Get();
        return response.Models.SingleOrDefault();
    }

    /// <summary>Stores the wrapped vault key of the signed-in user (user_id is set by the database).</summary>
    /// <param name="userKeys">KDF version and Base64 wrapped vault key.</param>
    /// <returns>The inserted row, as returned by the database.</returns>
    /// <exception cref="UnauthenticatedUserException">No Supabase session.</exception>
    public async Task<UserKeys> InsertAsync(UserKeys userKeys)
    {
        EnsureThatLoggedIn();
        var response = await supabase.From<UserKeys>().Insert(userKeys,
            new QueryOptions { Returning = QueryOptions.ReturnType.Representation });
        return response.Models.Single();
    }

    private void EnsureThatLoggedIn()
    {
        if (supabase.Auth.CurrentSession is null)
        {
            throw new UnauthenticatedUserException();
        }
    }
}
