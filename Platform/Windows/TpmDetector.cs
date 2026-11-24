using System.Runtime.InteropServices;

namespace PasswordManager.Platform.Windows;

/// <summary>
/// Detects whether the device has a TPM that Windows can use for keys.
/// </summary>
internal static class TpmDetector
{
    /// <summary>CNG key storage provider backed by the TPM (MS_PLATFORM_CRYPTO_PROVIDER).</summary>
    private const string PlatformCryptoProvider = "Microsoft Platform Crypto Provider";

    /// <summary>
    /// Returns <c>true</c> when the TPM key storage provider can be opened, i.e. a TPM is present,
    /// enabled and ready. Does not require administrator rights and does not prompt the user.
    /// </summary>
    public static bool IsTpmAvailable()
    {
        try
        {
            int status = NCryptOpenStorageProvider(out IntPtr provider, PlatformCryptoProvider, 0);
            if (status != 0) return false;
            NCryptFreeObject(provider);
            return true;
        }
        catch (Exception)
        {
            // ncrypt.dll unavailable or another interop failure: assume no TPM.
            return false;
        }
    }

    [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
    private static extern int NCryptOpenStorageProvider(out IntPtr phProvider, string pszProviderName, int dwFlags);

    [DllImport("ncrypt.dll")]
    private static extern int NCryptFreeObject(IntPtr hObject);
}
