using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Text;
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
                Execute(args);
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

        private static void Execute(string[] args)
        {
            var verb = args[0];
            switch (verb.ToLowerInvariant())
            {
                case "send":
                    Send(new Uri(args[1]), args[2]);
                    break;
                default:
                    var uri = new Uri(args[1]);
                    Fire(verb, uri);
                    break;
            }
        }

        private static void Fire(string verb, Uri uri)
        {
            Console.WriteLine($"Host: {uri.Host}");
            Console.WriteLine($"Port: {uri.Port}");
            Console.WriteLine($"Path: {uri.AbsolutePath}");
            Console.WriteLine();
            var request = new HttpRequest(uri)
            {
                Method = verb
            };
            Console.WriteLine("Rquest message");
            Console.WriteLine(request.GetRequestMessage());
            Console.WriteLine();

            using (var response = request.GetResponse())
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

        private static void Send(Uri uri, string filename, Encoding encoding = null)
        {
            Console.WriteLine($"Host: {uri.Host}");
            Console.WriteLine($"Port: {uri.Port}");
            Console.WriteLine();
            encoding = encoding ?? Encoding.UTF8;
            var content = string.Join("\r\n", File.ReadAllLines(filename, encoding).Concat(new []{"\r\n"}));
            Console.WriteLine("Request message:");
            Console.WriteLine("<");
            Console.WriteLine(content);
            Console.WriteLine(">");
            Console.WriteLine();
            using (var stream = uri.GetStream())
            {
                stream.WriteText(content, encoding);
                var responseMessage = stream.ReadResponseMessage(encoding);
                Console.WriteLine();
                Console.WriteLine("Response:");
                Console.WriteLine(responseMessage);
            }
        }
    }
}
