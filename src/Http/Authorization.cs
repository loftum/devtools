using System;
using System.Text;

namespace Http
{
    public static class Authorization
    {
        public static string Basic(string username, string password)
        {
            var bytes = Encoding.UTF8.GetBytes($"{username}:{password}");
            var auth = Convert.ToBase64String(bytes);
            return $"Basic {auth}";
        }
    }
}