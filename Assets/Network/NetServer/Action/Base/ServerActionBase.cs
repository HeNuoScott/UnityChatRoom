using Network.CommonData;
using System;

namespace Network.Server
{
    /// <summary>
    /// 处理基类
    /// </summary>
    public abstract class ServerActionBase : ICloneable
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
        /// 会话
        /// </summary>
        public ServiceSession Session { get; set; }

        /// <summary>
        /// 验证数据包(检测Packet中是否包含指定数据,如果包含就装入parameter)
        /// </summary>
        public abstract bool Check(ActionParameter parameter);

        /// <summary>
        /// 逻辑处理过程(建议把要发送的parameter重新封装一个DataPackage,并用Session发送)
        /// 不直接发送 Packet 是有可能ActionType要根据需要进行更改
        /// </summary>
        public abstract bool Process(ActionParameter parameter);

        /// <summary>
        /// 清理
        /// </summary>
        public virtual void Clean()
        {
            Packet = null;
        }

        public abstract object Clone();
    }
}
