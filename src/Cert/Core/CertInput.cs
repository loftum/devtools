using System;
using System.Security.Cryptography.X509Certificates;

namespace Cert.Core
{
    public enum CertType
    {
        Client,
        Server
    }
    
    
    public readonly struct CertInput
    {
        public CertType Type { get; init; }
        public string CommonName { get; init; }
        public string[] DnsNames { get; init; }
        public DateTimeOffset NotBefore { get; init; }
        public DateTimeOffset NotAfter { get; init; }
        public string CountryCode { get; init; }
        public string Organization { get; init; }
        public string[] OrganizationalUnits { get; init; }
        public X509Certificate2 Ca { get; init; }
    }
}