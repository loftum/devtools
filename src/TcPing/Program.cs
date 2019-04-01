using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace TcPing
{
    class Program
    {
        private const int ConnectionTimeout = 10060;

        static int Main(string[] args)
        {
            var arguments = TcPingArgs.Parse(args);
            if (arguments != null)
            {
                return Ping(arguments);
            }

            PrintUsage();
            return -1;
        }

        private static int Ping(TcPingArgs arg)
        {
            try
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    var connected = socket.Connect(arg.Host, arg.Port, arg.Timeout);
                    if (!connected)
                    {
                        throw new SocketException(ConnectionTimeout);
                    }
                    socket.Close();
                }

                Console.WriteLine("Pong!");
            }
            catch (SocketException ex)
            {
                Console.WriteLine("PANG!");
                switch (ex.SocketErrorCode)
                {
                    case SocketError.TimedOut:
                        Console.WriteLine($"Timeout ({arg.Timeout})");
                        break;
                    default:
                        Console.WriteLine($"{(int) (ex.SocketErrorCode)} {ex.SocketErrorCode}: {ex.Message}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("PANG!");
                Console.WriteLine(ex);
                return -1;
            }

            return 0;
        }

        private static void PrintUsage()
        {
            Console.WriteLine($"{Process.GetCurrentProcess().ProcessName} <host>:<port> [timeout ms=1000]");
        }
    }
}