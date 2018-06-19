using System;
using System.Diagnostics;
using System.Linq;

namespace SocketServer
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("Socket server");
            if (!args.Any())
            {
                PrintUsage();
                return 0;
            }

            if (!int.TryParse(args[0], out var port))
            {
                Console.WriteLine($"{args[0]} is not a valid port");
                PrintUsage();
                return -1;
            }

            using (var server = new EchoServer(port))
            {
                Console.CancelKeyPress += (s, e) => server.Stop();
                try
                {
                    server.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return -1;
                }
            }

            return 0;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine($"{Process.GetCurrentProcess().ProcessName} <port>");
        }
    }
}
