using System.Collections.Generic;
using System.Text;
using ManualHttp.Extensions;

namespace ManualHttp.Core
{
    public class HttpRequestMessage
    {
        public RequestLine RequestLine { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string MessageBody { get; set; }

        public HttpRequestMessage()
        {
            RequestLine = new RequestLine();
            Headers = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            return new StringBuilder()
                .Append(RequestLine).Append("\r\n")
                .Append(Headers.Format()).Append("\r\n")
                .Append("\r\n")
                .Append(MessageBody)
                .ToString();
        }
    }

    public class RequestLine
    {
        public string Method { get; set; }
        public string RequestUri { get; set; }
        public string HttpVersion { get; set; }

        public RequestLine()
        {
            HttpVersion = "HTTP/1.1";
            Method = "GET";
        }

        public override string ToString()
        {
            return $"{Method} {RequestUri} {HttpVersion}";
        }
    }
}