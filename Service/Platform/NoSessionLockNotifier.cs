namespace PasswordManager.Service.Platform;

/// <summary>
/// Used on platforms without session lock detection: never raises <see cref="SessionLocked"/>.
/// Auto-lock after inactivity still works.
/// </summary>
public class NoSessionLockNotifier : ISessionLockNotifier
{
    /// <inheritdoc />
    public event EventHandler? SessionLocked
    {
        add { }
        remove { }
    }
}
