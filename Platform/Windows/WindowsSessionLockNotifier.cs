using Avalonia.Threading;
using Microsoft.Win32;
using PasswordManager.Service.Platform;

namespace PasswordManager.Platform.Windows;

/// <summary>
/// Detects Windows session lock (Win+L, screen lock, switching user) through
/// <see cref="SystemEvents.SessionSwitch"/>. The OS hook is registered only while
/// someone is subscribed, because <see cref="SystemEvents"/> keeps static references.
/// </summary>
public class WindowsSessionLockNotifier : ISessionLockNotifier
{
    private EventHandler? sessionLocked;

    /// <inheritdoc />
    public event EventHandler? SessionLocked
    {
        add
        {
            if (sessionLocked is null)
            {
                SystemEvents.SessionSwitch += OnSessionSwitch;
            }
            sessionLocked += value;
        }
        remove
        {
            sessionLocked -= value;
            if (sessionLocked is null)
            {
                SystemEvents.SessionSwitch -= OnSessionSwitch;
            }
        }
    }

    /// <summary>
    /// May be called on the SystemEvents thread, so the event is forwarded to the UI thread.
    /// </summary>
    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        if (e.Reason != SessionSwitchReason.SessionLock) return;
        Dispatcher.UIThread.Post(() => sessionLocked?.Invoke(this, EventArgs.Empty));
    }
}
