using System;
using System.Diagnostics;

namespace SocketClient;

class Program
{
    static int Main(string[] args)
    {
        Console.WriteLine("Socket client");

        if (args.Length < 2)
        {
            PrintUsage();
            return 0;
        }

        if (!int.TryParse(args[1], out var port))
        {
            Console.WriteLine($"Invalid port {args[1]}");
            PrintUsage();
            return -1;
        }

        using (var client = new EchoClient(args[0], port))
        {
            Console.CancelKeyPress += (s, e) => client.Stop();
            try
            {
                client.Start();
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
        Console.WriteLine($"{Process.GetCurrentProcess().ProcessName} <host> <port>");
    }
}