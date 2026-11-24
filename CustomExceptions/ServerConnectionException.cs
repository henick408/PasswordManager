namespace PasswordManager.CustomExceptions;

/// <summary>The server (Supabase) could not be reached: no internet connection, DNS failure or timeout.</summary>
public class ServerConnectionException : Exception
{
    /// <param name="innerException">The original network error.</param>
    public ServerConnectionException(Exception innerException)
        : base("Cannot connect to the server.", innerException)
    {
    }
}
