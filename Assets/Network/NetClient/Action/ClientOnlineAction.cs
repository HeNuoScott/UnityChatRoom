using Network.CommonData;
using System.Collections.Generic;

namespace Network.Client
{
    /// <summary>
    /// 客户端上线消息,接收在线人员名单列表
    /// </summary>
    public class ClientOnlineAction : ClientActionBase
    {
        public override ActionTypeEnum ActionType => ActionTypeEnum.OnlineAction;

        public override bool ReceiveProcess(ActionParameter parameter)
        {
            List<string> onlineList = null;
            if (Packet.Data.TryReadObject(ref onlineList))
            {
                parameter[NetConfig.OnlineList] = onlineList;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool SendProcess(ActionParameter parameter)
        {
            return true;
        }
    }
}
