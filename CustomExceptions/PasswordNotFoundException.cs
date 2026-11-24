namespace PasswordManager.CustomExceptions;

/// <summary>The requested entry does not exist (or is not visible to the signed-in user).</summary>
public class PasswordNotFoundException : Exception
{
    /// <summary>Id of the missing entry.</summary>
    public Guid PasswordId { get; }

    /// <param name="id">Id of the missing entry.</param>
    public PasswordNotFoundException(Guid id) : base($"Password with ID = {id} was not found")
    {
        PasswordId = id;
    }
}
