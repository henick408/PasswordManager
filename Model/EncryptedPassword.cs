using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace PasswordManager.Model;

/// <summary>
/// Row of public.password: one vault entry encrypted with AES-256-GCM
/// (see <see cref="PasswordManager.Service.EncryptionService"/>). user_id is filled by the database.
/// </summary>
[Table("password")]
public class EncryptedPassword : BaseModel
{
    /// <summary>
    /// Client-generated (UUIDv7) so it is known before the INSERT and can be bound
    /// into the AES-GCM associated data.
    /// </summary>
    [PrimaryKey("id", true)]
    public Guid Id { get; set; }

    /// <summary>Base64 of the random 96-bit AES-GCM nonce.</summary>
    [Column("nonce")]
    public string Nonce { get; set; } = string.Empty;

    /// <summary>Base64 of the 128-bit AES-GCM authentication tag.</summary>
    [Column("tag")]
    public string Tag { get; set; } = string.Empty;

    /// <summary>Base64 of the ciphertext (encrypted JSON of <see cref="PasswordManager.Dto.PasswordEntry"/>).</summary>
    [Column("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>Ciphertext format version (see EncryptionService.CurrentVersion).</summary>
    [Column("version")]
    public short Version { get; set; }
}
