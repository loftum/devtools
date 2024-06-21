using System.Text.Json.Serialization;

namespace WorkloadIdentity;

public class OpenIdConnectConfiguration
{
    [JsonPropertyName("token_endpoint")]
    public string TokenEndpoint { get; init; }
}