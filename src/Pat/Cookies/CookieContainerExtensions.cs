using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Pat.Cookies
{
    public static class CookieContainerExtensions
    {
        public static IEnumerable<Cookie> GetAllCookies(this CookieContainer container)
        {
            var domainTable = (Hashtable)container.GetType().GetField("m_domainTable", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(container);
            foreach (DictionaryEntry domainEntry in domainTable)
            {
                var l = (SortedList)domainEntry.Value.GetType().GetField("m_list", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(domainEntry.Value);
                foreach (var e in l)
                {
                    var cookieCollection = (CookieCollection)((DictionaryEntry)e).Value;
                    foreach (Cookie cookie in cookieCollection)
                    {
                        yield return cookie;
                    }
                }
            }
        }
    }
}