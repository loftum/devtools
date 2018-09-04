using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;

namespace Http
{
    public static class WebRequester
    {
        public static HttpWebRequest Create(string method, string url)
        {
            var request = WebRequest.CreateHttp(url);
            request.Method = method;
            return request;
        }

        public static HttpWebRequest WithCert(this HttpWebRequest request, string certFile, string certPass)
        {
            var cert = GetCert(certFile, certPass);
            request.ClientCertificates.Add(cert);
            return request;
        }

        public static HttpWebResponse SafeGetResponse(this HttpWebRequest request)
        {
            try
            {
                return (HttpWebResponse) request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    return (HttpWebResponse) e.Response;
                }

                throw;
            }
        }

        private static X509Certificate2 GetCert(string certFile, string certPass)
        {
            if (!File.Exists(certFile))
            {
                throw new ArgumentException($"Certificate file {certFile} does not exist", nameof(certFile));
            }

            return new X509Certificate2(File.ReadAllBytes(certFile), certPass);
        }
    }

    public static class ResponseExtensions
    {
        public static object GetFormattedContent(this HttpWebResponse response)
        {
            using (var stream = response.GetResponseStream())
            {
                if (stream == null)
                {
                    return "";
                }

                using (var reader = new StreamReader(stream))
                {
                    switch (response.ContentType)
                    {
                        case "application/json":
                            return JsonConvert.DeserializeObject(reader.ReadToEnd());
                        default:
                            return reader.ReadToEnd();
                    }
                }
            }
        }
    }
}