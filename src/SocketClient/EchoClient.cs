using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketClient
{
    public class EchoClient : IDisposable
    {
        private readonly IPEndPoint _remoteEp;
        private readonly Socket _socket;
        private bool _running;
        public bool IsDisposed { get; private set; }

        public EchoClient(string host, int port)
        {
            var ipHostInfo = Dns.GetHostEntry(host);
            var ipAddress = ipHostInfo.AddressList[0];
            _remoteEp = new IPEndPoint(ipAddress, port);
            _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start()
        {
            _running = true;
            _socket.Connect(_remoteEp);

            Console.WriteLine("Enter message to send to server:");
            while (_running)
            {
                var echoBytes = new byte[1024];
                Console.Write("> ");
                var read = Console.ReadLine();
                read = $"{read}<EOF>";
                var bytes = Encoding.ASCII.GetBytes(read);
                var sent = _socket.Send(bytes);
                var received = _socket.Receive(echoBytes);
                var echo = Encoding.ASCII.GetString(echoBytes, 0, received);
                Console.WriteLine($"Got back: {echo}");
            }
        }

        public void Stop()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
            _running = false;
            _socket.Disconnect(true);
            _socket.Close();
            _socket.Dispose();
        }

        public void Dispose()
        {
            _socket?.Dispose();
        }
    }
}