using System;
using System.Net.Sockets;
using System.Collections.Generic;
using Network.CommonData;
using Network.Log;

namespace Network.NetServer
{
    /// <summary>
    /// 处理分发类
    /// </summary>
    public class SActionHandler
    {
        // 处理对象列表
        private Dictionary<int, SActionBase> actions;

        public SActionHandler(SessionClient session)
        {
            actions = SActionFactory.CreateAllAction(session);
        }

        /// <summary>
        /// 数据包处理过程
        /// </summary>
        public void Process(DataPackage packet)
        {
            if (!actions.ContainsKey(packet.PacketType))
                return;

            SActionParameter parameter = new SActionParameter();
            SActionBase handler = actions[packet.PacketType];
            handler.Packet = packet;
            if (handler.Check(parameter))
                if (!handler.Process(parameter))
                    NetLog.Log(NetLogLevel.Info, $"{packet.PacketType}请求处理失败");
            handler.Clean();
        }
    }
}
