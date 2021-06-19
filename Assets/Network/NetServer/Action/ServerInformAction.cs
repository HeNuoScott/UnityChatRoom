using Network.CommonData;

namespace Network.Server
{
    public class ServerInformAction : ServerActionBase
    {
        public override ActionTypeEnum ActionType { get { return ActionTypeEnum.InformAction; } }

        public override bool Check(ActionParameter parameter)
        {
            string message = null;
            if (Packet.Data.TryReadString(ref message))
            {
                parameter[NetConfig.MESSAGE] = message;
                return true;
            }
            return false;
        }

        public override object Clone()
        {
            return new ServerInformAction();
        }

        public override bool Process(ActionParameter parameter)
        {
            string message = "【系统通知】 " + parameter.GetValue<string>(NetConfig.MESSAGE);
            DynamicBuffer buffer = new DynamicBuffer(0);
            buffer.WriteValue(message);
            DataPackage packet = new DataPackage(buffer, ActionTypeEnum.InformAction);
            foreach (var session in ServiceSessionPool.GetOnlineSession())
            {
                session.Send(packet);
            }
            return true;
        }
    }
}
