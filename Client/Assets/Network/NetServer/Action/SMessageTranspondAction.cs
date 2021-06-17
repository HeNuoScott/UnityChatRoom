using Network.Data;

namespace Network.NetServer
{
    public class SMessageTranspondAction : SActionBase
    {
        private const int ACTIONTYPE = 1001;

        public override int ActionType { get { return ACTIONTYPE; } }

        public override bool Check(SActionParameter parameter)
        {
            string message = null;
            if (Packet.Data.TryReadString(ref message))
            {
                parameter["message"] = message;
                return true;
            }
            return false;
        }

        public override object Clone()
        {
            return new SMessageTranspondAction();
        }

        public override bool Process(SActionParameter parameter)
        {
            string message = Session.GetRemoteAddress() + ": " + parameter.GetValue<string>("message");
            DynamicBuffer buffer = new DynamicBuffer(0);
            buffer.WriteValue(message);
            DataPackage packet = new DataPackage(buffer, ActionType);
            foreach (var session in SessionClientPool.GetOnlineSession())
            {
                session.Send(packet);
            }
            return true;
        }
    }
}
