
namespace Network.NetClient
{
    /// <summary>
    /// 消息转发
    /// </summary>
    public class CMessageTranspondAction : CActionBase
    {
        private const int ACTIONTYPE = 1001;

        public override int ActionType { get { return ACTIONTYPE; } }

        public override void Clean()
        {
            base.Clean();
        }

        public override bool ReceiveProcess(CActionParameter parameter)
        {
            string message = null;
            if (Packet.Data.TryReadString(ref message))
            {
                parameter["message"] = message;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool SendProcess(CActionParameter parameter)
        {
            string message = parameter.GetValue<string>("message");
            if (string.IsNullOrEmpty(message))
                return false;

            Packet.Data.WriteValue(message);
            return true;
        }
    }
}
