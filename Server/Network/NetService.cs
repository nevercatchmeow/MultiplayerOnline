using System;
using System.Net.Sockets;
using Common.Network;
using Common.Proto;

namespace Server.Network
{
    public class NetService
    {
        private TcpSocketListener _ln;

        public void Init(int port)
        {
            _ln = new TcpSocketListener("0.0.0.0", port);
            _ln.EventHandler += OnClientConnected;
        }

        public void Start()
        {
            _ln?.Start();
        }

        private void OnClientConnected(object sender, Socket socket)
        {
            new NetConnection(socket,
                OnDataReceived,
                OnDisconnected);
        }

        private void OnDataReceived(NetConnection sender, byte[] data)
        {
            var v = Package.Parser.ParseFrom(data);
            MessageRouter.Instance.AddMessage(new Packet { Sender = sender, Message = v });
        }

        private void OnDisconnected(NetConnection sender)
        {
            Console.WriteLine("disconnected");
        }
    }
}