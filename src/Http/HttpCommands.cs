using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading.Tasks;
using Http.Logging;

namespace Http;

public class HttpCommands
{
    private static readonly ILogger Logger = Log.For<HttpCommands>();

    public static void LogLevel(LogLevel level)
    {
        Settings.Instance.LogLevel = level;
    }

    public static Task Get(string url, string host = null, string version = null, string accept = null, string username = null, string password = null, string certFile = null, string certPass = null)
    {
        return Send("GET", url, host, version, accept, username, password, certFile, certPass);
    }

    public static async Task Send(string method, string url, string host = null, string version = null, string accept = null, string username = null, string password = null, string certFile = null, string certPass = null)
    {
        var request = WebRequester.Create(method, url);
        if (version != null)
        {
            request.Version = new Version(version);
        }
        if (username != null || password != null)
        {
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(Authorization.Basic(username, password));
        }
        
        if (host != null)
        {
            request.Headers.Host = host;
        }

        if (request.Headers.Any())
        {
            Logger.Debug("Request headers:");
            foreach (var (key, value) in request.Headers)
            {
                Logger.Debug($"{key}: {value}");
            }
        }

        var handler = new HttpClientHandler();
        if (certFile != null)
        {
            handler.ClientCertificates.Add(WebRequester.GetCert(certFile, certPass));
            handler.CheckCertificateRevocationList = false;
            handler.ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true;
        }
        var client = new HttpClient(handler);


        try
        {
            using var response = await client.SendAsync(request);
            Logger.Important($"StatusCode: {response.StatusCode}");
            if (response.Headers.Any())
            {
                Logger.Debug("Headers:");
                foreach (var (key, value) in response.Headers)
                {
                    Logger.Debug($"{key}:{value}");
                }
            }
        
            Logger.Normal("Content:");
            Logger.Normal(response.Content.GetFormattedContent());
        }
        catch (Exception ex)
        {
            var b = ex.GetBaseException();
            switch (b)
            {
                case SocketException s:
                {
                    Console.WriteLine($"SocketException: {s.Message}");
                    Console.WriteLine($"Error code: {s.ErrorCode}");
                    Console.WriteLine($"Socket error {(int)s.SocketErrorCode}: {s.SocketErrorCode}");
                    Console.WriteLine(s.StackTrace);
                    break;
                }
            }

            throw;
            
        }
    }
}