using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TcPing
{
    public class TcPingArgs
    {
        public string Host { get; }
        public int Port { get; }
        public TimeSpan Timeout { get; }

        public TcPingArgs(string host, int port, TimeSpan timeout)
        {
            Host = host;
            Port = port;
            Timeout = timeout;
        }

        public static TcPingArgs Parse(string[] args)
        {
            if (args.Length < 1)
            {
                return null;
            }
            var values = args[0].Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (values.Length != 2)
            {
                return null;
            }
            var host = values[0];
            int port;
            if (!int.TryParse(values[1], out port))
            {
                return null;
            }
            TimeSpan timeout = TimeSpan.FromSeconds(10);
            int timeoutMillis;
            if (args.Length > 1 && int.TryParse(args[1], out timeoutMillis))
            {
                timeout = TimeSpan.FromMilliseconds(timeoutMillis);
            }
            return new TcPingArgs(host, port, timeout);
        }
    }

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
                Console.WriteLine(ex.Message);
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
            Console.WriteLine($"{Process.GetCurrentProcess().ProcessName} <host>:<port> [timeout ms=10000]");
        }
    }
}
