
using Network.CommonData;

namespace Network.Client
{
    /// <summary>
    /// 消息转发
    /// </summary>
    public class ClientMessageTranspondAction : ClientActionBase
    {
        public override ActionTypeEnum ActionType => ActionTypeEnum.MessageAction;

        public override void Clean()
        {
            base.Clean();
        }

        public override bool ReceiveProcess(ActionParameter parameter)
        {
            string message = null;
            if (Packet.Data.TryReadString(ref message))
            {
                parameter[NetConfig.MESSAGE] = message;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool SendProcess(ActionParameter parameter)
        {
            string message = parameter.GetValue<string>(NetConfig.MESSAGE);
            if (string.IsNullOrEmpty(message)) return false;

            Packet.Data.WriteValue(message);
            return true;
        }
    }
}
