using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Cert.Core;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace Cert;

public class CertificateCommands
{
    public static void CreateCa(string commonName,
        string outPath,
        DateTimeOffset? notBefore = null,
        DateTimeOffset? notAfter = null,
        string password = null,
        string countryCode = "NO",
        string organization = null,
        string[] organizationalUnits = null)
    {

        var now = DateTimeOffset.UtcNow;
        var input = new CaInput
        {
            Organization = organization,
            CommonName = commonName,
            CountryCode = countryCode,
            NotBefore = notBefore ?? now,
            NotAfter = notAfter ?? now.AddYears(10),
            Password = password,
            OrganizationalUnits = organizationalUnits
        };
            
        using var ca = CertificateGenerator.CreateCa(input);
            
        File.WriteAllBytes(outPath, ca.Export(X509ContentType.Pfx, password));
    }

    public static void Create(CertType type,
        string commonName,
        string caPath,
        string outPath,
        DateTimeOffset? notBefore = null,
        DateTimeOffset? notAfter = null,
        string[] dnsNames = null,
        string countryCode = "NO",
        string organization = null,
        string[] organizationalUnits = null)
    {
        var caCert = ReadCertFile(caPath);

        var now = DateTimeOffset.UtcNow;
        var input = new CertInput
        {
            Type = type,
            Organization = organization,
            CommonName = commonName,
            NotBefore = notBefore ?? now,
            NotAfter = notAfter ?? now.AddYears(1),
            CountryCode = countryCode,
            DnsNames = dnsNames,
            OrganizationalUnits = organizationalUnits,
            Ca = caCert
        };
            
        using var cert = CertificateGenerator.CreateCertificate(input);
            
        File.WriteAllBytes($"{outPath}.pfx", cert.Export(X509ContentType.Pfx));
            
        var certificatePem = PemEncoding.Write("CERTIFICATE", cert.RawData);
            
            
        File.WriteAllText($"{outPath}.pem", new string(certificatePem) + "\n" + new string(PemEncoding.Write("CERTIFICATE", caCert.RawData)));
            
        var key = cert.GetRSAPrivateKey();
        byte[] pubKeyBytes = key.ExportSubjectPublicKeyInfo();
        byte[] privKeyBytes = key.ExportPkcs8PrivateKey();
        char[] pubKeyPem = PemEncoding.Write("PUBLIC KEY", pubKeyBytes);
        char[] privKeyPem = PemEncoding.Write("PRIVATE KEY", privKeyBytes);
            
        File.WriteAllText($"{outPath}.key", new string(privKeyPem));
    }

    public static void Export(string inputPath, string outputPath)
    {
        using var cert = ReadCertFile(inputPath);
        var bytes = cert.Export(X509ContentType.Cert);
        File.WriteAllBytes(outputPath, bytes);
    }

    public static void ExportPem(string inputPath, string certPath, string keyPath)
    {
        using var cert = ReadCertFile(inputPath);
        var certificatePem = PemEncoding.Write("CERTIFICATE", cert.RawData);
            
            
        var key = cert.GetRSAPrivateKey();
            
            
        byte[] pubKeyBytes = key.ExportSubjectPublicKeyInfo();
        byte[] privKeyBytes = key.ExportRSAPrivateKey();// key.ExportPkcs8PrivateKey();
        char[] pubKeyPem = PemEncoding.Write("PUBLIC KEY", pubKeyBytes);
        char[] privKeyPem = PemEncoding.Write("PRIVATE KEY", privKeyBytes);

        var privateKey = GetPEMPrivateKey(cert);
            
        File.WriteAllText(certPath, new string(certificatePem));
            
        File.WriteAllBytes(keyPath, privateKey);
    }
        
    private static byte[] GetPEMPrivateKey(X509Certificate2 pfx)
    {
        var pair = DotNetUtilities.GetRsaKeyPair(pfx.GetRSAPrivateKey());

        using var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, Encoding.Default, 1024, true))
        {
            var pemWriter = new PemWriter(writer);
                
            pemWriter.WriteObject(pair.Private);
            writer.Flush();
        }

        stream.Seek(0, SeekOrigin.Begin);
        return stream.ToArray();
    }

    private static X509Certificate2 ReadCertFile(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var cert = new X509Certificate2(bytes);
        return cert;
    }

    public static void Read(string file)
    {
        using var cert = ReadCertFile(file);

            
        var builder = new StringBuilder()
                .AppendLine($"Friendly name: {cert.FriendlyName}")
                .AppendLine($"Thumbprint: {cert.Thumbprint}")
                .AppendLine($"Version: {cert.Version}")
                .AppendLine($"Not before: {cert.NotBefore}")
                .AppendLine($"Not after: {cert.NotAfter}")
                .AppendLine($"Serial number: {cert.SerialNumber}")
                .AppendLine($"Issuer: {cert.IssuerName.Format(true)}")
                .AppendLine($"Subject name: {cert.SubjectName.Format(true)}")
                .AppendLine($"Has private key: {cert.HasPrivateKey}")
            ;

        if (cert.Extensions.Any())
        {
            builder.AppendLine()
                .AppendLine("Extensions:");
            foreach (var extension in cert.Extensions.Where(e => e.Oid != null))
            {
                builder.AppendLine($"{extension.Oid?.Value} ({extension.Oid?.FriendlyName})");
            }
        }
            
        Console.WriteLine(builder);
    }
}