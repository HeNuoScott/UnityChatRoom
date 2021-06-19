using Network.CommonData;

namespace Network.Server
{
    /// <summary>
    /// ÐÄÌøÊÂ¼þ
    /// </summary>
    public class ServerHeartAction : ServerActionBase
    {
        public override ActionTypeEnum ActionType => ActionTypeEnum.HeartAction;

        public override bool Check(ActionParameter parameter)
        {
            string timestamp = null;
            if (Packet.Data.TryReadString(ref timestamp))
            {
                parameter[NetConfig.TIMESTAMP] = timestamp;
                return true;
            }
            return false;
        }

        public override object Clone()
        {
            return new ServerHeartAction();
        }

        public override bool Process(ActionParameter parameter)
        {
            string timestamp = parameter.GetValue<string>(NetConfig.TIMESTAMP);
            DynamicBuffer buffer = new DynamicBuffer(0);
            buffer.WriteValue(timestamp);
            DataPackage packet = new DataPackage(buffer, ActionType);
            Session.Send(packet);
            return true;
        }
    }
}