using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Http
{
    public class HttpCommands
    {
        public static void Get(string url, string certFile = null, string certPass = null)
        {
            var request = WebRequest.CreateHttp(url);
            if (certFile != null)
            {
                request.ClientCertificates.Add(GetCert(certFile, certPass));
            }
            using (var response = request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    if (stream == null)
                    {
                        return;
                    }
                    using (var reader = new StreamReader(stream))
                    {
                        var read = reader.ReadToEnd();
                        Console.WriteLine(read);
                    }
                }
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
}