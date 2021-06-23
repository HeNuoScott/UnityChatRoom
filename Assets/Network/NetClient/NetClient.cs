using System.Collections;
using System.Net.Sockets;
using UnityEngine;
using System;
using Network.CommonData;
using System.Collections.Generic;

namespace Network.Client
{
    /// <summary>
    /// 网络客户端
    /// </summary>
    public class NetClient : MonoBehaviour
    {
        private static NetClient instance;
        private ClientSession Session = null;
        private ClientActionHandler Action = null;
        private Coroutine reconnectCoroutine;//重连协程
        private float Timer = 0f;

        //单例全局访问接口
        public static NetClient Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject obj = new GameObject("NetClient");
                    instance = obj.AddComponent<NetClient>();
                    DontDestroyOnLoad(instance.gameObject);
                }
                return instance;
            }
        }
        public event Action OnConnectionSuccess;        // 链接成功
        public event Action OnConnectionFailed;         // 链接失败
        public event Action OnConnectionBreaked;        // 链接中断
        /// <summary>
        /// 调用队列
        /// </summary>
        private Queue<string> invokeQueue = new Queue<string>();
        // 客户端状态
        public SessionState ClientState { get { return Session.State; } }
        //服务器地址
        public string RemoteAddress { get { return Session.GetRemoteAddress(); } }
        //本地客户端地址
        public string LocalAddress { get { return Session.GetLocalAddress(); } }
        //主机地址
        public string Host { get; private set; }
        //主机端口
        public int Port { get; private set; }
        //延迟
        public int Ping { get; private set; }
        //心跳间隔
        public float HeartInterval { get; set; } = 2f;
        //连接超时毫秒
        public int Timeout { get; set; }

        private void Awake()
        {
            Timeout = 3000;
            Action = new ClientActionHandler();
            Session = new ClientSession(Action);
            Action.AddListener(ActionTypeEnum.HeartAction, HeartPing);
        }

        private void Update()
        {
            //检测服务器连接断开重连
            if (reconnectCoroutine == null && Session.State == SessionState.Close)
            {
                Connect();
            }

            if (Session.State == SessionState.Connected)
            {
                Timer -= Time.deltaTime;
                if (Timer <= 0)
                {
                    Timer = HeartInterval;
                    Session.SendHeartAction();
                }
            }

            Action.HandlerUpdate();

            lock (invokeQueue)
            {
                while (invokeQueue.Count > 0)
                {
                    string action = invokeQueue.Dequeue();
                    switch (action)
                    {
                        case "OnConnectionSuccess": OnConnectionSuccess?.Invoke(); break;
                        case "OnConnectionFailed": OnConnectionFailed?.Invoke(); break;
                        case "OnConnectionBreaked": OnConnectionBreaked?.Invoke(); break;
                        default: break;
                    }
                }
            }

        }

        private void OnDestroy()
        {
            //游戏结束关闭套接字
            if (Session != null && Session.isUse)
            {
                Session.Close();
            }
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        public void Connect(string address, int port)
        {
            if (Session != null && Session.isUse) return;
            Host = address;
            Port = port;
            if (string.IsNullOrEmpty(Host) || Port == 0) return;
            Connect();
        }

        /// <summary>
        /// 绑定数据处理模块
        /// </summary>
        public void ActionAddListener(ActionTypeEnum actionType, HandleModule listener)
        {
            Action.AddListener(actionType, listener);
        }

        /// <summary>
        /// 移除绑定数据处理模块
        /// </summary>
        public void ActionRemoveListener(ActionTypeEnum actionType, HandleModule listener)
        {
            Action.RemoveListener(actionType, listener);
        }

        /// <summary>
        /// 发送指定类型处理消息
        /// </summary>
        public void SendAction(ActionTypeEnum actionType, ActionParameter parameter)
        {
            Session.SendAction(actionType, parameter);
        }

        internal void Connect()
        {
            //创建套接字
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Session.AsyncConnect(socket, Host, Port);

            //启动超时重连
            if (reconnectCoroutine != null) StopCoroutine(reconnectCoroutine);
            reconnectCoroutine = StartCoroutine(AsyncConnectTimeout());
        }

        /// <summary>
        /// 连接服务器超时重连机制(ms)
        /// </summary>
        /// <param name="timeoutLength"></param>
        /// <returns></returns>
        internal IEnumerator AsyncConnectTimeout()
        {
            //每帧检测套接字是否连接
            DateTime currentTime = DateTime.Now;
            while ((int)(DateTime.Now - currentTime).TotalMilliseconds < Timeout)
            {
                if (Session != null && Session.isUse) break;
                else yield return null;
            }

            //超时后套接字未连接则循环重连
            if (!Session.isUse)
            {
                Session.Close();
                Connect();
            }

            reconnectCoroutine = null;
        }

        /// <summary>
        /// 心跳监测
        /// </summary>
        internal void HeartPing(ActionParameter parameter)
        {
            Ping = parameter.GetValue<int>(NetConfig.PING);
            NetLog.Log($"Ping:{Ping}");
        }

        /// <summary>
        /// 客户端连接成功
        /// </summary>
        internal void OnClientConnectionSuccess()
        {
            lock (invokeQueue) invokeQueue.Enqueue("OnConnectionSuccess");
            NetLog.Log("客户端连接成功");
        }

        /// <summary>
        /// 客户端连接失败
        /// </summary>
        internal void OnClientConnectionFailed()
        {
            lock (invokeQueue) invokeQueue.Enqueue("OnConnectionFailed");
            NetLog.Log("客户端连接失败");
        }

        /// <summary>
        /// 客户端连接中断
        /// </summary>
        internal void OnClientConnectionBreaked()
        {
            lock (invokeQueue) invokeQueue.Enqueue("OnConnectionBreaked");
            NetLog.Log("客户端连接中断");
        }
    }
}