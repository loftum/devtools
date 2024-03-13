using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using Http.Logging;

namespace Http;

public class HttpCommands
{
    private static readonly ILogger Logger = Log.For<HttpCommands>();

    public static void LogLevel(LogLevel level)
    {
        Settings.Instance.LogLevel = level;
    }

    public static void Get(string url, string host = null, string accept = null, string username = null, string password = null, string certFile = null, string certPass = null)
    {
        Send("GET", url, host, accept, username, password, certFile, certPass);
    }

    public static void Send(string method, string url, string host = null, string accept = null, string username = null, string password = null, string certFile = null, string certPass = null)
    {
        var request = WebRequester.Create(method, url);
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
            using var response = client.Send(request);
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