namespace PasswordManager.Service.Platform;

/// <summary>Notifies when the operating system session is locked (e.g. Win+L).</summary>
public interface ISessionLockNotifier
{
    /// <summary>
    /// Raised on the UI thread when the OS session is locked.
    /// Subscribers must unsubscribe, implementations may hold a static OS hook.
    /// </summary>
    event EventHandler? SessionLocked;
}
