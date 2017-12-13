using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ManualHttp.Core
{
    public class HttpResponse : IDisposable
    {
        public HttpResponseMessage RawMessage { get; }
        public int StatusCode { get; }
        public string StatusDescription { get; }
        public Dictionary<string, string> Headers { get; }
        private readonly Stream _stream;
        private bool _isDisposed;

        public HttpResponse(HttpResponseMessage message, Stream stream, Encoding encoding)
        {
            RawMessage = message;
            _stream = stream;
            StatusCode = int.Parse(message.StatusLine.StatusCode);
            StatusDescription = message.StatusLine.ReasonPhrase;
            Headers = message.Headers;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            _stream.Dispose();
        }
    }
}