using Network.CommonData;
using System;

namespace Network.Client
{
    /// <summary>
    /// �����¼� ���Ҽ���Ping
    /// </summary>
    public class ClientHeartAction : ClientActionBase
    {
        public override ActionTypeEnum ActionType => ActionTypeEnum.HeartAction;

        public override bool ReceiveProcess(ActionParameter parameter)
        {
            string timestamp = null;
            if (Packet.Data.TryReadString(ref timestamp))
            {
                parameter[NetConfig.TIMESTAMP] = timestamp;
                long sendtime = long.Parse(timestamp);
                long curtime = GetTime();
                parameter[NetConfig.PING] = (int)(curtime - sendtime);
                return true;
            }
            return false;
        }

        public override bool SendProcess(ActionParameter parameter)
        {
            string timestamp = GetTime().ToString();
            Packet.Data.WriteValue(timestamp);
            return true;
        }

        private long GetTime()
        {
            DateTime startTime = TimeZoneInfo.ConvertTime(new System.DateTime(1970, 1, 1), TimeZoneInfo.Utc, TimeZoneInfo.Local);  // ����ʱ��
            long timeStamp = (long)(DateTime.Now - startTime).TotalMilliseconds; // ��������
            return timeStamp;
        }
    }
}