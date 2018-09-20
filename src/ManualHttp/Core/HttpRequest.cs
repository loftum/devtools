using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ManualHttp.Extensions;

namespace ManualHttp.Core
{
    public class HttpRequest
    {
        public string Method { get; set; }
        public Uri Uri { get; }
        public Dictionary<string, string> Headers { get; }
        public Encoding Encoding { get; set; }
        public CookieStore CookieStore { get; set; }
        public Func<HttpResponseMessage,bool> OnRedirect { get; set; }

        public HttpRequest(Uri uri)
        {
            switch (uri.Scheme)
            {
                case "http":
                case "https":
                    break;
                default:
                    throw new ArgumentException($"Invalid scheme {uri.Scheme}", nameof(uri));
            }
            Uri = uri;
            Headers = new Dictionary<string, string>();
            Encoding = new UTF8Encoding(false);
            RemoteCertificateValidationCallback = AlwaysValid;
            OnRedirect = m => true;
            CookieStore = new CookieStore();
        }

        private static bool AlwaysValid(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }

        public Task<HttpResponse> GetResponseAsync()
        {
            return new HttpProtocol().GetResponseAsync(this);
        }

        public RemoteCertificateValidationCallback RemoteCertificateValidationCallback { get; set; }

        public HttpRequestMessage GetRequestMessage()
        {
            Headers.SetDefault("Host", Uri.Host);
            Headers.SetDefault("Connection", "keep-alive");
            Headers.SetDefault("User-Agent", "Casio Typewriter");
            Headers.SetDefault("Accept", "*/*");

            return new HttpRequestMessage
            {
                RequestLine = new RequestLine
                {
                    Method = Method,
                    RequestUri = Uri.PathAndQuery
                },
                Headers = Headers
            };
        }
    }
}