namespace Network.CommonData
{
    public static class NetConfig
    {
        public const int BUFFER_SIZE = 1024;// 缓冲区大小
        // ----------------------固定消息类型--------------------------
        public const string PING = "Ping";
        public const string MESSAGE = "Message"; 
        public const string TIMESTAMP = "Timestamp";
        public const string ACTIONTYPE = "ActionType";

        // -----自定义消息类型,与发送消息时的 Object声明 名称保持一致-----
        public const string OnlineList = "OnlineList";
    }

    public enum ActionTypeEnum
    {
        None = 0,
        HeartAction = 98,
        InformAction = 99,
        OnlineAction = 100,
        MessageAction = 101,
    }

    public static class NetLog
    {
        public static void Log(object message)
        {
            UnityEngine.Debug.Log(message.ToString());
        }
        public static void Warning(object message)
        {
            UnityEngine.Debug.LogWarning(message.ToString());
        }
        public static void Error(object message)
        {
            UnityEngine.Debug.LogError(message.ToString());
        }
    }
}