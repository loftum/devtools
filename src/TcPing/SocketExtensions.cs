using System;
using System.Net;
using System.Net.Sockets;

namespace TcPing
{
    public static class SocketExtensions
    {
        public static bool Connect(this Socket socket, string host, int port, TimeSpan timeout)
        {
            return IPAddress.TryParse(host, out var ip)
                ? socket.ConnectToIp(ip, port, timeout)
                : socket.ConnectToDns(host, port, timeout);
        }

        private static bool ConnectToDns(this Socket socket, string host, int port, TimeSpan timeout)
        {
            var entry = Dns.GetHostEntry(host);
            Print(entry);
            var handle = socket.BeginConnect(host, port, null, null);
            var connected = handle.AsyncWaitHandle.WaitOne(timeout, true);
            return connected;
        }

        private static void Print(IPHostEntry entry)
        {
            switch (entry.AddressList.Length)
            {
                case 0:
                    Console.WriteLine($"No addresses for {entry.HostName}");
                    break;
                case 1:
                    Console.WriteLine($"Remote address: {entry.AddressList[0]}");
                    break;
                default:
                    Console.WriteLine("Remote addresses:");
                    foreach (var address in entry.AddressList)
                    {
                        Console.WriteLine($" - {address}");
                    }
                    break;
            }
        }

        private static bool ConnectToIp(this Socket socket, IPAddress ip, int port, TimeSpan timeout)
        {
            var handle = socket.BeginConnect(ip, port, null, null);
            var connected = handle.AsyncWaitHandle.WaitOne(timeout, true);
            return connected;
        }
    }
}