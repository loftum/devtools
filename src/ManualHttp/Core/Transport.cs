using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace ManualHttp.Core
{
    public static class Transport
    {
        public static Stream GetStream(string host, int port, bool ssl, int sendTimeout = 10_000, int receiveTimeout = 10_000)
        {
            return ssl
                ? GetSecureStream(host, port, sendTimeout, receiveTimeout)
                : GetRegularStream(host, port, sendTimeout, receiveTimeout);
        }

        private static Stream GetSecureStream(string host, int port, int sendTimeout, int receiveTimeout)
        {
            var networkStream = GetRegularStream(host, port, sendTimeout, receiveTimeout);
            var stream = new SslStream(networkStream,
                false,
                AlwaysValid,
                null);
            stream.AuthenticateAsClient(host);
            return stream;
        }

        private static Stream GetRegularStream(string host, int port, int sendTimeout, int receiveTimeout)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                SendTimeout = sendTimeout,
                ReceiveTimeout = receiveTimeout
            };
            socket.Connect(host, port);
            return new NetworkStream(socket);
        }

        private static bool AlwaysValid(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }
    }
}