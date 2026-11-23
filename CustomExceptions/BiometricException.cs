namespace PasswordManager.CustomExceptions;

public class BiometricException : Exception
{
    public BiometricException(string? message) : base(message)
    {
    }

    public BiometricException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
