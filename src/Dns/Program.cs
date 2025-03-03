using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace Dns;

class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length < 1)
        {
            PrintUsage();
            return 1;
        }

        try
        {
            var entry = await System.Net.Dns.GetHostEntryAsync(args[0]);


            if (!string.IsNullOrWhiteSpace(entry.HostName))
            {
                Console.WriteLine($"Hostname: {entry.HostName}");
            }

            if (entry.Aliases.Length > 0)
            {
                Console.WriteLine("Aliases:");
                foreach (var alias in entry.Aliases)
                {
                    Console.WriteLine($"- {alias}");
                }

                Console.WriteLine();
            }

            if (entry.AddressList.Length > 0)
            {
                Console.WriteLine("Addresses:");
                foreach (var address in entry.AddressList)
                {
                    Console.WriteLine($"- {address}");
                }
            }

            return 0;
        }
        catch (SocketException e)
        {
            switch (e.SocketErrorCode)
            {
                case SocketError.HostNotFound:
                    Console.WriteLine($"Host not found: {args[0]}");
                    return 1;
                default:
                    Console.WriteLine($"SocketException: {(int)e.SocketErrorCode} {e.SocketErrorCode}: {e.Message}");
                    return 1;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 1;
        }
    }

    private static void PrintUsage()
    {
        var builder = new StringBuilder()
            .AppendLine("Usage:")
            .AppendLine($"{Process.GetCurrentProcess().ProcessName} hostname");
        Console.WriteLine(builder.ToString());
    }
}