using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace ManualHttp.Core
{
    public static class Transport
    {
        public static Stream GetStream(this Uri uri)
        {
            switch (uri.Scheme)
            {
                case "https":
                    return GetSecureStream(uri);
                default:
                    return GetClient(uri).GetStream();
            }
        }

        private static TcpClient GetClient(Uri uri)
        {
            return new TcpClient(uri.Host, uri.Port) {ReceiveTimeout = 10000};
        }

        private static Stream GetSecureStream(Uri uri)
        {
            var client = GetClient(uri);
            var stream = new SslStream(client.GetStream(),
                false,
                AlwaysValid,
                null);
            stream.AuthenticateAsClient(uri.Host);
            return stream;
        }

        private static bool AlwaysValid(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }
    }
}