using System.Text.Json.Serialization;

namespace PasswordManager.CustomExceptions;

public class AuthError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }
    
    [JsonPropertyName("error_code")]
    public string ErrorType { get; set; } = string.Empty;
    
    [JsonPropertyName("msg")]
    public string Message { get; set; } = string.Empty;
}