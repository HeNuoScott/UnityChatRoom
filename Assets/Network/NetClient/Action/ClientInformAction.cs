using Network.CommonData;
using System.Collections.Generic;

namespace Network.Client
{
    public class ClientInformAction : ClientActionBase
    {
        public override ActionTypeEnum ActionType => ActionTypeEnum.InformAction;

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
            return true;
        }
    }
}