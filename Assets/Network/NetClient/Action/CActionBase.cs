
using Network.CommonData;

namespace Network.NetClient
{
    /// <summary>
    /// 处理基类
    /// </summary>
    public abstract class CActionBase
    {
        /// <summary>
        /// 处理类型
        /// </summary>
        public abstract int ActionType { get; }

        /// <summary>
        /// 数据包
        /// </summary>
        public DataPackage Packet { get; set; }

        /// <summary>
        /// 发送过程
        /// </summary>
        public abstract bool SendProcess(CActionParameter parameter);

        /// <summary>
        /// 接收过程
        /// </summary>
        public abstract bool ReceiveProcess(CActionParameter parameter);

        /// <summary>
        /// 清理
        /// </summary>
        public virtual void Clean()
        {
            Packet = null;
        }
    }
}
