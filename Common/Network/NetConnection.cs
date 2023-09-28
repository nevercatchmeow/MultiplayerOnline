using System;
using System.IO;
using System.Net.Sockets;
using Common.Proto;
using Google.Protobuf;

namespace Common.Network
{
    public class NetConnection
    {
        public delegate void DataReceivedCallback(NetConnection sender, byte[] data);

        public delegate void DisconnectedCallback(NetConnection sender);

        private readonly DataReceivedCallback _dataReceivedCallback;
        private readonly DisconnectedCallback _disconnectedCallback;

        private Package _package;
        private Socket _socket;

        public NetConnection(Socket socket, DataReceivedCallback cb1, DisconnectedCallback cb2)
        {
            _socket = socket;
            _dataReceivedCallback = cb1;
            _disconnectedCallback = cb2;

            var decoder = new LengthFieldDecoder(socket, 64 * 1024, 0, 4, 0, 4);
            decoder.dataReceivedHandler += OnDataReceived;
            if (_disconnectedCallback != null) decoder.disconnectedHandler += () => _disconnectedCallback(this);
            decoder.Start();
        }

        public Request Request
        {
            get
            {
                if (_package == null) _package = new Package();
                if (_package.Request == null) _package.Request = new Request();
                return _package.Request;
            }
        }


        public Response Response
        {
            get
            {
                if (_package == null) _package = new Package();
                if (_package.Response == null) _package.Response = new Response();
                return _package.Response;
            }
        }

        public void Close()
        {
            lock (this)
            {
                try
                {
                    _socket?.Shutdown(SocketShutdown.Both);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                _socket?.Close();
                _socket = null;
                _disconnectedCallback?.Invoke(this);
            }
        }

        public void Send()
        {
            if (_package != null) Send(_package);
            _package = null;
        }

        public void Send(Package package)
        {
            byte[] data;
            using (var ms = new MemoryStream())
            {
                package.WriteTo(ms);
                //对消息进行编码
                data = new byte[4 + ms.Length];
                Buffer.BlockCopy(BitConverter.GetBytes(ms.Length), 0, data, 0, 4);
                Buffer.BlockCopy(ms.GetBuffer(), 0, data, 4, (int)ms.Length);
            }

            Send(data, 0, data.Length);
        }

        public void Send(byte[] data, int offset, int count)
        {
            lock (this)
            {
                if (_socket == null || !_socket.Connected) return;
                _socket.BeginSend(data, offset, count, SocketFlags.None, SendCallback, _socket);
            }
        }

        private void SendCallback(IAsyncResult result)
        {
            var len = _socket?.EndSend(result);
        }

        private void OnDataReceived(object sender, byte[] data)
        {
            _dataReceivedCallback?.Invoke(this, data);
        }
    }
}