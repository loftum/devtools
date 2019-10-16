using System.Collections.ObjectModel;
using System.Net;

namespace Pat.Models
{
    public class Request : PropertyChangeNotifier
    {
        public virtual string ProtocolVersion { get; set; }
        public virtual SecurityProtocolType SecurityProtocol { get; set; }
        public virtual string Method { get; set; }
        public virtual string Uri { get; set; }
        public virtual int Timeout { get; set; }
        public virtual string Address { get; set; }
        public virtual string Accept { get; set; }
        public virtual string Expect { get; set; }
        public virtual bool AllowAutoRedirect { get; set; }
        public virtual DecompressionMethods AutomaticDecompression { get; set; }
        public virtual string ConnectionGroupName { get; set; }
        public virtual ObservableCollection<KeyValue<string, string>> Headers { get; } = new ObservableCollection<KeyValue<string, string>>();
        public virtual ObservableCollection<Cookie> Cookies { get; } = new ObservableCollection<Cookie>();

        public void Update(HttpWebRequest request)
        {
            ProtocolVersion = request.ProtocolVersion.ToString();
            SecurityProtocol = ServicePointManager.SecurityProtocol;
            Method = request.Method;
            Uri = request.RequestUri?.ToString();
            Timeout = request.Timeout;
            Address = request.Address?.ToString();
            Accept = request.Accept;
            AllowAutoRedirect = request.AllowAutoRedirect;
            AutomaticDecompression = request.AutomaticDecompression;

            Expect = request.Expect;
            ConnectionGroupName = request.ConnectionGroupName;
            Headers.Clear();
            foreach (var key in request.Headers.AllKeys)
            {
                Headers.Add(new KeyValue<string, string>(key, request.Headers[key]));
            }
            Cookies.Clear();
            foreach (Cookie cookie in request.CookieContainer.GetCookies(request.RequestUri))
            {
                Cookies.Add(cookie);
            }
        }

        
    }
}