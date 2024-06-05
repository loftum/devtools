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
        
        Write(ca, outPath, password);
    }

    private static void Write(X509Certificate2 cert, OutPath path, string password)
    {
        Write(cert, null, path, password);
    }
    
    private static void Write(X509Certificate2 cert, X509Certificate2 ca, OutPath path, string password)
    {
        var certs = new[] {cert, ca}.Where(c => c != null); 
        
        if (path.IsDirectory)
        {
            var commonName = cert.GetCommonName();
            cert.WritePfx(path.Combine($"{commonName}.pfx"), password);
            certs.WritePem(path.Combine($"{commonName}.crt"));
            cert.WriteKey(path.Combine($"{commonName}.key"));
            ca?.WritePem(path.Combine("ca.crt"));
            return;
        }

        if (path.Parent.IsDirectory && !path.HasExtension)
        {
            cert.WritePfx(path.WithExtension("pfx"), password);
            certs.WritePem(path.WithExtension("crt"));
            cert.WriteKey(path.WithExtension("key"));
            ca?.WritePem(path.Parent.Combine("ca.crt"));
            return;
        }

        switch (path.Extension)
        {
            case "pfx":
                cert.WritePfx(path, password);
                return;
            case "pem":
            case "crt":
                certs.WritePem(path);
                cert.WriteKey(path.WithExtension("key"));
                return;
            default:
                throw new InvalidOperationException($"Invalid path {path}");
        }
    }

    public static void Create(CertType type,
        string commonName,
        string caPath,
        string outPath,
        string password = null,
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

        Write(cert, caCert, outPath, password);
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

    private static X509Certificate2 ReadCertFile(OutPath path)
    {
        switch (path.Extension)
        {
            case "pfx":
            {
                var bytes = File.ReadAllBytes(path);
                var cert = new X509Certificate2(bytes);
                return cert;
            }
            case "crt":
            case "pem":
            {
                var key = path.WithExtension("key");
                if (key.FileExists())
                {
                    return X509Certificate2.CreateFromPemFile(path, path.WithExtension("key"));
                }

                var content = File.ReadAllText(path);
                return X509Certificate2.CreateFromPem(content);
            }
            default:
            {
                var bytes = File.ReadAllBytes(path);
                var cert = new X509Certificate2(bytes);
                return cert;
            }
        }
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
                switch (extension)
                {
                    case X509EnhancedKeyUsageExtension e:
                    {
                        foreach (var usage in e.EnhancedKeyUsages)
                        {
                            builder.AppendLine($"- {usage.Value}: {usage.FriendlyName}");
                        }

                        break;
                    }
                }
            }
        }
            
        Console.WriteLine(builder);
    }
}

