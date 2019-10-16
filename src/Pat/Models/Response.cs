using System.Collections.ObjectModel;
using System.Net;
using System.Runtime.Remoting.Channels;
using Pat.ViewModels;

namespace Pat.Models
{
    public interface IResponse : IPropertyChangeNotifier
    {
        int StatusCode { get; set; }
        ObservableCollection<KeyValue<string, string>> Headers { get; }
        ObservableCollection<Cookie> Cookies { get; }
        void Update(HttpWebResponse response);
    }

    public class Response : PropertyChangeNotifier, IResponse
    {
        public virtual int StatusCode { get; set; }
        public virtual string StatusDescription { get; set; }
        public virtual string ContentType { get; set; }

        public ObservableCollection<KeyValue<string, string>> Headers { get; } = new ObservableCollection<KeyValue<string, string>>();
        public ObservableCollection<Cookie> Cookies { get; } = new ObservableCollection<Cookie>();

        public void Update(HttpWebResponse response)
        {
            StatusCode = (int)response.StatusCode;
            StatusDescription = response.StatusDescription;
            ContentType = response.ContentType;
            
            Headers.Clear();
            foreach (var key in response.Headers.AllKeys)
            {
                Headers.Add(new KeyValue<string, string>(key, response.Headers[key]));
            }
            Cookies.Clear();
            foreach (Cookie cookie in response.Cookies)
            {
                Cookies.Add(cookie);
            }
            
        }

        public void Clear()
        {
            StatusCode = 0;
            StatusDescription = null;
            ContentType = null;
            Headers.Clear();
            Cookies.Clear();
        }
    }
}