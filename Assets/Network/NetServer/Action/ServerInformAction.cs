using Network.CommonData;

namespace Network.Server
{
    public class ServerInformAction : ServerActionBase
    {
        public override ActionTypeEnum ActionType { get { return ActionTypeEnum.InformAction; } }

        /// <summary>
        /// 系统消息 一般由服务器只接下发 服务器不接收这类消息
        /// </summary>
        public override bool ReceiveCheck(ActionParameter parameter)
        {
            return true;
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
