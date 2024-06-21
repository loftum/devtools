using System.Text.Json.Serialization;

namespace WorkloadIdentity;

public class TokenResponse : Dictionary<string, string>
{
    // [JsonPropertyName("access_token")]
    // public string AccessToken { get; init; }
}