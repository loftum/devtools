using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Jwk.Encodings;

namespace Jwk;

public class JwkCommands
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
    
    public static async Task GenRsa(string outFolder, int keySize = 4096,  string use = "sig")
    {
        if (File.Exists(outFolder))
        {
            throw new ArgumentException($"{nameof(outFolder)} must be a folder", nameof(outFolder));
        }

        Directory.CreateDirectory(outFolder);
        
        using var rsa = RSA.Create(keySize);
        var parameters = rsa.ExportParameters(true);
        var jwk = new RsaJwk
        {
            Kid = Guid.NewGuid().ToString("N"),
            Kty = "RSA",
            Alg = "RS256",
            Use = use,
            N = Base64.UrlEncode(parameters.Modulus),
            E = Base64.UrlEncode(parameters.Exponent),
            P = Base64.UrlEncode(parameters.P),
            Q = Base64.UrlEncode(parameters.Q),
            D = Base64.UrlEncode(parameters.D),
            DP = Base64.UrlEncode(parameters.DP),
            DQ = Base64.UrlEncode(parameters.DQ),
        };

        var publicJwk = jwk.Public();

        var publicFileName = Path.Combine(outFolder, $"{jwk.Kid}.jwk.json"); 
        var privateFileName = Path.Combine(outFolder, $"{jwk.Kid}.private.jwk.json");
        
        await File.WriteAllTextAsync(publicFileName, JsonSerializer.Serialize(publicJwk, JsonOptions));
        await File.WriteAllTextAsync(privateFileName, JsonSerializer.Serialize(jwk, JsonOptions));
    }
}

public record Jwk
{
    public string? Kty { get; set; }
    public string? Alg { get; set; }
    public string? Use { get; set; }
    public string? Kid { get; set; }

    // Cert
    public string? X5t { get; set; }
    public string[] X5c { get; set; }
}

public record RsaPublicJwk : Jwk
{
    // RSA
    public string? E { get; set; }
    public string? N { get; set; }
}

public record RsaJwk : RsaPublicJwk
{
    // RSA private
    public string? P { get; set; }
    public string? Q { get; set; }
    public string? D { get; set; }
    public string? DP { get; set; }
    public string? DQ { get; set; }

    public RsaPublicJwk Public()
    {
        return new RsaPublicJwk
        {
            Alg = Alg,
            Kid = Kid,
            Kty = Kty,
            Use = Use,
            E = E,
            N = N,
            X5c = X5c,
            X5t = X5t,
        };
    }
}

public record EcPublicJwk : Jwk
{
    // EC
    public string? X { get; set; }
    public string? Y { get; set; }
    public string? Crv { get; set; }
}