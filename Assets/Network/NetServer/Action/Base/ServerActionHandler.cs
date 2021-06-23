using System.Collections.Generic;
using Network.CommonData;

namespace Network.Server
{
    /// <summary>
    /// 处理分发类
    /// </summary>
    public class ServerActionHandler
    {
        // 处理对象列表
        private Dictionary<ActionTypeEnum, ServerActionBase> actions;

        public ServerActionHandler(ServiceSession session)
        {
            actions = ServerActionFactory.CreateAllAction(session);
        }

        /// <summary>
        /// 数据包处理过程
        /// </summary>
        public void Process(DataPackage packet)
        {
            if (!actions.ContainsKey(packet.PacketType)) return;

            ActionParameter parameter = new ActionParameter();
            ServerActionBase handler = actions[packet.PacketType];

            handler.Packet = packet;
            if (handler.ReceiveCheck(parameter))
            {
                if (!handler.Process(parameter))
                {
                    NetLog.Log($"{packet.PacketType}请求处理失败");
                }
            }

            handler.Clean();
        }
    }
}
