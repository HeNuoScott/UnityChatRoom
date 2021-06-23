using Network.CommonData;

namespace Network.Server
{
    /// <summary>
    /// 心跳事件
    /// </summary>
    public class ServerHeartAction : ServerActionBase
    {
        public override ActionTypeEnum ActionType => ActionTypeEnum.HeartAction;
        /// <summary>
        /// 接收消息检测
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public override bool ReceiveCheck(ActionParameter parameter)
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