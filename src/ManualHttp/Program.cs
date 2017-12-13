using System;
using System.Diagnostics;
using System.Security.Authentication;
using ManualHttp.Core;

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
                return 0;
            }
            var verb = args[0];
            var uri = new Uri(args[1]);
            Console.WriteLine($"Host: {uri.Host}");
            Console.WriteLine($"Port: {uri.Port}");
            Console.WriteLine($"Path: {uri.AbsolutePath}");
            Console.WriteLine();

            try
            {
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
    }
}
