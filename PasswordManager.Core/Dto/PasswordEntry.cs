using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PasswordManager.Core.Dto;

public class PasswordEntry
{
    [JsonIgnore]
    public long Id { get; set; }
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
}
