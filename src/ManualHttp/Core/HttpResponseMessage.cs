using System.Collections.Generic;
using System.Text;
using ManualHttp.Extensions;

namespace ManualHttp.Core
{
    public class HttpResponseMessage
    {
        public StatusLine StatusLine { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string MessageBody { get; set; }

        public HttpResponseMessage()
        {
            StatusLine = new StatusLine();
            Headers = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            return new StringBuilder()
                .Append(StatusLine).Append("\r\n")
                .Append(Headers.Format()).Append("\r\n")
                .Append("\r\n")
                .Append(MessageBody)
                .ToString();
        }
    }

    public class StatusLine
    {
        public string HttpVersion { get; set; }
        public string StatusCode { get; set; }
        public string ReasonPhrase { get; set; }

        public StatusLine()
        {
            HttpVersion = "HTTP/1.1";
        }

        public override string ToString()
        {
            return $"{HttpVersion} {StatusCode} {ReasonPhrase}";
        }
    }
}