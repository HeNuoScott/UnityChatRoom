using System.Collections;
using System.Net.Sockets;
using UnityEngine;
using System;
using Network.CommonData;

namespace Network.Client
{
    /// <summary>
    /// 网络客户端
    /// </summary>
    public class NetClient : MonoBehaviour
    {
        private static NetClient instance;
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

        //会话对象
        public ClientSession Session { get; private set; }

        //消息事件处理助手
        public ClientActionHandler Action { get; private set; }

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

            if (Session.State == SessionState.Run)
            {
                Timer -= Time.deltaTime;
                if (Timer <= 0)
                {
                    Timer = HeartInterval;
                    Session.SendHeartAction();
                }
            }

            Action.Update();
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

        private void Connect()
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
        private IEnumerator AsyncConnectTimeout()
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

        private void HeartPing(ActionParameter parameter)
        {
            Ping = parameter.GetValue<int>(NetConfig.PING);
            NetLog.Log($"Ping:{Ping}");
        }
    }
}