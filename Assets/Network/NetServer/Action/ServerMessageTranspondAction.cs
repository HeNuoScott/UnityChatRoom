using Network.CommonData;

namespace Network.Server
{
    /// <summary>
    /// 消息传输事件
    /// </summary>
    public class ServerMessageTranspondAction : ServerActionBase
    {
        public override ActionTypeEnum ActionType { get { return ActionTypeEnum.MessageAction; } }

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
            return new ServerMessageTranspondAction();
        }

        public override bool Process(ActionParameter parameter)
        {
            string message = Session.GetRemoteAddress() + ": " + parameter.GetValue<string>(NetConfig.MESSAGE);
            DynamicBuffer buffer = new DynamicBuffer(0);
            buffer.WriteValue(message);
            DataPackage packet = new DataPackage(buffer, ActionType);
            foreach (var session in ServiceSessionPool.GetOnlineSession())
            {
                session.Send(packet);
            }
            return true;
        }
    }
}
