using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace PasswordManager.Model;

[Table("password")]
public class Password : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("content")]
    public string Content { get; set; } = string.Empty;
}
