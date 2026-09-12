using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace PasswordManager.Core.Model;

[Table("password")]
public class EncryptedPassword : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("nonce")]
    public string Nonce { get; set; } = string.Empty;

    [Column("tag")]
    public string Tag { get; set; } = string.Empty;

    [Column("content")]
    public string Content { get; set; } = string.Empty;
}
