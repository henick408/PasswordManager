namespace PasswordManager.CustomExceptions;

public class UnauthenticatedUserException : Exception
{
    public UnauthenticatedUserException() : base("User is not authenticated. Sign in first.")
    {
    }

    public UnauthenticatedUserException(string? message) : base(message)
    {
    }
}
