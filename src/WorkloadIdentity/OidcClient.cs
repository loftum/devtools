using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace WorkloadIdentity;

public class OidcClient
{
    private readonly HttpClient _client;
    private readonly OpenIdConnectConfiguration _configuration;

    public OidcClient(HttpClient client, OpenIdConnectConfiguration configuration)
    {
        _client = client;
        _configuration = configuration;
    }

    public static async Task<OidcClient> GetAsync(string openIdConfigurationUrl, CancellationToken cancellationToken = default)
    {
        var client = new HttpClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, openIdConfigurationUrl)
        {
            Headers =
            {
                Accept = { MediaTypeWithQualityHeaderValue.Parse("application/json") }
            }
        };

        using var response = await client.SendAsync(request, cancellationToken);
        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                var configuration = await response.Content.ReadFromJsonAsync<OpenIdConnectConfiguration>(cancellationToken: cancellationToken);
                return new OidcClient(client, configuration);
            default:
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new Exception($"Could not get oidc config:\nStatusCode:{response.StatusCode}\n{body}");
        }
    }

    public async Task<object> GetWorkloadTokenAsync(string clientId, string clientAssertion, CancellationToken cancellationToken = default)
    {
        var values = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_assertion"] = clientAssertion,
            ["client_assertion_type"] = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer", 
            ["scope"] = "https://graph.microsoft.com/.default"
        };
        
        using var request = new HttpRequestMessage(HttpMethod.Post, _configuration.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(values),
            Headers =
            {
                Accept = { MediaTypeWithQualityHeaderValue.Parse("application/json") }
            }
        };

        using var response = await _client.SendAsync(request, cancellationToken);

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                var tokenResponse = await response.Content.ReadFromJsonAsync<object>(cancellationToken);
                return tokenResponse;
            default:
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new Exception($"Could not get workload token:\nStatusCode:{response.StatusCode}\n{body}");
        }
    }
}