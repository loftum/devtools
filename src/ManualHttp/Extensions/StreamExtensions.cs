using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using ManualHttp.Core;

namespace ManualHttp.Extensions
{
    public static class StreamExtensions
    {
        public static T Get<T>(this IList<T> items, int index, T defaultValue = default(T))
        {
            return index > items.Count ? defaultValue : items[index];
        }

        public static void WriteRequestMessage(this Stream stream, HttpRequestMessage message, Encoding encoding)
        {
            var bytes = encoding.GetBytes(message.ToString());
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }

        public static HttpResponseMessage ReadResponseMessage(this Stream stream, Encoding encoding)
        {
            var message = new HttpResponseMessage();

            using (var reader = new StreamReader(stream, encoding, true, 1, true))
            {
                message.StatusLine = reader.ReadStatusLine();
                message.Headers = reader.ReadHeaders();
            }
            return message;
        }

        public static Dictionary<string, string> ReadHeaders(this StreamReader reader)
        {
            var headers = new Dictionary<string, string>();
            string line;
            while ((line = reader.ReadLine()) != string.Empty)
            {
                var separator = line.IndexOf(":");
                var key = line.Substring(0, separator).Trim();
                var value = line.Substring(separator+1).Trim();
                headers.SetOrAdd(key, value);
            }
            return headers;
        }

        public static StatusLine ReadStatusLine(this StreamReader reader)
        {
            var line = reader.ReadLine();
            if (line == null)
            {
                return null;
            }
            var parts = line.Split(' ');
            return new StatusLine
            {
                HttpVersion = parts.Get(0),
                StatusCode = parts.Get(1),
                ReasonPhrase = parts.Get(2)
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