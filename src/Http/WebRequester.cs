using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace Http;

public static class WebRequester
{
    public static HttpRequestMessage Create(string method, string url)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), url);
        return request;
    }

    public static X509Certificate2 GetCert(string certFile, string certPass)
    {
        if (!File.Exists(certFile))
        {
            throw new ArgumentException($"Certificate file {certFile} does not exist", nameof(certFile));
        }
        Console.WriteLine($"With certificate: {certFile}");

        return new X509Certificate2(File.ReadAllBytes(certFile), certPass);
    }
}

public static class ResponseExtensions
{
    public static object GetFormattedContent(this HttpContent response)
    {
        using var stream = response.ReadAsStream();
        
        switch (response.Headers.ContentType?.MediaType)
        {
            case "application/json":
                return JsonSerializer.Deserialize<object>(stream);
            default:
            {
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }
    }
}