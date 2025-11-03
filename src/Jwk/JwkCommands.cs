using System.Security.Cryptography;
using System.Text;
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
    
    public static async Task GenRsa(string outFolder, string? filenamePrefix = null, int keySize = 4096,  string use = "sig")
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
            QI = Base64.UrlEncode(parameters.InverseQ)
        };

        var publicJwk = jwk.Public();

        filenamePrefix ??= jwk.Kid;

        var publicFileName = Path.Combine(outFolder, $"{filenamePrefix}.public.jwk.json"); 
        var privateFileName = Path.Combine(outFolder, $"{filenamePrefix}.full.jwk.json");
        
        await File.WriteAllTextAsync(publicFileName, JsonSerializer.Serialize(publicJwk, JsonOptions));
        await File.WriteAllTextAsync(privateFileName, JsonSerializer.Serialize(jwk, JsonOptions));
    }

    public static async Task FromSsh(string file)
    {
        if (!File.Exists(file))
        {
            throw new ArgumentException($"File {file} does not exist");
        }

        var text = await File.ReadAllLinesAsync(file);

        var raw = string.Join("", text.Where(l => !l.StartsWith("--") && !l.StartsWith("Comment")));

        var keyBytes = Convert.FromBase64String(raw);
        
        var pos = 0;
        

        var type = ReadString(); // should be "ssh-rsa"
        if (type != "ssh-rsa")
        {
            throw new Exception("Expected ssh-rsa, but got '{type}'");
        }
        var e = ReadBytes();
        var n = ReadBytes();

        var jwk = new
        {
            e = Base64Url(e),
            n = Base64Url(n),
            kty = "RSA",
            alg = "RS256",
            use = "sig",
            kid = Guid.NewGuid().ToString("N")
        };

        var jwkJson = JsonSerializer.Serialize(jwk, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(jwkJson);
        
        int ReadInt()
        {
            int val = (keyBytes[pos] << 24) | (keyBytes[pos + 1] << 16) | (keyBytes[pos + 2] << 8) | keyBytes[pos + 3];
            pos += 4;
            return val;
        }
        string ReadString()
        {
            int len = ReadInt();
            string s = Encoding.ASCII.GetString(keyBytes, pos, len);
            pos += len;
            return s;
        }
        byte[] ReadBytes()
        {
            int len = ReadInt();
            byte[] b = new byte[len];
            Array.Copy(keyBytes, pos, b, 0, len);
            pos += len;
            return b;
        }
        
        // Base64url encode
        string Base64Url(byte[] input)
        {
            return Convert.ToBase64String(input)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
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
    public string? QI { get; set; }

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