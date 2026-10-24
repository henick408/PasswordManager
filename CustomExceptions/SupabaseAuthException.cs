using static Supabase.Gotrue.Exceptions.FailureHint;

namespace PasswordManager.CustomExceptions;

public class SupabaseAuthException : Exception
{
    public AuthError Details { get; }
    public Reason Reason { get; }

    public SupabaseAuthException(AuthError details, Reason reason) : base("Supabase authentication error. Error details can be found in Details property")
    {
        Details = details;
        Reason = reason;
    }
}