namespace PasswordManager.CustomExceptions;

public class BiometricKeyInvalidatedException : BiometricException
{
    public BiometricKeyInvalidatedException() : base("The biometric key is no longer valid.")
    {
    }

    public BiometricKeyInvalidatedException(Exception? innerException)
        : base("The biometric key is no longer valid.", innerException)
    {
    }
}
