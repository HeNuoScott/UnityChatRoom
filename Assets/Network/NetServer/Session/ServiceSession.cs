﻿using System.Net.Sockets;
using Network.CommonData;
using System;

namespace Network.Server
{
    public class ServiceSession
    {
        // 读数据缓冲区大小
        private byte[] readBuffer = new byte[NetConfig.BUFFER_SIZE];

        // 套接字
        private Socket socket;

        // 动态缓冲区
        private DynamicBuffer dynamicBuffer = new DynamicBuffer(NetConfig.BUFFER_SIZE);

        /// <summary>
        /// 是否使用
        /// </summary>
        public bool isUse = false;

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(Socket socket, int receiveTimeout = 1000)
        {
            this.socket = socket;
            this.socket.ReceiveTimeout = receiveTimeout;
            isUse = true;

            socket.BeginReceive(readBuffer, 0, readBuffer.Length, SocketFlags.None, ReceiveCallBack, new ServerActionHandler(this));
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
                NetLog.Error(e.Message);
            }
        }

        /// <summary>
        /// 获取远程套接字地址
        /// </summary>
        public string GetRemoteAddress()
        {
            if (!isUse) return null;

            return socket.RemoteEndPoint.ToString();
        }

        /// <summary>
        /// 获取本地套接字地址
        /// </summary>
        public string GetLocalAddress()
        {
            if (!isUse) return null;

            return socket.LocalEndPoint.ToString();
        }

        /// <summary>
        /// 关闭套接字
        /// </summary>
        public void Close()
        {
            if (!isUse) return;

            socket.Close();
            dynamicBuffer.Clear();
            isUse = false;
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
                    NetServer.Instance.OnServiceSessionOffline(GetRemoteAddress());
                    Close();
                    return;
                }

                dynamicBuffer.WriteBytes(readBuffer, count);

                //数据解析
                ServerActionHandler actionHandler = asyncResult.AsyncState as ServerActionHandler;
                DataPackage packet;
                while ((packet = dynamicBuffer.UnPack()) != null)
                {
                    actionHandler.Process(packet);
                }

                socket.BeginReceive(readBuffer, 0, readBuffer.Length, SocketFlags.None, ReceiveCallBack, actionHandler);
            }
            catch (Exception)
            {
                NetServer.Instance.OnServiceSessionDisconnect(GetRemoteAddress());
                Close();
            }
        }
    }
}
