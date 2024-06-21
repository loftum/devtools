using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace WorkloadIdentity;

class Program
{
    private static readonly JsonSerializerOptions Pretty = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    
    public static async Task<int> Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        try
        {
            if (!TryGetInputs(args, out var tenantId, out var clientId, out var assertionToken))
            {
                Console.WriteLine($"Usage: {Process.GetCurrentProcess().ProcessName} <tenantId> <clientId> <assertionToken>");
                return 0;
            }

            var api = await OidcClient.GetAsync($"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration", cancellationToken);

            var tokenResponse = await api.GetWorkloadTokenAsync(clientId.ToString(), assertionToken, cancellationToken);
            Console.WriteLine(JsonSerializer.Serialize(tokenResponse, Pretty));
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 1;
        }
    }

    private static bool TryGetInputs(string[] args, out Guid tenantId, out Guid clientId, out string assertionToken)
    {
        tenantId = default;
        clientId = default;
        assertionToken = default;
        
        if (args.Length < 3)
        {
            return false;
        }

        if (!Guid.TryParse(args[0], out tenantId))
        {
            return false;
        }

        if (!Guid.TryParse(args[1], out clientId))
        {
            return false;
        }

        assertionToken = args[2];
        return true;
    }
}
