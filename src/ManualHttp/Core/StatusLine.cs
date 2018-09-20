namespace ManualHttp.Core
{
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