using System.IO;
using System.Net.Security;
using System.Net.Sockets;
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

        public HttpResponse GetResponse(HttpRequest request)
        {
            var stream = GetStream(request);

            stream.WriteRequestMessage(request.GetRequestMessage(), request.Encoding);
            var responseMessage = stream.ReadResponseMessage(request.Encoding);

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
            //        stream.WriteRequestMessage(redirect, request.Encoding);
            //        var responseMessage = stream.ReadResponseMessage(request.Encoding);
            //        break;
            //    default:
            //        break;
            //}

            return new HttpResponse(responseMessage, stream, request.Encoding);
        }


    }
}