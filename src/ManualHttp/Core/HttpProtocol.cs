using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ManualHttp.Extensions;

namespace ManualHttp.Core
{
    public class HttpProtocol
    {
        public Encoding Encoding { get; set; } = new UTF8Encoding(false);
        public CookieStore CookieStore { get; set; } = new CookieStore();
        public RemoteCertificateValidationCallback RemoteCertificateValidationCallback { get; set; } = AlwaysValid;

        private static bool AlwaysValid(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }

        private Stream GetStream(Uri uri)
        {
            var client = new TcpClient(uri.Host, uri.Port) { ReceiveTimeout = 10000 };
            switch (uri.Scheme)
            {
                case "https":
                    return GetSecureStream(uri, client);
                default:
                    return client.GetStream();
            }
        }

        private Stream GetSecureStream(Uri uri, TcpClient client)
        {
            var stream = new SslStream(client.GetStream(),
                false,
                RemoteCertificateValidationCallback,
                null);
            stream.AuthenticateAsClient(uri.Host);
            return stream;
        }

        public async Task<HttpResponseMessage> SendAsync(Uri uri, HttpRequestMessage message)
        {
            using (var stream = GetStream(uri))
            {
                await stream.WriteRequestMessageAsync(message, Encoding);
                var responseMessage = await stream.ReadResponseMessageAsync(Encoding);

                foreach (var cookie in Cookie.ParseCookies(responseMessage.Headers.GetOrDefault("Set-Cookie")))
                {
                    if (string.IsNullOrWhiteSpace(cookie.Domain))
                    {
                        cookie.Domain = uri.Host;
                    }
                    CookieStore.Store(cookie);
                }

                return responseMessage;
            }
        }
    }
}