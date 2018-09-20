using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManualHttp.Core;

namespace ManualHttp.Extensions
{
    public static class StreamExtensions
    {
        public static T Get<T>(this IList<T> items, int index, T defaultValue = default(T))
        {
            return index > items.Count ? defaultValue : items[index];
        }

        public static async Task WriteRequestMessageAsync(this Stream stream, HttpRequestMessage message, Encoding encoding)
        {
            using (var writer = new StreamWriter(stream, encoding, 1024, true))
            {
                await writer.WriteAsync(message.RequestLine.ToString());
                await writer.WriteAsync("\r\n");
                await writer.WriteAsync(message.Headers.Format());
                await writer.WriteAsync("\r\n\r\n");
            }

            if (message.Body != null &&
                message.Headers.TryGetValue("Content-Length", out var val) &&
                int.TryParse(val, out var contentLength) &&
                contentLength > 0)
            {
                var bytes = encoding.GetBytes(message.Body);
                if (bytes.Length > 0)
                {
                    await stream.WriteAsync(bytes);
                }
            }
            await stream.FlushAsync();
        }

        public static async Task WriteTextAsync(this Stream stream, string message, Encoding encoding)
        {
            var bytes = encoding.GetBytes(message);
            Console.WriteLine($"Writing {bytes.Length} bytes");
            await stream.WriteAsync(bytes, 0, bytes.Length);
            await stream.FlushAsync();
        }

        public static async Task<HttpResponseMessage> ReadResponseMessageAsync(this Stream stream, Encoding encoding)
        {
            var message = new HttpResponseMessage();

            using (var reader = new StreamReader(stream, encoding, true, 1, true))
            {
                message.StatusLine = await reader.ReadStatusLineAsync();
                message.Headers = await reader.ReadHeadersAsync();
                if (message.Headers.TryGetValue("Content-Length", out var val) && int.TryParse(val, out var contentLength) && contentLength > 0)
                {
                    var buffer = new char[contentLength];
                    await reader.ReadAsync(buffer, 0, contentLength);
                    message.MessageBody = new string(buffer);
                }
            }
            return message;
        }

        public static async Task<Dictionary<string, string>> ReadHeadersAsync(this StreamReader reader)
        {
            var headers = new Dictionary<string, string>();
            string line;
            while ((line = await reader.ReadLineAsync()) != string.Empty)
            {
                var separator = line.IndexOf(":", StringComparison.Ordinal);
                var key = line.Substring(0, separator).Trim();
                var value = line.Substring(separator+1).Trim();
                headers.SetOrAdd(key, value);
            }
            return headers;
        }

        public static async Task<StatusLine> ReadStatusLineAsync(this StreamReader reader)
        {
            var line = await reader.ReadLineAsync();
            if (line == null)
            {
                return null;
            }
            var parts = line.Split(' ');
            return new StatusLine
            {
                HttpVersion = parts.Get(0),
                StatusCode = parts.Get(1),
                ReasonPhrase = string.Join(" ", parts.Skip(2))
            };
        }

        public static string ReadUntilDoubleLineFeed(this Stream stream, Encoding encoding)
        {
            var header = new StringBuilder();
            

            using (var reader = new StreamReader(stream, encoding, true, 1, true))
            {
                string line;
                while ((line = reader.ReadLine()) != string.Empty)
                {
                    header.AppendLine(line);
                }
            }



            //var lineFeeds = 0;
            //var read = -1;
            //while ((read = stream.ReadByte()) != -1)
            //{
            //    char c = (char) read;
            //    header.Append(c);
            //    switch (c)
            //    {
            //        case '\n':
            //            lineFeeds++;
            //            break;
            //        case '\r':
            //            break;
            //        default:
            //            lineFeeds = 0;
            //            break;
            //    }
            //    if (lineFeeds >= 2)
            //    {
            //        break;
            //    }
            //}
            return header.ToString();
        }
    }
}