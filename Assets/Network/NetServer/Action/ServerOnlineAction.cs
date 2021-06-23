using System.Collections.Generic;
using Network.CommonData;

namespace Network.Server
{
    /// <summary>
    /// 请求在线玩家列表
    /// </summary>
    public class ServerOnlineAction : ServerActionBase
    {
        public override ActionTypeEnum ActionType { get { return ActionTypeEnum.OnlineAction; } }

        /// <summary>
        /// 参数可为空 只接认为接受数据正常
        /// </summary>
        public override bool ReceiveCheck(ActionParameter parameter)
        {
            return true;
        }

        public override object Clone()
        {
            return new ServerOnlineAction();
        }

        public override bool Process(ActionParameter parameter)
        {
            List<string> OnlineList = new List<string>();
            foreach (var session in ServiceSessionPool.GetOnlineSession())
            {
                OnlineList.Add(session.GetRemoteAddress());
            }
            DynamicBuffer buffer = new DynamicBuffer(0);
            buffer.WriteObject(OnlineList);
            DataPackage packet = new DataPackage(buffer, ActionType);
            Session.Send(packet);
            return true;
        }
    }
}
