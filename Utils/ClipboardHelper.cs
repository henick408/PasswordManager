using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;

namespace PasswordManager.Utils;

/// <summary>Access to the system clipboard, with automatic clearing of sensitive data.</summary>
public static class ClipboardHelper
{
    /// <summary>How long a copied secret stays in the clipboard.</summary>
    public static readonly TimeSpan SensitiveTextLifetime = TimeSpan.FromSeconds(30);

    /// <summary>The secret this app put in the clipboard last, or <c>null</c> when there is none to clear.</summary>
    private static string? _sensitiveText;

    /// <summary>Cancels the pending clear when a newer secret is copied.</summary>
    private static CancellationTokenSource? _clearCancellation;

    /// <summary>Clipboard of the main window, or <c>null</c> when it is not available (e.g. in the designer).</summary>
    public static IClipboard? GetClipboard()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow?.Clipboard;
        }
        return null;
    }

    /// <summary>
    /// Copies a secret (e.g. a password) and clears it after <see cref="SensitiveTextLifetime"/>,
    /// unless the user has copied something else in the meantime. Copying another secret
    /// restarts the countdown for the new one.
    /// </summary>
    /// <param name="text">Text to copy.</param>
    /// <remarks>Must be called on the UI thread.</remarks>
    public static async Task SetSensitiveTextAsync(string text)
    {
        IClipboard? clipboard = GetClipboard();
        if (clipboard is null) return;

        await clipboard.SetTextAsync(text);
        _sensitiveText = text;

        _clearCancellation?.Cancel();
        _clearCancellation?.Dispose();
        _clearCancellation = new CancellationTokenSource();

        // Not awaited: the calling command must finish now, otherwise its button stays disabled for 30 s.
        _ = ClearAfterDelayAsync(_clearCancellation.Token);
    }

    /// <summary>Waits <see cref="SensitiveTextLifetime"/> and clears the secret, unless cancelled.</summary>
    private static async Task ClearAfterDelayAsync(CancellationToken token)
    {
        try
        {
            // Resumes on the UI thread (captured synchronization context).
            await Task.Delay(SensitiveTextLifetime, token);
        }
        catch (TaskCanceledException)
        {
            return;
        }
        await ClearIfOursAsync();
    }

    /// <summary>
    /// Clears the clipboard if it still holds the secret copied by <see cref="SetSensitiveTextAsync"/>.
    /// Anything the user copied afterwards (from this or another app) is left untouched.
    /// Called e.g. on sign-out.
    /// </summary>
    /// <remarks>Must be called on the UI thread.</remarks>
    public static async Task ClearIfOursAsync()
    {
        string? sensitiveText = _sensitiveText;
        if (sensitiveText is null) return;

        _sensitiveText = null;
        _clearCancellation?.Cancel();

        IClipboard? clipboard = GetClipboard();
        if (clipboard is null) return;

        try
        {
            if (await clipboard.TryGetTextAsync() == sensitiveText)
            {
                await clipboard.ClearAsync();
            }
        }
        catch (Exception)
        {
            // The clipboard can be temporarily locked by another process; nothing more we can do.
        }
    }
}
