using System;
using System.Diagnostics;
using System.Net;
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
                    var entry = Dns.GetHostEntry(arg.Host);
                    Print(entry);
                    var handle = socket.BeginConnect(arg.Host, arg.Port, null, null);
                    var connected = handle.AsyncWaitHandle.WaitOne(arg.Timeout, true);
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

        private static void PrintUsage()
        {
            Console.WriteLine($"{Process.GetCurrentProcess().ProcessName} <host>:<port> [timeout ms=1000]");
        }
    }
}