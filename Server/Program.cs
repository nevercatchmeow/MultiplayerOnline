using System;
using Common.Network;
using Common.Proto;
using Server.Network;

namespace Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var netService = new NetService();
            netService.Init(2359);
            netService.Start();
            MessageRouter.Instance.Start(5);
            MessageRouter.Instance.Subscribe<UserRegisterRequest>(OnUserRegisterRequest);
            MessageRouter.Instance.Subscribe<UserLoginRequest>(OnUserLoginRequest);
            Console.ReadKey();
        }

        private static void OnUserRegisterRequest(NetConnection sender, UserRegisterRequest message)
        {
            Console.WriteLine("received user register request, username={0}, password={1}",
                message.Username, message.Password);
        }

        private static void OnUserLoginRequest(NetConnection sender, UserLoginRequest message)
        {
            Console.WriteLine("received user login request, username={0}, password={1}",
                message.Username, message.Password);
        }
    }
}