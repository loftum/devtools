using System;
using System.Collections.Generic;
using System.Linq;

namespace ManualHttp.Core
{
    public class CookieStore
    {
        public Dictionary<string, DomainStore> DomainStores { get; } = new Dictionary<string, DomainStore>();

        public void Store(Cookie cookie)
        {
            Console.WriteLine($"Storing cookie {cookie.Name}");
            if (!cookie.IsValid())
            {
                return;
            }
            GetDomainStore(cookie.Domain).Store(cookie);
        }

        private DomainStore GetDomainStore(string domain)
        {
            if (!DomainStores.ContainsKey(domain))
            {
                DomainStores[domain] = new DomainStore();
            }
            return DomainStores[domain];
        }

        public IEnumerable<Cookie> GetAllCookies()
        {
            return DomainStores.Values.SelectMany(s => s.Cookies.Values);
        }
    }

    public class DomainStore
    {
        public Dictionary<string, Cookie> Cookies { get; } = new Dictionary<string, Cookie>();

        public void Store(Cookie cookie)
        {
            if (cookie.IsExpired())
            {
                Cookies.Remove(cookie.Name);
                return;
            }
            Cookies[cookie.Name] = cookie;
        }
    }
}