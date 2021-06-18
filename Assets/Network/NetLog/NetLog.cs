namespace Network.Log
{
    //
    // 摘要:
    //     游戏框架日志等级。
    public enum NetLogLevel : byte
    {
        //
        // 摘要:
        //     信息。
        Info = 1,
        //
        // 摘要:
        //     警告。
        Warning = 2,
        //
        // 摘要:
        //     错误。
        Error = 3,
    }
    public static class NetLog
    {
        /// <summary>
        /// 记录日志。
        /// </summary>
        /// <param name="level">日志等级。</param>
        /// <param name="message">日志内容。</param>
        public static void Log(NetLogLevel level, object message)
        {
            switch (level)
            {
                case NetLogLevel.Info:
                    UnityEngine.Debug.Log(message.ToString());
                    break;

                case NetLogLevel.Warning:
                    UnityEngine.Debug.LogWarning(message.ToString());
                    break;

                case NetLogLevel.Error:
                    UnityEngine.Debug.LogError(message.ToString());
                    break;
            }
        }
    }
}
