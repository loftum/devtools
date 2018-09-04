using Http.Logging;

namespace Http
{
    public class HttpCommands
    {
        private static readonly ILogger Logger = Log.For<HttpCommands>();

        public static void LogLevel(LogLevel level)
        {
            Settings.Instance.LogLevel = level;
        }

        public static void Get(string url, string accept = null, string username = null, string password = null, string certFile = null, string certPass = null)
        {
            Send("GET", url, accept, username, password, certFile, certPass);
        }

        public static void Send(string method, string url, string accept = null, string username = null, string password = null, string certFile = null, string certPass = null)
        {
            var request = WebRequester.Create(method, url);
            if (username != null || password != null)
            {
                request.Headers["Authorization"] = Authorization.Basic(username, password);
            }
            if (certFile != null)
            {
                request = request.WithCert(certFile, certPass);
            }

            if (request.Headers.Count > 0)
            {
                Logger.Debug("Request headers:");
                foreach (var key in request.Headers.AllKeys)
                {
                    Logger.Debug($"{key}: {request.Headers[key]}");
                }
            }

            using (var response = request.SafeGetResponse())
            {
                Logger.Important($"StatusCode: {response.StatusCode}");
                if (response.Headers.Count > 0)
                {
                    Logger.Debug("Headers:");
                    foreach (var key in response.Headers.AllKeys)
                    {
                        Logger.Debug($"{key}:{response.Headers[key]}");
                    }
                }
                Logger.Normal("Content:");
                Logger.Normal(response.GetFormattedContent());
            }
        }
    }
}