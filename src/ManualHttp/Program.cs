using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using ManualHttp.Core;
using ManualHttp.Extensions;

namespace ManualHttp
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine($"{Process.GetCurrentProcess().ProcessName} <verb> <url>");
                Console.WriteLine($"{Process.GetCurrentProcess().ProcessName} send <url> <file> [encoding]");
                return 0;
            }
            
            try
            {
                Execute(args).Wait();
                return 0;
            }
            catch (AuthenticationException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Could not authenticate");
                throw;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ToString());
                return -1;
            }
        }

        private static Task Execute(string[] args)
        {
            var verb = args[0];
            switch (verb.ToLowerInvariant())
            {
                case "send":
                    return Send(new Uri(args[1]), args[2]);
                default:
                    var uri = new Uri(args[1]);
                    return Fire(verb, uri);
            }
        }

        private static async Task Fire(string verb, Uri uri)
        {
            Console.WriteLine($"Host: {uri.Host}");
            Console.WriteLine($"Port: {uri.Port}");
            Console.WriteLine($"Path: {uri.AbsolutePath}");
            Console.WriteLine();
            var request = new HttpRequest(uri)
            {
                Method = verb
            };

            using (var response = await request.GetResponseAsync())
            {
                Console.WriteLine(response.RawMessage);
                Console.WriteLine();
            }

            Console.WriteLine("Cookies:");
            foreach (var cookie in request.CookieStore.GetAllCookies())
            {
                Console.WriteLine(cookie);
            }
        }

        private static async Task Send(Uri uri, string filename, Encoding encoding = null)
        {
            Console.WriteLine($"Host: {uri.Host}");
            Console.WriteLine($"Port: {uri.Port}");
            Console.WriteLine();
            encoding = encoding ?? new UTF8Encoding(false);
            var content = string.Join("\r\n", File.ReadAllLines(filename, encoding).Concat(new []{"\r\n"}));
            Console.WriteLine("Request message:");
            Console.WriteLine("<");
            Console.WriteLine(content);
            Console.WriteLine(">");
            Console.WriteLine();
            using (var stream = uri.GetStream())
            {
                await stream.WriteTextAsync(content, encoding);
                var responseMessage = await stream.ReadResponseMessageAsync(encoding);
                Console.WriteLine();
                Console.WriteLine("Response:");
                Console.WriteLine(responseMessage);
            }
        }
    }
}
