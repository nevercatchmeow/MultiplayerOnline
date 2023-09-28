using System;
using System.Net.Sockets;

namespace Common.Network
{
    /// <summary>
    ///     这是Socket异步接收器，可以对接收的数据粘包与拆包
    ///     事件委托：
    ///     -- Received 数据包接收完成事件，参数为接收的数据包
    ///     -- ConnectFailed 接收异常事件
    ///     使用方法：
    ///     var lfd = new LengthFieldDecoder(socket, 64*1024, 0, 4, 0, 4);
    ///     lfd.DataReceived += OnDataReceived;
    ///     lfd.Start();
    ///     注意：长度是签名的int类型
    /// </summary>
    public class LengthFieldDecoder
    {
        //连接断开的委托事件
        public delegate void DisconnectedEventHandler();

        private readonly int initialBytesToStrip; //结果数据中前几个字节不需要的字节数

        /// <summary>
        //长度字段和内容之间距离几个字节，
        //负数代表向前偏移，body实际长度要减去这个绝对值
        /// </summary>
        private readonly int lengthAdjustment;

        private readonly int lengthFieldLength = 4; //长度字段占几个字节

        private readonly int lengthFieldOffset = 8; //第几个是body长度字段

        private readonly byte[] mBuffer; //接收数据的缓存空间

        /// <summary>
        ///     一次性接收数据的最大字节，默认64k
        /// </summary>
        private readonly int mSize = 64 * 1024;

        private bool isStart; //是否已经启动
        private int mOffect; //读取位置

        private Socket mSocket;


        public LengthFieldDecoder(Socket socket, int lengthFieldOffset, int lengthFieldLength)
            : this(socket, 64 * 1024, lengthFieldOffset, lengthFieldLength, 0, lengthFieldLength)
        {
        }

        /// <summary>
        ///     长度字段解码器
        /// </summary>
        /// <param name="socket">Socket对象</param>
        /// <param name="maxBufferLength">缓冲区大小</param>
        /// <param name="lengthFieldOffset">长度字段开始位置</param>
        /// <param name="lengthFieldLength">长度字段本身长度</param>
        /// <param name="lengthAdjustment">长度字段和内容的距离</param>
        /// <param name="initialBytesToStrip">结果跳过的字节数</param>
        public LengthFieldDecoder(Socket socket, int maxBufferLength, int lengthFieldOffset, int lengthFieldLength,
            int lengthAdjustment, int initialBytesToStrip)
        {
            mSocket = socket;
            mSize = maxBufferLength;
            this.lengthFieldOffset = lengthFieldOffset;
            this.lengthFieldLength = lengthFieldLength;
            this.lengthAdjustment = lengthAdjustment;
            this.initialBytesToStrip = initialBytesToStrip;
            mBuffer = new byte[mSize];
        }

        //成功收到消息的委托事件
        public event EventHandler<byte[]> dataReceivedHandler;
        public event DisconnectedEventHandler disconnectedHandler;

        public void Start()
        {
            if (mSocket != null && !isStart)
            {
                BeginAsyncReceive();
                isStart = true;
            }
        }

        private void BeginAsyncReceive()
        {
            //Debug.Log("开始接收");
            mSocket.BeginReceive(mBuffer, mOffect, mSize - mOffect, SocketFlags.None, Receive, null);
        }

        private void Receive(IAsyncResult result)
        {
            try
            {
                var len = mSocket.EndReceive(result);
                // 0代表连接失败
                if (len == 0)
                {
                    doDisconnected();
                    return;
                }

                OnReceiveData(len);

                //继续接收数据
                BeginAsyncReceive();
            }
            catch (SocketException)
            {
                //Console.WriteLine("消息接收异常");
                doDisconnected();
            }
        }

        private void doDisconnected()
        {
            try
            {
                disconnectedHandler?.Invoke();
                mSocket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
            } // throws if client process has already closed

            mSocket.Close();
            mSocket = null;
        }

        private void OnReceiveData(int len)
        {
            //headLen+bodyLen=totalLen
            var headLen = lengthFieldOffset + lengthFieldLength;
            var adj = lengthAdjustment; //body偏移量

            //size是待处理的数据长度，mOffect每次都从0开始，
            //循环开始之前mOffect代表上次剩余长度
            var size = len;
            if (mOffect > 0)
            {
                size += mOffect;
                mOffect = 0;
            }

            //循环解析
            while (true)
            {
                var remain = size - mOffect; //剩余未处理的长度
                //Debug.Log("剩余未处理的长度：" + remain);

                //如果未处理的数据超出限制
                if (remain > mSize) throw new IndexOutOfRangeException();
                if (remain < headLen)
                {
                    //接收的数据不够一个完整的包，继续接收
                    Array.Copy(mBuffer, mOffect, mBuffer, 0, remain);
                    mOffect = remain;
                    return;
                }

                //获取包长度
                var bodyLen = BitConverter.ToInt32(mBuffer, mOffect + lengthFieldOffset);
                if (remain < headLen + adj + bodyLen)
                {
                    //接收的数据不够一个完整的包，继续接收
                    Array.Copy(mBuffer, mOffect, mBuffer, 0, remain);
                    mOffect = remain;
                    return;
                }

                //body的读取位置
                var bodyStart = mOffect + Math.Max(headLen, headLen + adj);
                //body的真实长度
                var bodyCount = Math.Min(bodyLen, bodyLen + adj);
                //Debug.Log("bodyStart=" + bodyStart + ", bodyCount=" + bodyCount+ ", remain=" + remain);

                //获取包体
                var total = headLen + adj + bodyLen; //数据包总长度
                var count = total - initialBytesToStrip;
                var data = new byte[count];
                Array.Copy(mBuffer, mOffect + initialBytesToStrip, data, 0, count);
                mOffect += total;

                //完成一个数据包
                if (dataReceivedHandler != null) dataReceivedHandler(this, data);

                //Debug.Log("完成一个数据包");
            }
        }
    }
}