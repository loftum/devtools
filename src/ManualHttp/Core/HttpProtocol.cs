using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ManualHttp.Extensions;

namespace ManualHttp.Core
{
    public class TransportData
    {
        public Uri Uri { get; set; }
        public SslProtocols SslProtocols { get; set; }
        public bool CheckCertificateRevocation { get; set; }
        public RemoteCertificateValidationCallback RemoteCertificateValidationCallback { get; set; } = AlwaysValid;
        
        private static bool AlwaysValid(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }
    }

    internal static class TcpClientExtensions
    {
        public static SslStream GetSecureStream(this TcpClient client, TransportData transport)
        {
            var stream = new SslStream(client.GetStream(),
                false,
                transport.RemoteCertificateValidationCallback,
                null);
            stream.AuthenticateAsClient(transport.Uri.Host, new X509CertificateCollection(), transport.SslProtocols, transport.CheckCertificateRevocation);
            Console.WriteLine($"SSL Protocol: {stream.SslProtocol}");
            return stream;
        }
    }
    
    public class HttpProtocol
    {
        public Encoding Encoding { get; set; } = new UTF8Encoding(false);
        public CookieStore CookieStore { get; set; } = new CookieStore();

        private static bool AlwaysValid(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }

        private static Stream GetStream(TransportData transport)
        {
            var client = new TcpClient(transport.Uri.Host, transport.Uri.Port) { ReceiveTimeout = 10000 };
            switch (transport.Uri.Scheme)
            {
                case "https":
                    return client.GetSecureStream(transport);
                default:
                    return client.GetStream();
            }
        }

        public async Task<HttpResponseMessage> SendAsync(TransportData transport, HttpRequestMessage message)
        {
            using (var stream = GetStream(transport))
            {
                await stream.WriteRequestMessageAsync(message, Encoding);
                var responseMessage = await stream.ReadResponseMessageAsync(Encoding);

                foreach (var cookie in Cookie.ParseCookies(responseMessage.Headers.GetOrDefault("Set-Cookie")))
                {
                    if (string.IsNullOrWhiteSpace(cookie.Domain))
                    {
                        cookie.Domain = transport.Uri.Host;
                    }
                    CookieStore.Store(cookie);
                }

                return responseMessage;
            }
        }
    }
}