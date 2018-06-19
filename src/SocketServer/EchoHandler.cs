using System;
using System.Net.Sockets;
using System.Text;

namespace SocketServer
{
    public class EchoHandler: IDisposable
    {
        private static int _nextId;
        private readonly int _id;
        private readonly Socket _socket;
        public bool IsRunning { get; private set; }
        public bool IsDisposed { get; private set; }

        public EchoHandler(Socket socket)
        {
            _socket = socket;
            _id = ++_nextId;
        }

        public void Start()
        {
            try
            {
                IsRunning = true;
                while (IsRunning)
                {
                    var bytes = new byte[1024];
                    string data = null;
                    while (true)
                    {
                        var received = _socket.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, received);
                        if (data.IndexOf("<EOF>") >= 0)
                        {
                            break;
                        }
                    }

                    Console.WriteLine($"EchoHandler {_id}: Echoing back: {data}");
                    _socket.Send(Encoding.ASCII.GetBytes(data));
                }
                Console.WriteLine($"EchoHandler {_id}: Stopped");
            }
            catch (SocketException e)
            {
                Console.WriteLine($"EchoHandler {e.Message}. Stopping");
                throw;
            }
            finally
            {
                Stop();
            }
            
        }

        public void Stop()
        {
            if (IsDisposed)
            {
                return;
            }
            IsDisposed = true;
            _socket.Disconnect(true);
            _socket.Close();
            _socket.Dispose();
        }

        public void Dispose()
        {
            _socket?.Dispose();
        }
    }
}