using System;
using System.Collections.Generic;
using System.Linq;
using ManualHttp.Extensions;

namespace ManualHttp.Core
{
    public class Cookie
    {
        public string Domain { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public bool Secure { get; set; }
        public bool HttpOnly { get; set; }
        public DateTime? Expires { get; set; }

        public bool IsExpired() => Expires.HasValue && Expires.Value < DateTime.Now;

        public bool IsValid()
            =>
            !(string.IsNullOrWhiteSpace(Domain) || string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Value));

        public static Cookie Parse(string raw)
        {
            var cookie = new Cookie();
            var values = raw.Split(';');
            var first = values[0];
            var nameValue = first.Split('=');
            cookie.Name = nameValue[0].Trim();
            cookie.Value = nameValue[1].Trim();
            
            foreach (var pair in values.Skip(1).Select(v => v.ToKeyValue('=')))
            {
                switch (pair.Key)
                {
                    case "Domain":
                        cookie.Domain = pair.Value;
                        break;
                    case "Secure":
                        cookie.Secure = true;
                        break;
                    case "HttpOnly":
                        cookie.HttpOnly = true;
                        break;
                    case "Path":
                        cookie.Path = pair.Value;
                        break;
                    case "Expires":
                        cookie.Expires = DateTime.Parse(pair.Value);
                        break;
                    case "Max-Age":
                        cookie.Expires = DateTime.Now.AddSeconds(int.Parse(pair.Value));
                        break;
                    default:
                        break;
                }
            }
            return cookie;
        }

        public static IEnumerable<Cookie> ParseCookies(string setCookie)
        {
            return string.IsNullOrEmpty(setCookie)
                ? Enumerable.Empty<Cookie>()
                : setCookie.Split(',').Select(Parse);
        }

        public override string ToString()
        {
            var values = GetType().GetProperties().Select(p => $"{p.Name}={p.GetValue(this)}");
            return string.Join(" ", values);
        }
    }
}