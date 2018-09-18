using System;

namespace TcPing
{
    public class TcPingArgs
    {
        public string Host { get; }
        public int Port { get; }
        public TimeSpan Timeout { get; }

        public TcPingArgs(string host, int port, TimeSpan timeout)
        {
            Host = host;
            Port = port;
            Timeout = timeout;
        }

        public static TcPingArgs Parse(string[] args)
        {
            if (args.Length < 1)
            {
                return null;
            }
            var values = args[0].Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (values.Length != 2)
            {
                return null;
            }
            var host = values[0];
            if (!int.TryParse(values[1], out var port))
            {
                return null;
            }
            var timeout = TimeSpan.FromSeconds(1);
            if (args.Length > 1 && int.TryParse(args[1], out var timeoutMillis))
            {
                timeout = TimeSpan.FromMilliseconds(timeoutMillis);
            }
            return new TcPingArgs(host, port, timeout);
        }
    }
}