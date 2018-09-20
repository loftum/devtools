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
                ExecuteAsync(args).Wait();
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

        private static Task ExecuteAsync(string[] args)
        {
            var verb = args[0];
            switch (verb.ToLowerInvariant())
            {
                case "send":
                    return SendAsync(new Uri(args[1]), args[2]);
                default:
                    var uri = new Uri(args[1]);
                    return FireAsync(verb, uri);
            }
        }

        private static async Task FireAsync(string verb, Uri uri)
        {
            Console.WriteLine($"Host: {uri.Host}");
            Console.WriteLine($"Port: {uri.Port}");
            Console.WriteLine($"Path: {uri.AbsolutePath}");
            Console.WriteLine();
            var protocol = new HttpProtocol();

            var request = new HttpRequestMessage
            {
                RequestLine = new RequestLine
                {
                    HttpVersion = "HTTP/1.1",
                    Method = verb,
                    RequestUri = uri.PathAndQuery
                },
                Headers =
                {
                    ["Host"] = uri.Host,
                    ["Connection"] = "keep-alive",
                    ["User-Agent"] = "Casio Typewriter",
                    ["Accept"] = "*/*"
                }
            };
            Console.WriteLine("<Request>");
            Console.WriteLine(request);
            Console.WriteLine("</Request>");
            Console.WriteLine();
            var response = await protocol.SendAsync(uri, request);
            Console.WriteLine("<Response>");
            Console.WriteLine(response);
            Console.WriteLine("</Response>");
            Console.WriteLine();
            Console.WriteLine("<Cookies>");
            foreach (var cookie in protocol.CookieStore.GetAllCookies())
            {
                Console.WriteLine(cookie);
            }
            Console.WriteLine("</Cookies>");
        }

        private static async Task SendAsync(Uri uri, string filename, Encoding encoding = null)
        {
            Console.WriteLine($"Host: {uri.Host}");
            Console.WriteLine($"Port: {uri.Port}");
            Console.WriteLine();
            encoding = encoding ?? new UTF8Encoding(false);
            var request = string.Join("\r\n", File.ReadAllLines(filename, encoding).Concat(new []{"\r\n"}));
            Console.WriteLine("<Request>");
            Console.WriteLine(request);
            Console.WriteLine("</Request>");
            Console.WriteLine();
            using (var stream = uri.GetStream())
            {
                await stream.WriteTextAsync(request, encoding);
                var responseMessage = await stream.ReadResponseMessageAsync(encoding);
                Console.WriteLine();
                Console.WriteLine("<Response>");
                Console.WriteLine(responseMessage);
                Console.WriteLine("</Response>");
            }
        }
    }
}
