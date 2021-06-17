using System.Collections.Generic;

namespace Network.NetClient
{
    /// <summary>
    /// 消息转发
    /// </summary>
    public class COnlineAction : CActionBase
    {
        private const int ACTIONTYPE = 100;

        public override int ActionType { get { return ACTIONTYPE; } }

        public override void Clean()
        {
            base.Clean();
        }

        public override bool ReceiveProcess(CActionParameter parameter)
        {
            List<string> onlineList = null;
            if (Packet.Data.TryReadObject(ref onlineList))
            {
                parameter["onlineList"] = onlineList;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool SendProcess(CActionParameter parameter)
        {
            return true;
        }
    }
}
