using System.Collections.ObjectModel;
using System.Net;
using Pat.ViewModels;

namespace Pat.Models
{
    public interface IRequest : IPropertyChangeNotifier
    {
        string Method { get; set; }
        string Uri { get; set; }
        string Address { get; set; }
        ObservableCollection<KeyValue<string, string>> Headers { get; }
        ObservableCollection<Cookie> Cookies { get; }
        void Update(HttpWebRequest request);
    }

    public class Request : PropertyChangeNotifier, IRequest
    {
        public virtual string Method { get; set; }
        public virtual string Uri { get; set; }
        public virtual string Address { get; set; }
        public virtual ObservableCollection<KeyValue<string, string>> Headers { get; } = new ObservableCollection<KeyValue<string, string>>();
        public virtual ObservableCollection<Cookie> Cookies { get; } = new ObservableCollection<Cookie>();

        public void Update(HttpWebRequest request)
        {
            Method = request.Method;
            Uri = request.RequestUri?.ToString();
            Address = request.Address?.ToString();
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