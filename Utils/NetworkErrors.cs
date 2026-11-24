using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;

namespace PasswordManager.Utils;

/// <summary>Recognizes exceptions caused by a missing or broken connection to the server.</summary>
public static class NetworkErrors
{
    /// <summary>
    /// <c>true</c> when <paramref name="exception"/> or any of its inner exceptions is a network failure
    /// (no internet, DNS failure, connection refused, timeout).
    /// </summary>
    /// <remarks>
    /// Supabase does not use its own type for this: GoTrue wraps the failure in a <c>GotrueException</c>
    /// with an inner <see cref="HttpRequestException"/>, Postgrest throws <see cref="HttpRequestException"/> directly,
    /// so the whole chain of inner exceptions is checked.
    /// </remarks>
    public static bool IsConnectionProblem(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is HttpRequestException or SocketException or WebSocketException or TimeoutException)
            {
                return true;
            }
        }
        return false;
    }
}
