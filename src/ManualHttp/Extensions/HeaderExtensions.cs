using System.Collections.Generic;
using System.Linq;

namespace ManualHttp.Extensions
{
    public static class HeaderExtensions
    {
        public static void SetDefault(this Dictionary<string, string> headers, string key, string value)
        {
            if (!headers.ContainsKey(key))
            {
                headers[key] = value;
            }
        }

        public static void SetOrAdd(this Dictionary<string, string> headers, string key, string value)
        {
            var theValue = headers.TryGetValue(key, out var existing)
                ? string.Join(",", existing, value)
                : value;
            headers[key] = theValue;
        }

        public static string GetOrDefault(this Dictionary<string, string> headers, string key, string defaultValue = null)
        {
            return headers.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public static string Format(this Dictionary<string, string> headers)
        {
            return string.Join("\r\n", headers.Select(p => $"{p.Key}: {p.Value}"));
        }
    }
}