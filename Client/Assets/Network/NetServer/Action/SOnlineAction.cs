using System.Collections.Generic;
using Network.Data;

namespace Network.NetServer
{
    public class SOnlineAction : SActionBase
    {
        private const int ACTIONTYPE = 100;

        public override int ActionType { get { return ACTIONTYPE; } }

        public override bool Check(SActionParameter parameter)
        {
            return true;
        }

        public override object Clone()
        {
            return new SOnlineAction();
        }

        public override bool Process(SActionParameter parameter)
        {
            List<string> onlineList = new List<string>();
            foreach (var session in SessionClientPool.GetOnlineSession())
            {
                onlineList.Add(session.GetRemoteAddress());
            }
            DynamicBuffer buffer = new DynamicBuffer(0);
            buffer.WriteObject(onlineList);
            DataPackage packet = new DataPackage(buffer, 100);
            Session.Send(packet);
            return true;
        }
    }
}
