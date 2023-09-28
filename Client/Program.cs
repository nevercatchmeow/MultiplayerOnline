using System;
using System.Net;
using System.Net.Sockets;
using Common.Network;
using Common.Proto;

namespace Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var client = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2359);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(client);
            Console.WriteLine("connected to server successfully");

            var conn = new NetConnection(socket, null, null);
            conn.Request.UserLogin = new UserLoginRequest
            {
                Username = "username", Password = "password"
            };
            conn.Send();

            Console.ReadLine();
        }
    }
}