using System;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Cert.Core
{
    public class CertificateGenerator
    {
        public static X509Certificate2 CreateCa(CaInput input)
        {
            //var sanBuilder = new SubjectAlternativeNameBuilder();

            var dn = new StringBuilder();
            
            dn.Append("CN=\"" + input.CommonName.Replace("\"", "\"\"") + "\"");
            
            if (input.OrganizationalUnits != null)
            {
                foreach (var ou in input.OrganizationalUnits)
                {
                    dn.Append(",OU=\"" + ou.Replace("\"", "\"\"") + "\"");
                }    
            }
            
            dn.Append(",O=\"" + input.Organization.Replace("\"", "\"\"") + "\"");
            dn.Append(",C=" + input.CountryCode.ToUpper());

            var distinguishedName = new X500DistinguishedName(dn.ToString());

            using var rsa = RSA.Create(4096);
            var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            const X509KeyUsageFlags usages = X509KeyUsageFlags.KeyCertSign |
                                             X509KeyUsageFlags.CrlSign;

            request.CertificateExtensions.Add(new X509KeyUsageExtension(usages, true));

            request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, true, 1, true));

            var subjectKeyExtension = new X509SubjectKeyIdentifierExtension(request.PublicKey, false);
            
            request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));
            
            // set the AuthorityKeyIdentifier. There is no built-in 
            // support, so it needs to be copied from the Subject Key 
            // Identifier of the signing certificate and massaged slightly.
            // AuthorityKeyIdentifier is "KeyID="
            var issuerSubjectKey = subjectKeyExtension.RawData;
            var segment = new ArraySegment<byte>(issuerSubjectKey, 2, issuerSubjectKey.Length - 2);
            var authorityKeyIdentifer = new byte[segment.Count + 4];
            // these bytes define the "KeyID" part of the AuthorityKeyIdentifer
            authorityKeyIdentifer[0] = 0x30;
            authorityKeyIdentifer[1] = 0x16;
            authorityKeyIdentifer[2] = 0x80;
            authorityKeyIdentifer[3] = 0x14;
            segment.CopyTo(authorityKeyIdentifer, 4);
            request.CertificateExtensions.Add(new X509Extension("2.5.29.35", authorityKeyIdentifer, false));

            //request.CertificateExtensions.Add(sanBuilder.Build());

            var certificate = request.CreateSelfSigned(input.NotBefore, input.NotAfter);
            
            return certificate;
        }
        
        
        
        public static X509Certificate2 CreateCertificate(CertInput input)
        {
            var sanBuilder = new SubjectAlternativeNameBuilder();
            if (input.DnsNames == null)
            {
                sanBuilder.AddIpAddress(IPAddress.Loopback);
                sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
                sanBuilder.AddDnsName("localhost");
                sanBuilder.AddDnsName(Environment.MachineName);
            }
            else
            {
                foreach(var dnsName in input.DnsNames)
                {
                    sanBuilder.AddDnsName(dnsName);
                }
            }

            var dn = new StringBuilder();
            
            dn.Append("CN=\"" + input.CommonName.Replace("\"", "\"\"") + "\"");
            
            if (input.OrganizationalUnits != null)
            {
                foreach (var ou in input.OrganizationalUnits)
                {
                    dn.Append(",OU=\"" + ou.Replace("\"", "\"\"") + "\"");
                }    
            }
            
            dn.Append(",O=\"" + input.Organization.Replace("\"", "\"\"") + "\"");
            dn.Append(",C=" + input.CountryCode.ToUpper());

            var distinguishedName = new X500DistinguishedName(dn.ToString());

            using var rsa = RSA.Create(4096);
            var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            const X509KeyUsageFlags usages = X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature;

            request.CertificateExtensions.Add(new X509KeyUsageExtension(usages, false));

            var oid = input.Type == CertType.Client ? "1.3.6.1.5.5.7.3.2" : "1.3.6.1.5.5.7.3.1";

            request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid(oid) }, false));

            request.CertificateExtensions.Add(sanBuilder.Build());
            
            Span<byte> serialNumber = stackalloc byte[8];
            RandomNumberGenerator.Fill(serialNumber);
            using var certificate = request.Create(input.Ca, input.NotBefore, input.NotAfter, serialNumber);
            
            // CopyWithPrivateKey will include private key.
            return certificate.CopyWithPrivateKey(rsa);
        }
    }
}