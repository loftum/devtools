namespace Cert.Core;

public readonly struct CaInput
{
    public string CommonName { get; init; }
    public DateTimeOffset NotBefore { get; init; }
    public DateTimeOffset NotAfter { get; init; }
    public string CountryCode { get; init; }
    public string Organization { get; init; }
    public string[] OrganizationalUnits { get; init; }
    public string Password { get; init; }
}