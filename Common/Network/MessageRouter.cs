using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Google.Protobuf;

namespace Common.Network
{
    public class Packet
    {
        public IMessage Message;
        public NetConnection Sender;
    }

    public class MessageRouter : Singleton<MessageRouter>
    {
        public delegate void MessageHandler<T>(NetConnection sender, T message) where T : IMessage;

        private readonly Dictionary<string, Delegate> _delegates = new();

        private readonly Queue<Packet> _queue = new();

        private readonly AutoResetEvent _threadEvent = new(true);
        private bool _running;
        private int _threadCount = 1;
        private int _workerCount;


        public void Start(int threadCount)
        {
            _running = true;
            _threadCount = Math.Min(Math.Max(threadCount, 1), 200);
            for (var i = _threadCount - 1; i >= 0; i--) ThreadPool.QueueUserWorkItem(MessageWorker);
            while (_workerCount < _threadCount) Thread.Sleep(100);
        }

        public void Stop()
        {
            _running = false;
            _queue.Clear();
            while (_workerCount > 0) _threadEvent.Set();

            Thread.Sleep(100);
        }

        public void AddMessage(Packet packet)
        {
            _queue.Enqueue(new Packet { Sender = packet.Sender, Message = packet.Message });
            _threadEvent.Set();
        }

        public void Subscribe<T>(MessageHandler<T> handler) where T : IMessage
        {
            var key = typeof(T).FullName;
            if (key == null) return;
            _delegates.TryAdd(key, handler);
            Console.WriteLine("subscribed " + key);
        }

        public void Unsubscribe<T>(MessageHandler<T> handler) where T : IMessage
        {
            var key = typeof(T).FullName;
            if (key == null) return;
            if (!_delegates.ContainsKey(key))
                _delegates[key] = null;
            else
                _delegates[key] = (_delegates[key] as MessageHandler<T>) - handler;
        }

        private void Dispatch<T>(NetConnection sender, T message) where T : IMessage
        {
            var key = typeof(T).FullName;
            if (key == null) return;
            if (!_delegates.ContainsKey(key)) return;
            var handler = _delegates[key] as MessageHandler<T>;
            try
            {
                handler?.Invoke(sender, message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Message dispatch error: ", e);
            }
        }


        private void MessageWorker(object state)
        {
            try
            {
                _workerCount = Interlocked.Increment(ref _workerCount);
                Console.WriteLine("worker thread started " + _workerCount);
                while (_running)
                {
                    if (_queue.Count == 0)
                    {
                        _threadEvent.WaitOne();
                        continue;
                    }

                    var message = _queue.Dequeue();
                    var package = message.Message;
                    if (package == null) continue;
                    ExecuteMessage(message.Sender, package);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                _workerCount = Interlocked.Decrement(ref _workerCount);
            }

            Console.WriteLine("worker thread stopped");
        }

        private void ExecuteMessage(NetConnection sender, IMessage message)
        {
            var method = GetType().GetMethod("Dispatch", BindingFlags.NonPublic | BindingFlags.Instance);
            var dispatch = method?.MakeGenericMethod(message.GetType());
            dispatch?.Invoke(this, new object[] { sender, message });

            var type = message.GetType();
            foreach (var property in type.GetProperties())
            {
                if ("Parser" == property.Name || "Descriptor" == property.Name)
                    continue;
                var value = property.GetValue(message);
                if (value != null)
                    if (value.GetType().IsAssignableTo(typeof(IMessage)))
                        ExecuteMessage(sender, (IMessage)value);
            }
        }
    }
}