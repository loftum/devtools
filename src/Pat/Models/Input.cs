using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Pat.Models
{
    public interface IInput
    {
        string Method { get; set; }
        string Url { get; set; }
        string CertFile { get; set; }
        string CertPass { get; set; }
        bool AllowAutoRedirect { get; set; }
        ObservableCollection<KeyValue<string, string>> Headers { get; set; }
        HttpWebRequest CreateRequest();
    }

    public class Input : IInput
    {
        public string Method { get; set; }
        public string Url { get; set; }
        public string CertFile { get; set; }
        public string CertPass { get; set; }
        public bool AllowAutoRedirect { get; set; }
        public ObservableCollection<KeyValue<string, string>> Headers { get; set; }
        
        public Input()
        {
            Headers = new ObservableCollection<KeyValue<string, string>>();
        }

        public HttpWebRequest CreateRequest()
        {
            var request = WebRequest.CreateHttp(Url);
            request.Method = Method;
            if (!string.IsNullOrWhiteSpace(CertFile))
            {
                request.ClientCertificates.Add(new X509Certificate2(File.ReadAllBytes(CertFile), CertPass));
            }

            request.AllowAutoRedirect = AllowAutoRedirect;
            foreach (var header in Headers)
            {
                var lowerKey = header.Key.ToLowerInvariant();
                switch (lowerKey)
                {
                    case "accept":
                        request.Accept = header.Value;
                        break;
                    case "user-agent":
                        request.UserAgent = header.Value;
                        break;
                    case "content-type":
                        request.ContentType = header.Value;
                        break;
                    default:
                        request.Headers[header.Key] = header.Value;
                        break;
                }
            }
            return request;
        }
    }
}