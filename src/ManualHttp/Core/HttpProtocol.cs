using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;
using ManualHttp.Extensions;

namespace ManualHttp.Core
{
    public class HttpProtocol
    {
        private static Stream GetStream(HttpRequest request)
        {
            var client = new TcpClient(request.Uri.Host, request.Uri.Port) { ReceiveTimeout = 10000 };
            switch (request.Uri.Scheme)
            {
                case "https":
                    return GetSecureStream(request, client);
                default:
                    return client.GetStream();
            }
        }

        private static Stream GetSecureStream(HttpRequest request, TcpClient client)
        {
            var stream = new SslStream(client.GetStream(),
                false,
                request.RemoteCertificateValidationCallback,
                null);
            stream.AuthenticateAsClient(request.Uri.Host);
            return stream;
        }

        public async Task<HttpResponse> GetResponseAsync(HttpRequest request)
        {
            var stream = GetStream(request);
            var requestMessage = request.GetRequestMessage();
            Console.WriteLine("Request:");
            Console.WriteLine(requestMessage);
            Console.WriteLine("<end>");

            await stream.WriteRequestMessageAsync(requestMessage, request.Encoding);
            var responseMessage = await stream.ReadResponseMessageAsync(request.Encoding);
            Console.WriteLine("Response:");
            Console.WriteLine(responseMessage);
            Console.WriteLine("<end>");

            foreach (var cookie in Cookie.ParseCookies(responseMessage.Headers.GetOrDefault("Set-Cookie")))
            {
                if (string.IsNullOrWhiteSpace(cookie.Domain))
                {
                    cookie.Domain = request.Uri.Host;
                }
                request.CookieStore.Store(cookie);
            }

            //switch (responseMessage.StatusLine.StatusCode)
            //{
            //    case "302":
            //        if (!request.OnRedirect(responseMessage))
            //        {
            //            break;
            //        }
            //        var location = responseMessage.Headers["Location"];
            //        var redirect = new HttpRequestMessage
            //        {
            //            RequestLine = new RequestLine
            //            {
            //                Method = "GET",
            //                RequestUri = location
            //            },
            //            Headers = new Dictionary<string, string>
            //            {
            //                ["Referer"] = request.Uri.ToString()
            //            }
            //        };
            //        stream.WriteRequestMessageAsync(redirect, request.Encoding);
            //        var responseMessage = stream.ReadResponseMessageAsync(request.Encoding);
            //        break;
            //    default:
            //        break;
            //}

            return new HttpResponse(responseMessage, stream, request.Encoding);
        }


    }
}