using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace PasswordManager.Model;

/// <summary>
/// Row of public.user_keys: the user's vault key wrapped with the password-derived KEK.
/// One row per user, created on the first sign-in.
/// </summary>
[Table("user_keys")]
public class UserKeys : BaseModel
{
    /// <summary>
    /// Owner of the row. Not inserted: the database fills it with <c>DEFAULT auth.uid()</c>,
    /// so a client cannot create keys for another user.
    /// </summary>
    [PrimaryKey("user_id", false)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Version of the key derivation used to create the KEK
    /// (see <see cref="PasswordManager.Service.Crypto.MasterKeyDerivation.CurrentVersion"/>).
    /// </summary>
    [Column("kdf_version")]
    public short KdfVersion { get; set; }

    /// <summary>Base64 of <c>nonce | tag | ciphertext</c> (see AesGcmBox.SealBlob).</summary>
    [Column("wrapped_vault_key")]
    public string WrappedVaultKey { get; set; } = string.Empty;
}
