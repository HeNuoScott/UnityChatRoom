﻿
using Network.CommonData;

namespace Network.Client
{
    /// <summary>
    /// 处理基类
    /// </summary>
    public abstract class ClientActionBase
    {
        /// <summary>
        /// 处理类型
        /// </summary>
        public abstract ActionTypeEnum ActionType { get; }

        /// <summary>
        /// 数据包
        /// </summary>
        public DataPackage Packet { get; set; }

        /// <summary>
        /// 发送过程(将Parameter数据 装入Packet)
        /// </summary>
        public abstract bool SendProcess(ActionParameter parameter);

        /// <summary>
        /// 接收过程(提取Packet数据到Parameter)
        /// </summary>
        public abstract bool ReceiveProcess(ActionParameter parameter);

        /// <summary>
        /// 清理
        /// </summary>
        public virtual void Clean()
        {
            Packet = null;
        }
    }
}
