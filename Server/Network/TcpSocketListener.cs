using System;
using System.Net;
using System.Net.Sockets;

namespace Server.Network
{
    /// <summary>
    ///     负责监听TCP网络端口，异步接收Socket连接
    /// </summary>
    public class TcpSocketListener
    {
        private readonly IPEndPoint _endpoint;
        private Socket _socket;

        public TcpSocketListener(string host, int port)
        {
            _endpoint = new IPEndPoint(IPAddress.Parse(host), port);
        }

        private bool IsRunning => _socket != null;

        public event EventHandler<Socket> EventHandler;

        public void Start()
        {
            if (IsRunning) return;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(_endpoint);
            _socket.Listen();
            Console.WriteLine("listening at " + _endpoint.Port);
            var args = new SocketAsyncEventArgs();
            args.Completed += OnAccept;
            _socket.AcceptAsync(args);
        }

        private void OnAccept(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                var client = e.AcceptSocket;
                if (client != null) EventHandler?.Invoke(this, client);
            }

            e.AcceptSocket = null;
            _socket?.AcceptAsync(e);
        }

        public void Stop()
        {
            if (_socket == null)
                return;
            _socket.Close();
            _socket = null;
        }
    }
}