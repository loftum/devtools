using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketServer;

public class EchoServer : IDisposable
{
    private bool _running = true;
    private readonly IPEndPoint _localEndPoint;
    private readonly Socket _listener;
    public bool IsDisposed { get; private set; }
    private readonly List<EchoHandler> _handlers = new();
    private readonly int _port;

    public EchoServer(int port)
    {
        _port = port;
        var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        var address = ipHostInfo.AddressList[0];
        _localEndPoint = new IPEndPoint(address, port);
        _listener = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    }

    public void Start()
    {
        _listener.Bind(_localEndPoint);
        _listener.Listen(10);
        Console.WriteLine($"Listening for connections on port {_port} ...");
        while (_running)
        {
            var socket = _listener.Accept();
            Console.WriteLine($"Got connection from {socket.RemoteEndPoint}");
            var handler = new EchoHandler(socket);
            _handlers.Add(handler);
            new Thread(handler.Start).Start();
        }
    }

    public void Stop()
    {
        if (IsDisposed)
        {
            return;
        }

        foreach (var handler in _handlers.ToList())
        {
            handler.Dispose();
            _handlers.Remove(handler);
        }
        IsDisposed = true;
        _listener.Disconnect(true);
        _listener.Close();
        _listener.Dispose();
    }

    public void Dispose()
    {
        Stop();
    }
}