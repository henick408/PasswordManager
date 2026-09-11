namespace PasswordManager.CustomExceptions;

public class PasswordNotFoundException : Exception
{
    public long PasswordId { get; }

    public PasswordNotFoundException(long id) : base($"Password with ID = {id} was not found")
    {
        PasswordId = id;
    }
}
