using System.Net.Sockets;
using Network.Log;
using System.Net;
using System;

namespace Network.NetServer
{
    /// <summary>
    /// 服务器类
    /// </summary>
    public class Server
    {
        /// <summary>
        /// 监听套接字
        /// </summary>
        public Socket listen;

        /// <summary>
        /// 最大连接数
        /// </summary>
        public int maxSessionClient = 50;

        /// <summary>
        /// 启动服务器
        /// </summary>
        public void StartServer(string host, int port)
        {
            //Socket udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //udp.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

            //IPEndPoint sendEndPoint = new IPEndPoint(IPAddress.Broadcast, 25566);
            //byte[] sendBytes = Encoding.UTF8.GetBytes("UDP Sned");
            //while (true)
            //{
            //    udp.SendTo(sendBytes, sendEndPoint);
            //    NetLog.Info("UDP广播");
            //    Thread.Sleep(1000);
            //}

            SessionClientPool.SetMaxSessionClient(maxSessionClient);
            listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddress = IPAddress.Parse(host);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
            listen.Bind(ipEndPoint);
            listen.Listen(maxSessionClient);
            listen.BeginAccept(AcceptCallBack, null);
            NetLog.Log(NetLogLevel.Info, "服务器启动成功！");
        }

        /// <summary>
        /// 异步建立客户端连接回调
        /// </summary>
        private void AcceptCallBack(IAsyncResult ar)
        {
            try
            {
                Socket socket = listen.EndAccept(ar);
                SessionClient session = SessionClientPool.GetSessionClient();

                if (session == null)
                {
                    socket.Close();
                    NetLog.Log(NetLogLevel.Warning, "警告：连接已满！");
                }
                else
                {
                    session.Initialize(socket);
                    string address = session.GetRemoteAddress();
                    NetLog.Log(NetLogLevel.Info, $"客户端连接 [{address}]");
                }

                listen.BeginAccept(AcceptCallBack, null);
            }
            catch (Exception e)
            {
                NetLog.Log(NetLogLevel.Error, "异步建立客户端连接失败：" + e.Message);
            }
        }
    }
}
