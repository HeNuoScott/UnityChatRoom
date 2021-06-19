using System.Net.Sockets;
using Network.CommonData;
using UnityEngine;
using System;

namespace Network.Client
{
    /// <summary>
    /// 会话状态
    /// </summary>
    public enum SessionState
    {
        None,
        Connect,
        Run,
        Close
    }

    /// <summary>
    /// 会话客户端类
    /// </summary>
    public class ClientSession
    {
        /// <summary>
        /// 读数据缓冲区大小
        /// </summary>
        private byte[] readBuffer = new byte[NetConfig.BUFFER_SIZE];

        /// <summary>
        /// 套接字
        /// </summary>
        private Socket socket;

        /// <summary>
        /// 动态缓冲区
        /// </summary>
        private DynamicBuffer dynamicBuffer = new DynamicBuffer(NetConfig.BUFFER_SIZE);

        /// <summary>
        /// 请求处理
        /// </summary>
        private ClientActionHandler actionHandler = null;

        /// <summary>
        /// 是否使用
        /// </summary>
        public bool isUse = false;

        public SessionState State { get; set; }
        public int ReceiveTimeout { set { socket.ReceiveTimeout = value; } }
        public int SendTimeout { set { socket.SendTimeout = value; } }

        public ClientSession(ClientActionHandler clientActionHandler)
        {
            actionHandler = clientActionHandler;
        }

        /// <summary>
        /// 启动套接字异步接收
        /// </summary>
        /// <param name="socket"></param>
        public void AsyncReceive(Socket socket)
        {
            State = SessionState.Run;
            this.socket = socket;
            isUse = true;
            socket.BeginReceive(readBuffer, 0, readBuffer.Length, SocketFlags.None, ReceiveCallBack, null);
        }

        /// <summary>
        /// 启动套接字异步连接
        /// </summary>
        public void AsyncConnect(Socket socket, string host, int port)
        {
            State = SessionState.Connect;
            this.socket = socket;
            socket.BeginConnect(host, port, ConnectCallBack, this.socket);
        }

        /// <summary>
        /// 发送数据包
        /// </summary>
        public void Send(DataPackage packet)
        {
            try
            {
                socket.Send(packet.Pack());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 发送指定类型处理消息
        /// </summary>
        public void SendAction(ActionTypeEnum actionType, ActionParameter parameter)
        {
            DataPackage packet = actionHandler.SendProcess(actionType, parameter);
            if (packet != null) Send(packet);
        }

        /// <summary>
        /// 发送心跳事件 并且监听Ping
        /// </summary>
        public void SendHeartAction()
        {
            DataPackage packet = actionHandler.SendProcess(ActionTypeEnum.HeartAction, null);
            if (packet != null) Send(packet);
        }

        /// <summary>
        /// 获取远程套接字地址
        /// </summary>
        public string GetRemoteAddress()
        {
            if (!isUse)
                return null;

            return socket.RemoteEndPoint.ToString();
        }

        /// <summary>
        /// 获取本地套接字地址
        /// </summary>
        public string GetLocalAddress()
        {
            if (!isUse)
                return null;

            return socket.LocalEndPoint.ToString();
        }

        /// <summary>
        /// 关闭套接字
        /// </summary>
        public void Close()
        {
            if (!isUse) return;
            State = SessionState.Close;
            socket.Close();
            dynamicBuffer.Clear();
            isUse = false;
        }

        /// <summary>
        /// 异步连接回调
        /// </summary>
        private void ConnectCallBack(IAsyncResult asyncResult)
        {
            Socket socket = (Socket)asyncResult.AsyncState;
            socket.EndConnect(asyncResult);

            //开始异步接收数据
            AsyncReceive(socket);
        }

        /// <summary>
        /// 异步接收回调
        /// </summary>
        private void ReceiveCallBack(IAsyncResult asyncResult)
        {
            try
            {
                int count = socket.EndReceive(asyncResult);
                if (count <= 0)
                {
                    Debug.Log("服务器断开连接");
                    Close();
                    return;
                }

                dynamicBuffer.WriteBytes(readBuffer, count);

                //数据解析
                DataPackage packet;
                while ((packet = dynamicBuffer.UnPack()) != null)
                {
                    actionHandler.DisposeProcess(packet);
                }

                socket.BeginReceive(readBuffer, 0, readBuffer.Length, SocketFlags.None, ReceiveCallBack, null);
            }
            catch (Exception)
            {
                Close();
            }
        }
    }
}
