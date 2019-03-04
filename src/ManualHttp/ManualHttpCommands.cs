using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManualHttp.Core;
using ManualHttp.Extensions;

namespace ManualHttp
{
    public class ManualHttpCommands
    {
        public static void Send(string file, string addressOrIp, bool ssl = false, int port = 0, int sendTimeout = 10_000, int receiveTimeout = 10_000, Encoding encoding = null)
        {
            SendAsync(file, addressOrIp, ssl, port, sendTimeout, receiveTimeout, encoding).Wait();
        }

        private static async Task SendAsync(string file, string addressOrIp, bool ssl = false, int port = 0, int sendTimeout = 10_000, int receiveTimeout = 10_000, Encoding encoding = null)
        {
            port = port == 0 ? DefaultPortFor(ssl) : port;

            Console.WriteLine($"Host: {addressOrIp}");
            Console.WriteLine($"Port: {port}");
            Console.WriteLine();
            encoding = encoding ?? new UTF8Encoding(false);
            var request = string.Join("\r\n", File.ReadAllLines(file, encoding)
                .Where(l => !l.StartsWith("#"))
                .Concat(new[] { "\r\n" }));
            Console.WriteLine("<Request>");
            Console.WriteLine(request);
            Console.WriteLine("</Request>");
            Console.WriteLine();
            using (var stream = Transport.GetStream(addressOrIp, port, ssl, sendTimeout, receiveTimeout))
            {
                await stream.WriteTextAsync(request, encoding);
                var responseMessage = await stream.ReadResponseMessageAsync(encoding);
                Console.WriteLine();
                Console.WriteLine("<Response>");
                Console.WriteLine(responseMessage);
                Console.WriteLine("</Response>");
            }
        }

        private static int DefaultPortFor(bool ssl)
        {
            return ssl ? 443 : 80;
        }

        public static void Fire(string verb, string url)
        {
            FireAsync(verb, url).Wait();
        }

        private static async Task FireAsync(string verb, string url)
        {
            var uri = new Uri(url);
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
    }
}