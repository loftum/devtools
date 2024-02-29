using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Cert;

internal static class CertificateExtensions
{
    public static void WritePfx(this X509Certificate2 cert, string path, string password)
    {
        File.WriteAllBytes(path, cert.Export(X509ContentType.Pfx, password));
    }

    public static void WritePem(this X509Certificate2 cert, string path)
    {
        var certificatePem = PemEncoding.Write("CERTIFICATE", cert.RawData);
        File.WriteAllText(path, new string(certificatePem));
    }
    
    public static void WritePem(this IEnumerable<X509Certificate2> certs, string path)
    {
        File.WriteAllText(path, string.Join('\n', certs.Select(c => PemEncoding.Write("CERTIFICATE", c.RawData))));
    }

    public static void WriteKey(this X509Certificate2 cert, string path)
    {
        if (!cert.HasPrivateKey)
        {
            return;
        }
        
        var key = (AsymmetricAlgorithm)cert.GetRSAPrivateKey() ?? cert.GetECDsaPrivateKey();
        if (key == null)
        {
            return;
        }
        byte[] privKeyBytes = key.ExportPkcs8PrivateKey();
        char[] privKeyPem = PemEncoding.Write("PRIVATE KEY", privKeyBytes);
        File.WriteAllText(path, new string(privKeyPem));
    }

    public static string GetCommonName(this X509Certificate2 cert)
    {
        return cert.Subject.GetSubjectProperty("CN");
    }
    
    public static string GetOrganization(this X509Certificate2 cert)
    {
        return cert.Subject.GetSubjectProperty("O");
    }

    private static string GetSubjectProperty(this string subject, string property)
    {
        foreach (var (key, value) in subject.Split(',').Select(Parse))
        {
            if (property == key)
            {
                return value;
            }
        }

        return default;
    }

    private static (string, string) Parse(string value)
    {
        var parts = value.Split('=');
        if (parts.Length < 2)
        {
            return (value, null);
        }

        return (parts[0].Trim(), parts[1].Trim());
    }
}