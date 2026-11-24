using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PasswordManager.Dto;

public class PasswordEntry
{
    /// <summary>
    /// Entry id (= password.id). Not part of the encrypted JSON; it is bound to the ciphertext
    /// through the AAD instead. <see cref="Guid.Empty"/> means a new, not yet saved entry.
    /// </summary>
    [JsonIgnore]
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    private static readonly JsonSerializerOptions jsonSerializerOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, jsonSerializerOptions);
    }

    public static PasswordEntry FromJson(string json)
    {
        return JsonSerializer.Deserialize<PasswordEntry>(json, jsonSerializerOptions)!;
    }
    
    public PasswordEntry() {}

    public PasswordEntry(PasswordEntry passwordEntry)
    {
        Id = passwordEntry.Id;
        Name = passwordEntry.Name;
        Url = passwordEntry.Url;
        Username = passwordEntry.Username;
        Password = passwordEntry.Password;
        Category = passwordEntry.Category;
        Notes = passwordEntry.Notes;
    }
    
}
