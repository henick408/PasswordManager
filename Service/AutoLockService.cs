using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PasswordManager.Service.Platform;

namespace PasswordManager.Service;

/// <summary>
/// Requests locking the vault after <see cref="InactivityTimeout"/> without user input
/// in any window of the app (dialogs included), or when the OS session is locked.
/// Active only between <see cref="Start"/> and <see cref="Stop"/> (i.e. while signed in).
/// </summary>
/// <remarks>All members must be used on the UI thread.</remarks>
public class AutoLockService
{
    /// <summary>Time without keyboard or mouse input after which the vault is locked.</summary>
    public static readonly TimeSpan InactivityTimeout = TimeSpan.FromMinutes(5);

    private readonly ISessionLockNotifier sessionLockNotifier;
    private readonly DispatcherTimer inactivityTimer;
    private static bool inputHandlersRegistered;
    private static event Action? UserActivity;

    /// <summary>Raised on the UI thread when the vault should be locked (signed out).</summary>
    public event EventHandler? LockRequested;

    /// <summary><c>true</c> between <see cref="Start"/> and <see cref="Stop"/>.</summary>
    public bool IsRunning { get; private set; }

    public AutoLockService(ISessionLockNotifier sessionLockNotifier)
    {
        this.sessionLockNotifier = sessionLockNotifier;
        inactivityTimer = new DispatcherTimer { Interval = InactivityTimeout };
        inactivityTimer.Tick += (_, _) => RequestLock();
        RegisterInputHandlers();
    }

    /// <summary>Starts watching for inactivity and OS session lock. Call after signing in.</summary>
    public void Start()
    {
        if (IsRunning) return;
        IsRunning = true;
        UserActivity += ResetTimer;
        sessionLockNotifier.SessionLocked += OnSessionLocked;
        inactivityTimer.Start();
    }

    /// <summary>Stops watching. Call when the main window closes or the user signs out.</summary>
    public void Stop()
    {
        if (!IsRunning) return;
        IsRunning = false;
        inactivityTimer.Stop();
        UserActivity -= ResetTimer;
        sessionLockNotifier.SessionLocked -= OnSessionLocked;
    }

    /// <summary>Restarts the inactivity countdown.</summary>
    private void ResetTimer()
    {
        inactivityTimer.Stop();
        inactivityTimer.Start();
    }

    private void OnSessionLocked(object? sender, EventArgs e) => RequestLock();

    /// <summary>Stops watching (so the lock is requested only once) and raises <see cref="LockRequested"/>.</summary>
    private void RequestLock()
    {
        if (!IsRunning) return;
        Stop();
        LockRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Registers class handlers once per process. Tunnelling events start at the window,
    /// so a handler on <see cref="Window"/> sees input in every window, including dialogs,
    /// even when a control marks the event as handled.
    /// </summary>
    private static void RegisterInputHandlers()
    {
        if (inputHandlersRegistered) return;
        inputHandlersRegistered = true;

        const RoutingStrategies tunnel = RoutingStrategies.Tunnel;
        InputElement.KeyDownEvent.AddClassHandler<Window>((_, _) => UserActivity?.Invoke(), tunnel, true);
        InputElement.PointerPressedEvent.AddClassHandler<Window>((_, _) => UserActivity?.Invoke(), tunnel, true);
        InputElement.PointerMovedEvent.AddClassHandler<Window>((_, _) => UserActivity?.Invoke(), tunnel, true);
        InputElement.PointerWheelChangedEvent.AddClassHandler<Window>((_, _) => UserActivity?.Invoke(), tunnel, true);
    }
}
