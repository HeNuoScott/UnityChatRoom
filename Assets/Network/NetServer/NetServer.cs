using Network.CommonData;
using System.Net.Sockets;
using System.Net;
using System;

namespace Network.Server
{
    /// <summary>
    /// 服务器类
    /// </summary>
    public class NetServer 
    {
        public static NetServer Instance = null;

        /// <summary>
        /// 最大连接数
        /// </summary>
        public int maxSessionClient = 50;

        /// <summary>
        /// 监听套接字
        /// </summary>
        public Socket listenSocket;

        public bool isOpen = false;

        public static NetServer BuildServer()
        {
            if (Instance == null)
            {
                Instance = new NetServer();
            }
            return Instance;
        }

        /// <summary>
        /// 启动服务器
        /// </summary>
        public void StartServer(string host, int port)
        {
            ServiceSessionPool.SetMaxSessionClient(maxSessionClient);
            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddress = IPAddress.Parse(host);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
            listenSocket.Bind(ipEndPoint);
            listenSocket.Listen(maxSessionClient);
            listenSocket.BeginAccept(AcceptCallBack, null);
            isOpen = true;
            NetLog.Log("服务器启动成功！");
        }

        /// <summary>
        /// 关闭服务器
        /// </summary>
        public void StopServer()
        {
            if (!isOpen) return;
            isOpen = false;

            listenSocket.Close();
            foreach (var session in ServiceSessionPool.GetOnlineSession())
            {
                session.Close();
            }
            NetLog.Log("服务器关闭!");
        }

        /// <summary>
        /// 异步建立客户端连接回调
        /// </summary>
        private void AcceptCallBack(IAsyncResult asyncResult)
        {
            if (!isOpen)
            {
                listenSocket.EndAccept(asyncResult);
                listenSocket.Close();
            }
            try
            {
                Socket socket = listenSocket.EndAccept(asyncResult);
                ServiceSession session = ServiceSessionPool.GetSessionClient();

                if (session == null)
                {
                    socket.Close();
                    NetLog.Warning("警告：连接已满！");
                }
                else
                {
                    session.Initialize(socket);
                    NetLog.Log($"客户端连接 [{session.GetRemoteAddress()}]");
                }

                listenSocket.BeginAccept(AcceptCallBack, null);
            }
            catch (Exception e)
            {
                NetLog.Error("异步建立客户端连接失败：" + e.Message);
            }
        }

        /// <summary>
        /// 广播消息系统通知
        /// </summary>
        public void BroadCast(string mssage)
        {
            DynamicBuffer buffer = new DynamicBuffer(0);
            buffer.WriteObject(mssage);
            DataPackage packet = new DataPackage(buffer, ActionTypeEnum.InformAction);

            foreach (var session in ServiceSessionPool.GetOnlineSession())
            {
                ServerActionBase action = ServerActionFactory.CreateAction(ActionTypeEnum.InformAction, session);
                ActionParameter parameter = new ActionParameter();

                action.Packet = packet;
                if (action.Check(parameter))
                {
                    bool processResult = action.Process(parameter);
                    if (!processResult) NetLog.Log($"{packet.PacketType}广播处理失败!!");
                }
                action.Clean();
            }
        }

        /// <summary>
        /// 发送给指定会话消息
        /// </summary>
        /// <param name="mssage"></param>
        public void SendSessionMessage(string remoteAddress, string mssage)
        {
            DynamicBuffer buffer = new DynamicBuffer(0);
            buffer.WriteObject(mssage);
            DataPackage packet = new DataPackage(buffer, ActionTypeEnum.MessageAction);

            foreach (var session in ServiceSessionPool.GetOnlineSession())
            {
                if (remoteAddress== session.GetRemoteAddress())
                {
                    ServerActionBase action = ServerActionFactory.CreateAction(ActionTypeEnum.MessageAction, session);
                    ActionParameter parameter = new ActionParameter();
                    action.Packet = packet;
                    if (action.Check(parameter))
                    {
                        bool processResult = action.Process(parameter);
                        if (!processResult) NetLog.Log($"{packet.PacketType}服务器指定消息发送失败!!");
                    }
                    action.Clean();
                } 
            }
        }
    }
}
