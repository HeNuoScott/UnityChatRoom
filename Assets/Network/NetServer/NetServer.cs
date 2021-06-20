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

        public event Action OnServerStarted;            // 服务器 启动
        public event Action OnServerStoped;             // 服务器 关闭
        public event Action<string> OnHoldbackConnect;  // 客户端 链接达到上限 
        public event Action<string> OnClientConnect;    // 客户端上线
        public event Action<string> OnClientOffline;    // 客户端下线
        public event Action<string> OnClientDisconnect; // 客户端意外断开链接

        /// <summary>
        /// 启动服务器
        /// </summary>
        public void StartServer(int port,int maxClientCount)
        {
            ServiceSessionPool.SetMaxSessionClient(maxClientCount);
            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, port);
            listenSocket.Bind(ipEndPoint);
            listenSocket.Listen(maxClientCount);
            listenSocket.BeginAccept(AcceptCallBack, null);
            isOpen = true;
            OnServerStarted?.Invoke();
            NetLog.Log($"[{GetLocalIp()}:{port}] 服务器启动成功!");
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
            OnServerStoped?.Invoke();
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
                    OnerviceSessionHoldbackConnect(socket.RemoteEndPoint.ToString());
                }
                else
                {
                    session.Initialize(socket);
                    OnerviceSessionConnect(session.GetRemoteAddress());
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


        /// <summary>
        /// 客户端上线
        /// </summary>
        /// <param name="remoteAddress"></param>
        internal void OnerviceSessionConnect(string remoteAddress)
        {
            OnClientConnect?.Invoke(remoteAddress);
            NetLog.Log($"客户端连接 [{remoteAddress}]");
        }

        /// <summary>
        /// 客户端已经达到上线数量 禁止链接
        /// </summary>
        internal void OnerviceSessionHoldbackConnect(string remoteAddress)
        {
            OnHoldbackConnect?.Invoke(remoteAddress);
            NetLog.Warning($"警告：连接已满！[{remoteAddress}]");
        }

        /// <summary>
        /// 客户端下线
        /// </summary>
        /// <param name="remoteAddress"></param>
        internal void OnServiceSessionOffline(string remoteAddress)
        {
            OnClientOffline?.Invoke(remoteAddress);
            NetLog.Log($"收到 {remoteAddress} 下线");
        }

        /// <summary>
        /// 客户端意外断开链接
        /// </summary>
        /// <param name="remoteAddress"></param>
        internal void OnServiceSessionDisconnect(string remoteAddress)
        {
            OnClientDisconnect?.Invoke(remoteAddress);
            NetLog.Warning($"收到 {remoteAddress} 断开连接");
        }

        /// <summary>
        /// 获取本机IP
        /// </summary>
        internal string GetLocalIp()
        {
            ///获取本地的IP地址
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                }
            }
            return AddressIP;
        }
    }
}
